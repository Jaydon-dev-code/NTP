using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection.DataCollection;
using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage.SPC;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.SPC;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar.Extensions;

namespace SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection
{
    /// <summary>
    /// SPC 采集管理服务 — 从 SPC 特性表加载采集任务，Worker 大循环统一消费子项。
    ///
    /// 调度模型（同 StationCollectionManager）：
    /// 1. LoadAsync：联动启动型号同步 → 从特性表加载 → 按 CollectionMethod 反射创建采集器
    ///    → 初始解析（当前型号→零件）并 SetContext → 入队 → StartWorkers
    /// 2. StartWorkers 启动 N 个 Worker（= CPU 核数），每个循环「抢」一个到期子项执行
    /// 3. Execute 前做换型感知：ModelVersion/型号变化则重解析并再次 SetContext
    /// 4. 启动/停止只是改 IsRunning 标记，Worker 自动接管或放弃
    ///
    /// 与型号同步的关系：
    ///   - LoadAsync 时若 modelSync.IsEnabled 则 EnsureStarted（一起拉起）
    ///   - StopWorkers 不强制停型号同步（同步可独立控制；批量停全部时由 ServiceController 一起停）
    /// </summary>
    public class SPCDataCollectionManager
    {
        private readonly ConcurrentDictionary<int, SPCRuntime> _tasks =
            new ConcurrentDictionary<int, SPCRuntime>();

        private readonly Tb_SPC_CharacteristicRepository _characteristicRepo;
        private readonly Tb_SPC_CollectionSourceRepository _collectionSourceRepo;
        private readonly Tb_SPC_PlcPointRepository _plcPointRepo;
        private readonly Tb_PlcPointRepository _plcPointOrigRepo;
        private readonly Tb_PlcConnectionRepository _plcConnectionRepo;

        /// <summary>三方当前型号同步服务（隶属本 SPC 服务组）</summary>
        private readonly CurrentModelSyncService _modelSyncService;

        /// <summary>型号→产品→零件 解析器</summary>
        private readonly CurrentModelResolver _modelResolver;

        private readonly ILifetimeScope _scope;

        private readonly object _scheduleLock = new object();
        private CancellationTokenSource _workersCts;
        private Task[] _workers;
        private readonly int _workerCount;

        public int WorkerCount => _workerCount;
        public bool WorkersRunning => _workers != null && _workers.Any(w => !w.IsCompleted);
        public bool IsEnabled { get; set; } = true;

        public SPCDataCollectionManager(
            Tb_SPC_CharacteristicRepository characteristicRepo,
            Tb_SPC_CollectionSourceRepository collectionSourceRepo,
            Tb_SPC_PlcPointRepository plcPointRepo,
            Tb_PlcPointRepository plcPointOrigRepo,
            Tb_PlcConnectionRepository plcConnectionRepo,
            CurrentModelSyncService modelSyncService,
            CurrentModelResolver modelResolver,
            ILifetimeScope scope
        )
        {
            _characteristicRepo = characteristicRepo;
            _collectionSourceRepo = collectionSourceRepo;
            _plcPointRepo = plcPointRepo;
            _plcPointOrigRepo = plcPointOrigRepo;
            _plcConnectionRepo = plcConnectionRepo;
            _modelSyncService = modelSyncService;
            _modelResolver = modelResolver;
            _scope = scope;
            _workerCount = Math.Max(1, Environment.ProcessorCount);
        }

        #region 加载与初始化

        /// <summary>
        /// 加载全部特性为采集任务并启动 Worker。
        /// 服务列表「启动 SPC 采集管理服务」/ Reload 调用。
        /// </summary>
        public async Task LoadAsync()
        {
            // 型号同步隶属 SPC：若未禁用则随 SPC 采集一并启动（幂等，不覆盖用户单独停止）
            try
            {
                //if (_modelSyncService.IsEnabled)
                //    _modelSyncService.EnsureStarted();
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning("[SPC采集] 启动型号同步失败: {Message}", ex.Message);
            }

            var characteristics = await _characteristicRepo.GetListAsync();
            foreach (var chara in characteristics)
            {
                try { await AddOrUpdateAsync(chara); }
                catch (Exception ex)
                {
                    Serilog.Log.Warning("[SPC采集]加载特性失败: {Name}({Id}) {Message}", chara.Name, chara.Id, ex.Message);
                }
            }

            // 加载完成后启动 Worker（幂等）
            StartWorkers();
        }

