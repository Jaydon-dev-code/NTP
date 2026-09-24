using System;
using System.Threading;
using System.Threading.Tasks;
using SqlSugar;
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection
{
    /// <summary>
    /// SPC 当前生产型号同步服务（隶属 SPC 采集服务组）。
    ///
    /// 设计：不落本地缓存表 — 后台按 ThirdPartyDb_SyncIntervalSeconds 不间断轮询三方库，
    /// 结果仅保存在进程内存（ModelNo / SyncStatus / LastError / LastSyncTime）。
    /// 解析器与接口通过 CurrentModel 属性读内存；型号变化时递增 ModelVersion 失效解析缓存。
    ///
    /// 控制模型：
    ///   - 服务列表可独立 启动/停止/重启/启用禁用
    ///   - 启动 SPC 采集（LoadAsync）时若 IsEnabled 则 EnsureStarted 联动拉起
    ///   - 停止 SPC 采集不强制停本服务；批量「停止全部」一起停
    ///
    /// 失败策略：stale-while-error — 查库失败保留上次内存中的 ModelNo，仅标记 Error。
    /// </summary>
    public class CurrentModelSyncService
    {
        /// <summary>保护 CTS/循环任务/内存型号快照的锁</summary>
        private readonly object _startLock = new object();

        private CancellationTokenSource _cts;
        private Task _loop;

        /// <summary>型号变更版本号（ModelNo 真正变化时 +1）</summary>
        private long _modelVersion;

        /// <summary>内存中的当前型号快照（无缓存表，仅进程内）</summary>
        private string _modelNo;
        private string _syncStatus = "Unknown";
        private string _lastError;
        private DateTime? _lastSyncTime;

        /// <summary>服务列表「启用」开关；默认 true</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>轮询循环是否在跑</summary>
        public bool IsRunning
        {
            get
            {
                lock (_startLock)
                {
                    return _loop != null && !_loop.IsCompleted;
                }
            }
        }

        /// <summary>型号变更版本号；解析缓存用它做失效</summary>
        public long ModelVersion => Interlocked.Read(ref _modelVersion);

        /// <summary>内存当前型号（解析器/Manager 高频读，不打三方库）</summary>
        public string CurrentModelNo
        {
            get { lock (_startLock) { return _modelNo; } }
        }

        /// <summary>独立启动（服务列表 / SPC 联动）</summary>
        public void Start()
        {
            if (!IsEnabled) return;
            EnsureStarted();
        }

        /// <summary>独立停止；不影响 SPC 采集 Worker</summary>
        public void Stop()
        {
            lock (_startLock)
            {
                if (_cts == null) return;
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
                _loop = null;
            }
        }

        /// <summary>幂等启动：校验 IsEnabled → ThirdPartyDb_Enabled → IsConfigured 后开轮询循环</summary>
        public void EnsureStarted()
        {
            if (!IsEnabled) return;
            if (!ThirdPartyDbConfig.Enabled)
            {
                Serilog.Log.Warning("[型号同步] ThirdPartyDb_Enabled=false，跳过同步");
                return;
            }
            if (!ThirdPartyDbConfig.IsConfigured)
            {
                Serilog.Log.Warning("[型号同步] 连接串/型号SQL未配置，跳过同步");
                return;
            }

            lock (_startLock)
            {
                if (_loop != null && !_loop.IsCompleted) return;
                _cts = new CancellationTokenSource();
                var token = _cts.Token;
                _loop = Task.Run(() => LoopAsync(token));
            }
        }

        /// <summary>
        /// 不间断轮询：启动立即查一次，之后每 SyncIntervalSeconds 查一次三方库。
        /// 只更新内存，不写任何本地表。
        /// </summary>
        private async Task LoopAsync(CancellationToken token)
        {
            await SafeSyncOnceAsync();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(ThirdPartyDbConfig.SyncIntervalSeconds), token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                await SafeSyncOnceAsync();
            }
        }

        private async Task SafeSyncOnceAsync()
        {
            try
            {
                await SyncOnceAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning("[型号同步] 同步异常: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// 立即查一次三方库并更新内存快照（API「立即同步」/ 轮询循环调用）。
        /// 成功：更新 ModelNo（变化则 ModelVersion++）与 Ok；失败：保留旧 ModelNo，标 Error。
        /// </summary>
        public async Task SyncOnceAsync()
        {
            if (!ThirdPartyDbConfig.Enabled || !ThirdPartyDbConfig.IsConfigured)
                return;

            string modelNo = null;
            string error = null;
            var ok = false;

            try
            {
                modelNo = await ReadThirdPartyModelNoAsync();
                ok = !string.IsNullOrWhiteSpace(modelNo);
                if (!ok) error = "三方库返回空型号";
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            lock (_startLock)
            {
                if (ok)
                {
                    var trimmed = modelNo.Trim();
                    var changed = !string.Equals(_modelNo, trimmed, StringComparison.Ordinal);
                    _modelNo = trimmed;
                    _syncStatus = "Ok";
                    _lastError = null;
                    if (changed)
                    {
                        Interlocked.Increment(ref _modelVersion);
                        Serilog.Log.Information("[型号同步] 当前生产型号变更: {ModelNo}", _modelNo);
                    }
                }
                else
                {
                    // 失败：保留上次成功型号
                    _syncStatus = "Error";
                    _lastError = error;
                }

                _lastSyncTime = DateTime.Now;
            }
        }

        /// <summary>读取内存型号快照（供接口展示；不打三方库）</summary>
        public SPCCurrentModelInfo GetCurrentModel()
        {
            lock (_startLock)
            {
                return new SPCCurrentModelInfo
                {
                    ModelNo = _modelNo,
                    SyncStatus = _syncStatus,
                    LastError = _lastError,
                    LastSyncTime = _lastSyncTime,
                };
            }
        }

        /// <summary>兼容旧调用：返回内存型号字符串</summary>
        public Task<string> GetCurrentModelAsync()
        {
            return Task.FromResult(CurrentModelNo);
        }

        /// <summary>连三方库执行 CurrentModelSql，取第一行第一列（每次新建连接，低频轮询）</summary>
        private async Task<string> ReadThirdPartyModelNoAsync()
        {
            var sql = ThirdPartyDbConfig.CurrentModelSql.Trim().TrimEnd(';');
            var db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = ThirdPartyDbConfig.ConnectionString,
                DbType = ThirdPartyDbConfig.GetDbType(),
                IsAutoCloseConnection = true,
            });

            var obj = await db.Ado.GetScalarAsync(sql);
            return obj?.ToString();
        }
    }
}