        /// <summary>
        /// 新增/更新单个采集任务：反射创建采集器 → Init → 初始解析零件上下文 → 入队。
        /// 特性配置变更后 Reload 会对每个特性调用一次。
        /// </summary>
        public async Task<bool> AddOrUpdateAsync(Tb_SPC_Characteristic characteristic)
        {
            // 按 CollectionMethod 类名反射找到采集器类型（DBMeasureResult / PlcCollectStatus…）
            var collectorType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.IsClass && !t.IsAbstract
                    && typeof(SPCDataCollectionAbstract).IsAssignableFrom(t)
                    && t.Name == characteristic.CollectionMethod);

            if (collectorType == null)
            {
                Serilog.Log.Warning("[SPC采集]未找到采集方式实现: {CollectionMethod}", characteristic.CollectionMethod);
                return false;
            }

            var collector = _scope.Resolve(collectorType) as SPCDataCollectionAbstract;
            if (collector == null) return false;

            // 按数据来源组装 Init 参数：PLC → 点位列表；数据库 → CollectionSource 配置行
            object[] initArgs;
            if (characteristic.DataSource == SPCDataSource.PLC)
            {
                var spcPlcList = await _plcPointRepo.GetAllByCharacteristicIdAsync(characteristic.Id);
                if (spcPlcList == null || spcPlcList.Count == 0) return false;

                var devPoints = new List<Models.Dtos.DevPlcPointDto>();
                foreach (var spcPlc in spcPlcList)
                {
                    if (spcPlc.PlcPointId > 0)
                    {
                        // 引用模式：查 Tb_PlcPoint + Tb_PlcConnection
                        var plcPoint = await _plcPointOrigRepo.QueryableFirstAsync(p => p.Id == spcPlc.PlcPointId);
                        if (plcPoint == null) continue;
                        var conn = await _plcConnectionRepo.QueryableFirstAsync(c => c.Id == plcPoint.PlcConnectionId);
                        devPoints.Add(new Models.Dtos.DevPlcPointDto
                        {
                            DeviceName = characteristic.Name,
                            PointName = plcPoint.PointName,
                            IpAddress = conn?.IpAddress ?? "127.0.0.1",
                            Port = conn?.Port ?? 8000,
                            Prefix = plcPoint.Area,
                            Address = plcPoint.Address,
                            DataType = plcPoint.DataType.ToTypeCode(),
                            Length = plcPoint.Length,
                            ReadFormula = plcPoint.ReadFormula ?? "",
                            WriteFormula = plcPoint.WriteFormula ?? "",
                        });
                    }
                    else
                    {
                        // 手动录入模式：用 Tb_SPC_PlcPoint 内联字段
                        devPoints.Add(new Models.Dtos.DevPlcPointDto
                        {
                            DeviceName = characteristic.Name,
                            PointName = spcPlc.PointName ?? "",
                            IpAddress = spcPlc.IpAddress ?? "127.0.0.1",
                            Port = spcPlc.Port ?? 8000,
                            Prefix = spcPlc.Area ?? "",
                            Address = spcPlc.Address ?? "",
                            DataType = ParseTypeCode(spcPlc.DataType),
                            Length = spcPlc.Length ?? 1,
                            ReadFormula = "",
                            WriteFormula = "",
                        });
                    }
                }
                if (devPoints.Count == 0) return false;
                initArgs = new object[] { characteristic.Name, devPoints };
            }
            else
            {
                var source = await _collectionSourceRepo.GetByCharacteristicIdAsync(characteristic.Id);
                initArgs = new object[] { characteristic.Name, source };
            }

            collector.Init(initArgs);

            // 初始解析：当前型号 → 产品 → 零件；结果注入采集器供 Execute 消费
            SPCResolvedContext resolved = null;
            long modelVersion = 0;
            try
            {
                resolved = await _modelResolver.ResolveAsync(characteristic);
                modelVersion = _modelSyncService.ModelVersion;
                collector.SetContext(resolved);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning("[SPC采集] 特性 {Name}({Id}) 初始解析失败: {Message}",
                    characteristic.Name, characteristic.Id, ex.Message);
            }

            // 入队运行时状态；记录解析快照与版本，供换型比对
            _tasks[characteristic.Id] = new SPCRuntime
            {
                CharacteristicId = characteristic.Id,
                Collector = collector,
                Characteristic = characteristic,
                Resolved = resolved,
                LastResolvedModelNo = resolved?.ModelNo,
                LastModelVersion = modelVersion,
                IsRunning = false,
                IsEnabled = true,
                LastRun = DateTime.MinValue,
                Interval = TimeSpan.FromSeconds(5),
            };
            return true;
        }

        public bool Remove(int characteristicId)
        {
            return _tasks.TryRemove(characteristicId, out _);
        }

        #endregion

        #region Worker 大循环

        public void StartWorkers()
        {
            lock (_scheduleLock)
            {
                if (_workers != null && _workers.Any(w => !w.IsCompleted)) return;
                _workersCts = new CancellationTokenSource();
                _workers = new Task[_workerCount];
                for (int i = 0; i < _workerCount; i++)
                {
                    int idx = i;
                    _workers[idx] = Task.Run(() => WorkerLoopAsync(_workersCts.Token));
                }
            }
        }

        public void StopWorkers()
        {
            lock (_scheduleLock)
            {
                if (_workersCts != null)
                {
                    _workersCts.Cancel();
                    _workersCts.Dispose();
                    _workersCts = null;
                }
                _workers = null;
            }
            // 型号同步独立控制：SPC 停止不强制停同步；由服务列表单独停或 StopAll 联动
        }

        private async Task WorkerLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                SPCDataCollectionAbstract collector = null;
                int charId = 0;

                lock (_scheduleLock)
                {
                    var candidate = _tasks.Values
                        .Where(rt => rt.IsRunning && rt.IsEnabled && !rt.InProgress
                            && DateTime.UtcNow - rt.LastRun >= rt.Interval)
                        .OrderBy(rt => rt.LastRun)
                        .FirstOrDefault();
                    if (candidate != null)
                    {
                        candidate.InProgress = true;
                        collector = candidate.Collector;
                        charId = candidate.CharacteristicId;
                    }
                }

                if (collector == null)
                {
                    try { await Task.Delay(250, token); }
                    catch (OperationCanceledException) { break; }
                    continue;
                }

                // 换型感知：型号/版本变化时重新解析并 SetContext
                await RefreshContextIfModelChangedAsync(charId);

                try { collector.Execute(); }
                catch (Exception ex)
                {
                    Serilog.Log.Warning("[SPC采集]【{Name}】执行异常: {Message}", collector.FeatureName, ex.Message);
                }
                finally
                {
                    lock (_scheduleLock)
                    {
                        if (_tasks.TryGetValue(charId, out var rt))
                        {
                            rt.LastRun = DateTime.UtcNow;
                            rt.InProgress = false;
                        }
                    }
                }
            }
        }

        private async Task RefreshContextIfModelChangedAsync(int characteristicId)
        {
            if (!_tasks.TryGetValue(characteristicId, out var rt) || rt.Characteristic == null)
                return;

            var version = _modelSyncService.ModelVersion;
            var currentModel = _modelSyncService.CurrentModelNo?.Trim();

            // 型号与版本均未变化 → 跳过（不重复解析）
            if (version == rt.LastModelVersion
                && string.Equals(currentModel, rt.LastResolvedModelNo, StringComparison.Ordinal))
            {
                return;
            }

            try
            {
                var ctx = await _modelResolver.ResolveAsync(rt.Characteristic);
                rt.Resolved = ctx;
                rt.LastResolvedModelNo = ctx?.ModelNo;
                rt.LastModelVersion = version;
                rt.Collector.SetContext(ctx);

                if (ctx != null && ctx.IsSuccess)
                {
                    Serilog.Log.Information("[SPC解析]【{Name}】型号变更生效 ModelNo={ModelNo} Part={PartName}",
                        rt.Collector.FeatureName, ctx.ModelNo, ctx.Part?.Name);
                }
                else if (ctx != null)
                {
                    Serilog.Log.Warning("[SPC解析]【{Name}】解析失败 Reason={Reason}",
                        rt.Collector.FeatureName, ctx.Reason);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning("[SPC解析]【{Name}】刷新上下文异常: {Message}",
                    rt.Collector.FeatureName, ex.Message);
            }
        }

        #endregion

        #region 运行控制

        public ApiResult Start(int characteristicId)
        {
            if (!_tasks.TryGetValue(characteristicId, out var rt))
                return ApiResult.Fail("特性未加载");
            if (!rt.IsEnabled) return ApiResult.Fail("特性未启用");
            rt.IsRunning = true;
            rt.LastRun = DateTime.MinValue;
            return ApiResult.Success($"\"{rt.Collector.FeatureName}\" 采集已启动");
        }

        public ApiResult Stop(int characteristicId)
        {
            if (!_tasks.TryGetValue(characteristicId, out var rt))
                return ApiResult.Fail("特性未加载");
            rt.IsRunning = false;
            return ApiResult.Success($"\"{rt.Collector.FeatureName}\" 采集已停止");
        }

        public ApiResult SetEnabled(int characteristicId, bool enabled)
        {
            if (!_tasks.TryGetValue(characteristicId, out var rt))
                return ApiResult.Fail("特性未加载");
            rt.IsEnabled = enabled;
            if (!enabled) rt.IsRunning = false;
            return ApiResult.Success(enabled ? "已启用" : "已禁用");
        }

        public void StartAll()
        {
            foreach (var rt in _tasks.Values.Where(x => x.IsEnabled))
            {
                rt.IsRunning = true;
                rt.LastRun = DateTime.MinValue;
            }
        }

        public void StopAll()
        {
            foreach (var rt in _tasks.Values)
                rt.IsRunning = false;
        }

        #endregion

        #region 查询

        static TypeCode ParseTypeCode(string dataType)
        {
            switch (dataType?.ToLower())
            {
                case "int16": return TypeCode.Int16;
                case "int32": return TypeCode.Int32;
                case "float": return TypeCode.Single;
                case "double": return TypeCode.Double;
                case "bool": return TypeCode.Boolean;
                case "string": return TypeCode.String;
                default: return TypeCode.Int16;
            }
        }

        public List<SPCDataCollectionInfo> GetAll()
        {
            return _tasks.Values
                .OrderBy(x => x.CharacteristicId)
                .Select(rt => new SPCDataCollectionInfo
                {
                    CharacteristicId = rt.CharacteristicId,
                    FeatureName = rt.Collector.FeatureName,
                    DataSource = rt.Collector.DataSource,
                    Description = rt.Collector.Description,
                    IsRunning = rt.IsRunning,
                    IsEnabled = rt.IsEnabled,
                    IntervalMs = rt.Interval.TotalMilliseconds,
                    ResolvedModelNo = rt.Resolved?.ModelNo,
                    ResolvedPartName = rt.Resolved?.Part?.Name,
                    ResolvedPartModelNo = rt.Resolved?.Part?.ModelNo,
                    ResolveReason = rt.Resolved == null ? null
                        : (rt.Resolved.IsSuccess ? null : rt.Resolved.Reason),
                })
                .ToList();
        }

        #endregion

        class SPCRuntime
        {
            public int CharacteristicId;
            public SPCDataCollectionAbstract Collector;
            public Tb_SPC_Characteristic Characteristic;
            public Models.Domain.SPCResolvedContext Resolved;
            public string LastResolvedModelNo;
            public long LastModelVersion;
            public bool IsRunning;
            public bool IsEnabled;
            public DateTime LastRun;
            public TimeSpan Interval;
            public volatile bool InProgress;
        }
    }

    public class SPCDataCollectionInfo
    {
        public int CharacteristicId { get; set; }
        public string FeatureName { get; set; }
        public SPCDataSource DataSource { get; set; }
        public string Description { get; set; }
        public bool IsRunning { get; set; }
        public bool IsEnabled { get; set; }
        public double IntervalMs { get; set; }
        public string ResolvedModelNo { get; set; }
        public string ResolvedPartName { get; set; }
        public string ResolvedPartModelNo { get; set; }
        public string ResolveReason { get; set; }
    }
}
