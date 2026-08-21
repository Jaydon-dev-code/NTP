using System;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Internal;
using Newtonsoft.Json.Linq;
using SL.MLineDataPrecisionTracking.Core.Mqtt;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Domain.Mqtt;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection
{
    /// <summary>
    /// 工位采集管理服务 — 采集的"统筹调度中枢"。
    ///
    /// ── 设计思想 ─────────────────────────────────────────────────────────────
    /// 旧架构是"每个工位自己起一个线程循环"，工位一多、设备又弱时，线程和并发连接数
    /// 会失控。本类反其道而行：采集执行由本管理服务统一调度，工位只是被调度的单元。
    ///
    /// ── 调度模型（三步曲）────────────────────────────────────────────────────
    /// 1. 注册（RegisterDeviceAsync）：
    ///      根据设备编号 EquipmentId 找到设备 → 解析 MQTT 配置 → 创建 IStationCollection 工位对象
    ///      → 调用 工位.InitAsync() 初始化（加载 PLC 点位等）
    ///      → 初始化【成功】才把工位放入采集队列 _stations
    ///      → 注册信息落库（Tb_DeviceCollection），重启后据此恢复
    /// 2. 运行（Worker 并行调度）：
    ///      启动固定 N 个 Worker（默认 = CPU 核数），每个 Worker 反复执行 WorkerLoopAsync。
    ///      每个 Worker 每次只"抢"一个满足条件的工位执行其 DataCollectionAsync()，
    ///      多个 Worker 之间天然并行，但同一工位有 InProgress 标记保证不会被重复执行。
    /// 3. 停止/注销（Stop/Unregister）：
    ///      把工位的 IsRunning 置为 false（Worker 不再选它）→ 从队列移除 → 删除持久化记录。
    ///
    /// ── 设备标识 ─────────────────────────────────────────────────────────────
    /// 本类全程使用 string 设备编号（Tb_Equipment.EquipmentId，业务标识）作为工位唯一 key，
    /// 与 Tb_Equipment 的 int 自增主键 Id 解耦。Tb_DeviceCollection.EquipmentId 亦为 string。
    ///
    /// ── 并发与安全 ────────────────────────────────────────────────────────────
    /// • _stations：ConcurrentDictionary，key = 设备编号(string)，value = StationRuntime
    /// • _scheduleLock：保护"选工位/改运行标记"这段临界区，防止多个 Worker 抢同一工位
    /// • StationRuntime.InProgress：volatile 标记，表示该工位正在被某个 Worker 执行
    /// • 单工位异常被 try/catch 隔离，不会影响其他工位或让 Worker 崩溃
    /// • 工位内部 DataCollectionAsync 已做超时隔离（PLC 卡死时跳过），不会阻塞 Worker
    ///
    /// ── 持久化与恢复 ──────────────────────────────────────────────────────────
    /// 注册记录存 Tb_DeviceCollection（含 MQTT 配置、Topic、IsEnabled、IsRunning）。
    /// 服务重启后调用 RestoreAsync()：从表里读出所有注册 → 重新 Init → 放回队列，
    /// 上次 IsRunning=true 且启用的工位会自动恢复运行。
    ///
    /// 与 Service 项目现有的 DataCollectionServiceManager（类名驱动的专用采集服务）
    /// 完全独立、互不影响。
    /// </summary>
    public class StationCollectionManager
    {
        /// <summary>
        /// 采集队列：已注册且初始化成功的工位。
        /// key = 设备编号（string EquipmentId），value = 该工位的运行时信息。
        /// Worker 循环遍历此字典挑选要执行的工位。
        /// </summary>
        private readonly ConcurrentDictionary<string, StationRuntime> _stations =
            new ConcurrentDictionary<string, StationRuntime>();

        /// <summary>
        /// MQTT 客户端缓存：按 "host:port:user:pass" 复用，避免每工位各建一条连接。
        /// </summary>
        private readonly ConcurrentDictionary<string, MqttService> _mqttServices =
            new ConcurrentDictionary<string, MqttService>();

        private readonly Tb_EquipmentRepository _equipmentRepository;
        private readonly EquipmentManagementService _equipmentManagementService;
        private readonly Tb_DeviceCollectionRepository _deviceCollectionRepository;
        private readonly McpCommunication _mcp;

        /// <summary>
        /// 调度锁：保护"挑选工位 + 修改运行标记/上次时间"这段临界区。
        /// 多个 Worker 同时扫描 _stations 时，靠它保证同一工位只会被一个 Worker 选中。
        /// </summary>
        private readonly object _scheduleLock = new object();

        private CancellationTokenSource _workersCts; // Worker 的取消令牌源（StopWorkers 时取消）
        private Task[] _workers; // 并行 Worker 任务数组
        private readonly int _workerCount; // Worker 数量（= CPU 核数）

        /// <summary>当前 Worker 数量（外部可查）</summary>
        public int WorkerCount => _workerCount;

        public StationCollectionManager(
            Tb_EquipmentRepository equipmentRepository,
            EquipmentManagementService equipmentManagementService,
            Tb_DeviceCollectionRepository deviceCollectionRepository,
            McpCommunication mcp
        )
        {
            _equipmentRepository = equipmentRepository;
            _equipmentManagementService = equipmentManagementService;
            _deviceCollectionRepository = deviceCollectionRepository;
            _mcp = mcp;

            // Worker 数量 = CPU 核数（弱设备核数少，并发也自然少，正好限流）
            _workerCount = Math.Max(1, Environment.ProcessorCount);
        }

        #region Worker 启停

        /// <summary>
        /// 启动 N 个并行 Worker。
        /// 每个 Worker 是一个后台任务，持续执行 WorkerLoopAsync 从队列里"抢"工位采集。
        /// 幂等：已启动（有未完成的任务）时直接返回，不会重复创建。
        /// </summary>
        public void StartWorkers()
        {
            lock (_scheduleLock)
            {
                if (_workers != null && _workers.Any(w => !w.IsCompleted))
                    return;

                _workersCts = new CancellationTokenSource();
                _workers = new Task[_workerCount];
                for (int i = 0; i < _workerCount; i++)
                {
                    int idx = i;
                    _workers[idx] = Task.Run(() => WorkerLoopAsync(_workersCts.Token));
                }

                PCHeartbeat(_workersCts);
            }
        }

        /// <summary>
        /// 停止所有 Worker（通过取消令牌通知各循环退出）。
        /// </summary>
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
        }

        /// <summary>
        /// 单个 Worker 的调度循环（每个 Worker 一个实例，并行运行）。
        ///
        /// 每一轮：
        /// ① 在锁内扫描 _stations，找出"正在运行 + 没在执行 + 距上次执行已够间隔"的工位，
        ///    命中后立刻把它标记为 InProgress=true（占住，防止别的 Worker 也抢到它）。
        /// ② 没找到这样的工位 → 睡 20ms 再下一轮（空转，让出 CPU）。
        /// ③ 找到 → 在锁外执行 工位.DataCollectionAsync()（真正的 PLC 读 + MQTT 推送）。
        ///    - 在锁外执行是关键：如果带着锁执行，其他 Worker 抢工位会被锁卡住，并行就失效了。
        /// ④ 执行完毕（无论成败）在 finally 里：更新 LastRun=now，InProgress=false，释放工位。
        /// 这样 N 个 Worker 各自循环，就能并行服务不同的工位。
        /// </summary>
        private async Task WorkerLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // ① 挑选一个可执行的工位（临界区）
                IStationCollection station = null;
                lock (_scheduleLock)
                {
                    // 筛选满足条件，取上次执行完成时间最早的工位
                    var candidate = _stations
                        .Values.Where(rt =>
                            rt.Station.IsRunning
                            && !rt.InProgress
                            && DateTime.UtcNow - rt.LastRun >= rt.Station.Interval
                        )
                        .OrderBy(rt => rt.LastRun)
                        .FirstOrDefault();
                    if (candidate != null)
                    {
                        candidate.InProgress = true;
                        station = candidate.Station;
                    }
                }

                // ② 无可执行工位 → 短暂等待后继续
                if (station == null)
                {
                    try
                    {
                        await Task.Delay(20, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    continue;
                }

                // ③ 执行工位采集（锁外，不阻塞其他 Worker 抢位）
                try
                {
                    using (var timeoutCts = new CancellationTokenSource())
                    {
                        var collectTask = station.DataCollectionAsync();
                        var delayTask = Task.Delay(5000, timeoutCts.Token);

                        var completed = await Task.WhenAny(collectTask, delayTask);

                        if (completed == delayTask)
                        {
                            // 超时分支
                            Serilog.Log.Warning(
                                "[工位采集]【{StationName}】采集执行超时({TimeoutMs}ms)",
                                station.StationName,
                                5000
                            );
                        }
                        else
                        {
                            // 采集先完成，取消delay，释放timer
                            timeoutCts.Cancel();
                            // ⭐关键：必须await，把业务异常抛到外层catch
                            await collectTask;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 单工位异常隔离：只记日志，不让 Worker 崩溃
                    Serilog.Log.Warning(
                        "[工位采集]【{StationName}】采集异常:{Message}",
                        station.StationName,
                        ex.Message
                    );
                }
                finally
                {
                    // ④ 释放工位：记录本次执行时间 + 清除占用标记
                    lock (_scheduleLock)
                    {
                        if (_stations.TryGetValue(station.StationId, out var rt))
                        {
                            rt.LastRun = DateTime.UtcNow; // 统一UtcNow
                            rt.InProgress = false;
                        }
                    }
                }
            }
        }

        #endregion

        #region 恢复（服务启动时调用）

        /// <summary>
        /// 服务启动时的恢复入口（OwinHostService.OnStart 调用）。
        /// 流程：读 Tb_DeviceCollection 全部注册记录 → 逐个重新创建工位并 Init →
        /// 成功则放回队列 → 上次"启用且运行中"的工位自动置为运行（Worker 会接手采集）。
        /// 恢复完成后启动 Worker（幂等）。
        /// 目的：重启服务不丢注册，采集自动接着跑。
        /// </summary>
        public async Task RestoreAsync()
        {
            var records = await _deviceCollectionRepository.GetListAsync();
            foreach (var record in records.Where(x=>x.IsEnabled && x.IsRunning))
            {
                try
                {
                    // 设备是否还在（可能已被删除）— 按设备编号查
                    var equipment = await _equipmentRepository.GetWithLineAsync(x =>
                        x.EquipmentId == record.EquipmentId 
                    );
                    if (equipment == null)
                    {
                        // 设备不存在了 → 清理残留注册
                        await _deviceCollectionRepository.DeleteAsync(record.EquipmentId);
                        continue;
                    }

                    // 已在内存队列（例如先注册后恢复）→ 跳过
                    if (_stations.ContainsKey(record.EquipmentId))
                        continue;

                    // 用持久化的 MQTT 配置重建连接（record 存的是上次注册时的解析结果）
                    var config = new MqttPublishConfig
                    {
                        Host = record.MqttHost,
                        Port = record.MqttPort ?? 1883,
                        Username = record.MqttUsername,
                        Password = record.MqttPassword,
                        Topic = record.Topic,
                    };
                    var resolved = ResolveMqttConfig(config, equipment);
                    var mqtt = GetMqttService(resolved);
                    // Topic 优先用持久化的，没有则按 厂标识/产线标识 重新拼接
                    var topic =
                        record.Topic
                        ?? resolved.BuildTopic(
                            equipment.ProductionLine?.Factory?.FactoryCode,
                            equipment.ProductionLine?.LineCode,
                            equipment.DeviceName
                        );

                    // 创建工位对象（key = string 设备编号）
                    var station = new DeviceStationCollection(
                        equipment.ProductionLine?.LineCode,
                        record.EquipmentId,
                        equipment.DeviceName,
                        _equipmentRepository,
                        _mcp,
                        mqtt,
                        topic
                    );

                    // 初始化成功才恢复；失败（如点位没了）则跳过并记日志
                    var init = await station.InitAsync(null);
                    if (init.IsSuccess is false)
                    {
                        Serilog.Log.Warning(
                            "[工位采集]恢复初始化失败: {DeviceName}({EquipmentId}) {Message}",
                            equipment.DeviceName,
                            record.EquipmentId,
                            init.Message
                        );
                        continue;
                    }

                    // 恢复运行状态：只有"启用且上次在运行"的工位才自动接着跑
                    station.IsRunning = record.IsEnabled && record.IsRunning;
                    station.Interval = TimeSpan.FromSeconds(1);

                    _stations[record.EquipmentId] = new StationRuntime
                    {
                        Station = station,
                        LastRun = DateTime.MinValue, // 初始为最早，保证恢复后立刻可被调度
                        InProgress = false,
                    };

                    if (record.IsEnabled && record.IsRunning)
                    {
                        Serilog.Log.Information(
                            "[工位采集]恢复注册并自动运行: {DeviceName}({EquipmentId})",
                            equipment.DeviceName,
                            record.EquipmentId
                        );
                    }
                }
                catch (Exception ex)
                {
                    // 单条恢复失败不阻塞其他记录的恢复
                    Serilog.Log.Warning(
                        "[工位采集]恢复注册失败: EquipmentId={EquipmentId} {Message}",
                        record.EquipmentId,
                        ex.Message
                    );
                }
            }

            // 恢复后启动 Worker（幂等，重复调用安全）
            StartWorkers();
        }

        #endregion

        #region 心跳（上线/离线信号，定时发布）

        private void PCHeartbeat(CancellationTokenSource workersCts)
        {
            var raw = ConfigurationManager.AppSettings["MqttHeartbeat"];
            // 解析键值对
            var dict = raw.Split(';')
                .Select(s => s.Split('='))
                .ToDictionary(arr => arr[0], arr => arr[1]);

            // 构造匿名对象（注意类型手动转换）
            var mqttObj = new
            {
                ip = dict["ip"],
                port = int.Parse(dict["port"]),
                user = dict["user"],
                pwd = dict["pwd"],
                topicId = int.Parse(dict["topicId"]),
            };
            var mqtt = GetMqttService(
                new MqttPublishConfig()
                {
                    Host = mqttObj.ip,
                    Port = mqttObj.port,
                    Username = mqttObj.user,
                    Password = mqttObj.pwd,
                    Topic = $"BearingBranch6/{mqttObj.topicId}/online",
                }
            );
            Task.Run(
                async () =>
                {
                    while (!workersCts.IsCancellationRequested)
                    {
                        mqtt.PublishAsync(
                            $"BearingBranch6/{mqttObj.topicId}/online",
                            new
                            {
                                ID = mqttObj.topicId,
                                status = "online",
                                time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            }
                        );
                        await Task.Delay(1000, workersCts.Token);
                    }
                    mqtt.PublishAsync(
                        $"BearingBranch6/{mqttObj.topicId}/online",
                        new { ID = mqttObj.topicId, status = "offline" }
                    );
                },
                workersCts.Token
            );
        }

        #endregion

        #region 注册 / 注销

        /// <summary>
        /// 注册一台设备工位采集（单台）。
        ///
        /// 完整流程：
        ///  1. 按设备编号 EquipmentId 校验设备存在、已归属产线（否则无法拼接 topic / 解析 MQTT）
        ///  2. 解析 MQTT 配置：优先用调用方传入的 mqttConfig，否则取 产线/厂 的配置字段
        ///  3. 生成发布 topic：未指定时按 厂标识/产线标识/设备名 拼接
        ///  4. 创建 DeviceStationCollection 工位 → 调用 InitAsync 初始化（加载 PLC 点位）
        ///  5. 【初始化成功】才加入 _stations 队列（失败返回，不入队）
        ///  6. 注册信息落库 Tb_DeviceCollection（MQTT 配置 + topic + 启用/运行状态）
        ///  7. 启动 Worker（幂等），工位会被 Worker 按间隔调度采集
        /// </summary>
        public async Task<ApiResult> RegisterDeviceAsync(
            string equipmentId,
            MqttPublishConfig mqttConfig = null
        )
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
                return ApiResult.Fail("设备编号不能为空");

            var equipment = await _equipmentRepository.GetWithLineAsync(x =>
                x.EquipmentId == equipmentId
            );
            if (equipment == null)
                return ApiResult.Fail("设备不存在");

            if (equipment.LineId == null)
                return ApiResult.Fail("设备未归属产线，无法注册采集服务");

            lock (_scheduleLock)
            {
                if (_stations.ContainsKey(equipmentId))
                    return ApiResult.Fail($"设备 \"{equipment.DeviceName}\" 已注册");
            }

            // 解析 MQTT 配置（注册传入优先，否则用产线/厂配置）
            var resolved = ResolveMqttConfig(mqttConfig, equipment);
            var factoryCode = equipment.ProductionLine?.Factory?.FactoryCode;
            var lineCode = equipment.ProductionLine?.LineCode;
            var topic = resolved.BuildTopic(factoryCode, lineCode, equipmentId);
            var mqtt = GetMqttService(resolved);

            // 创建工位对象（key = string 设备编号）
            var station = new DeviceStationCollection(
                lineCode,
                equipmentId,
                equipment.DeviceName,
                _equipmentRepository,
                _mcp,
                mqtt,
                topic
            );

            // 初始化成功才入队（失败说明点位缺失等，直接拒绝注册）
            var init = await station.InitAsync(null);
            if (init.IsSuccess is false)
                return ApiResult.Fail($"设备初始化失败: {init.Message}");

            // 读取历史注册记录：保留之前的启用状态，但注册时不自动运行（需用户 Start）
            var record = await _deviceCollectionRepository.GetByEquipmentIdAsync(equipmentId);
            var wasEnabled = record?.IsEnabled ?? true;

            station.IsRunning = false;
            station.Interval = TimeSpan.FromMilliseconds(500);

            // 加入采集队列
            _stations[equipmentId] = new StationRuntime
            {
                Station = station,
                LastRun = DateTime.MinValue,
                InProgress = false,
            };

            // 持久化注册记录
            await _deviceCollectionRepository.UpsertAsync(
                new Tb_DeviceCollection
                {
                    EquipmentId = equipmentId,
                    DeviceName = equipment.DeviceName,
                    MqttHost = resolved.Host,
                    MqttPort = resolved.Port,
                    MqttUsername = resolved.Username,
                    MqttPassword = resolved.Password,
                    Topic = topic,
                    IsEnabled = wasEnabled,
                    IsRunning = false,
                }
            );

            // 确保 Worker 在跑（幂等），工位之后才会被调度
            StartWorkers();

            return ApiResult.Success(
                $"设备 \"{equipment.DeviceName}\" 已注册，MQTT Topic: {topic}"
            );
        }

        /// <summary>
        /// 按产线批量注册：查出该产线下所有设备，逐台 RegisterDeviceAsync。
        /// 已注册的跳过，返回"共注册 N 台"。
        /// </summary>
        public async Task<ApiResult> RegisterLineAsync(int lineId)
        {
            var equipments = await _equipmentManagementService.GetEquipmentsAsync(
                factoryId: null,
                lineId: lineId
            );
            if (equipments.IsSuccess is false)
                return ApiResult.Fail(equipments.Message);

            var list = equipments.Data ?? new List<Tb_Equipment>();
            if (list.Count == 0)
                return ApiResult.Fail("该产线下没有设备");

            foreach (var equipment in list)
            {
                if (!_stations.ContainsKey(equipment.EquipmentId))
                {
                    await RegisterDeviceAsync(equipment.EquipmentId);
                }
            }
            return ApiResult.Success($"产线 {lineId} 下共注册 {list.Count} 台设备");
        }

        /// <summary>
        /// 按厂批量注册：查出该厂下所有设备，逐台 RegisterDeviceAsync。
        /// 已注册的跳过，返回"共注册 N 台"。
        /// </summary>
        public async Task<ApiResult> RegisterFactoryAsync(int factoryId)
        {
            var equipments = await _equipmentManagementService.GetEquipmentsAsync(
                factoryId: factoryId,
                lineId: null
            );
            if (equipments.IsSuccess is false)
                return ApiResult.Fail(equipments.Message);

            var list = equipments.Data ?? new List<Tb_Equipment>();
            if (list.Count == 0)
                return ApiResult.Fail("该厂下没有设备");

            foreach (var equipment in list)
            {
                if (!_stations.ContainsKey(equipment.EquipmentId))
                {
                    await RegisterDeviceAsync(equipment.EquipmentId);
                }
            }
            return ApiResult.Success($"厂 {factoryId} 下共注册 {list.Count} 台设备");
        }

        /// <summary>
        /// 注销设备工位采集（单台）。
        /// 从队列移除（TryRemove）→ 调工位 StopAsync 释放资源 → 删除持久化记录。
        /// 移除后 Worker 不会再调度它，即彻底停止采集。
        /// </summary>
        public async Task<ApiResult> UnregisterDeviceAsync(string equipmentId)
        {
            if (!_stations.TryRemove(equipmentId, out var runtime))
                return ApiResult.Fail("设备未注册");

            await runtime.Station.StopAsync();
            await _deviceCollectionRepository.DeleteAsync(equipmentId);
            return ApiResult.Success($"设备 \"{runtime.Station.StationName}\" 已注销");
        }

        #endregion

        #region 运行控制

        /// <summary>
        /// 启动采集：把工位置为运行（IsRunning=true），并把 LastRun 置为最早，
        /// 这样 Worker 下一轮就会选中它开始采集。运行状态同步落库（供重启恢复）。
        /// </summary>
        public async Task<ApiResult> StartAsync(string equipmentId)
        {
            if (!_stations.TryGetValue(equipmentId, out var runtime))
                return ApiResult.Fail("设备未注册");

            var record = await _deviceCollectionRepository.GetByEquipmentIdAsync(equipmentId);
            if (record != null && record.IsEnabled is false)
                return ApiResult.Fail("设备采集未启用");

            runtime.Station.IsRunning = true;
            runtime.LastRun = DateTime.MinValue;
            await PersistRunStateAsync(runtime.Station, running: true);
            return ApiResult.Success($"设备 \"{runtime.Station.StationName}\" 采集已启动");
        }

        /// <summary>
        /// 停止采集：工位置为非运行（IsRunning=false），Worker 不再选中它。
        /// 工位仍在队列中（可再 Start 恢复），运行状态落库。
        /// </summary>
        public async Task<ApiResult> StopAsync(string equipmentId)
        {
            if (!_stations.TryGetValue(equipmentId, out var runtime))
                return ApiResult.Fail("设备未注册");

            runtime.Station.IsRunning = false;
            await PersistRunStateAsync(runtime.Station, running: false);
            return ApiResult.Success($"设备 \"{runtime.Station.StationName}\" 采集已停止");
        }

        /// <summary>
        /// 重启采集：等价于"先置为运行并把 LastRun 归零"（工位会立即被重新调度），
        /// 运行状态落库。注意：不是先停后等，而是直接触发下一轮采集。
        /// </summary>
        public async Task<ApiResult> RestartAsync(string equipmentId)
        {
            if (!_stations.TryGetValue(equipmentId, out var runtime))
                return ApiResult.Fail("设备未注册");

            runtime.Station.IsRunning = true;
            runtime.LastRun = DateTime.MinValue;
            await PersistRunStateAsync(runtime.Station, running: true);
            return ApiResult.Success($"设备 \"{runtime.Station.StationName}\" 采集已重启");
        }

        /// <summary>
        /// 启用/禁用工位采集。
        /// 启用状态写入持久化记录；禁用时同时停止该工位运行。
        /// 禁用仅阻止采集，不删除注册（之后可再启用）。
        /// </summary>
        public async Task<ApiResult> SetEnabledAsync(string equipmentId, bool enabled)
        {
            if (!_stations.TryGetValue(equipmentId, out var runtime))
                return ApiResult.Fail("设备未注册");

            var record = await _deviceCollectionRepository.GetByEquipmentIdAsync(equipmentId);
            await _deviceCollectionRepository.UpsertAsync(
                new Tb_DeviceCollection
                {
                    EquipmentId = equipmentId,
                    DeviceName = runtime.Station.StationName,
                    MqttHost = record?.MqttHost,
                    MqttPort = record?.MqttPort,
                    MqttUsername = record?.MqttUsername,
                    MqttPassword = record?.MqttPassword,
                    Topic = record?.Topic ?? runtime.Station.Topic,
                    IsEnabled = enabled,
                    IsRunning = runtime.Station.IsRunning,
                }
            );

            // 禁用 → 立即停止
            if (!enabled)
            {
                runtime.Station.IsRunning = false;
                await PersistRunStateAsync(runtime.Station, running: false);
            }
            return ApiResult.Success(enabled ? "已启用" : "已禁用");
        }

        /// <summary>
        /// 启动队列中全部启用的工位（逐一 StartAsync 的逻辑，含落库）。
        /// </summary>
        public async Task StartAllAsync()
        {
            foreach (var runtime in _stations.Values)
            {
                var record = await _deviceCollectionRepository.GetByEquipmentIdAsync(
                    runtime.Station.StationId
                );
                if (record == null || record.IsEnabled)
                {
                    runtime.Station.IsRunning = true;
                    runtime.LastRun = DateTime.MinValue;
                    await PersistRunStateAsync(runtime.Station, running: true);
                }
            }
        }

        /// <summary>
        /// 停止队列中全部工位（逐一 StopAsync 的逻辑，含落库）。
        /// </summary>
        public async Task StopAllAsync()
        {
            foreach (var runtime in _stations.Values)
            {
                runtime.Station.IsRunning = false;
                await PersistRunStateAsync(runtime.Station, running: false);
            }
        }

        #endregion

        #region 查询

        /// <summary>
        /// 查询队列中所有已注册工位的信息（含厂/产线、状态、最新快照）。
        /// </summary>
        public async Task<ApiResult<List<StationCollectionInfo>>> GetAllRegisteredAsync()
        {
            var list = new List<StationCollectionInfo>();
            foreach (var kv in _stations)
            {
                list.Add(await BuildInfoAsync(kv.Value));
            }
            list = list.OrderBy(x => x.EquipmentId).ToList();
            return ApiResult<List<StationCollectionInfo>>.Success(list);
        }

        /// <summary>
        /// 查询单个已注册工位的信息。
        /// </summary>
        public async Task<ApiResult<StationCollectionInfo>> GetRegisteredAsync(string equipmentId)
        {
            if (!_stations.TryGetValue(equipmentId, out var runtime))
                return ApiResult<StationCollectionInfo>.Fail("设备未注册");

            return ApiResult<StationCollectionInfo>.Success(await BuildInfoAsync(runtime));
        }

        /// <summary>
        /// 查询工位最新点位快照（点位名 → 当前值）。
        /// 仅通用设备工位（DeviceStationCollection）支持；其他工位类型返回空字典。
        /// </summary>
        public async Task<ApiResult<Dictionary<string, object>>> GetDeviceValuesAsync(
            string equipmentId
        )
        {
            if (!_stations.TryGetValue(equipmentId, out var runtime))
                return ApiResult<Dictionary<string, object>>.Fail("设备未注册");

            if (runtime.Station is DeviceStationCollection dsc)
                return ApiResult<Dictionary<string, object>>.Success(
                    null /*dsc.GetLatestSnapshot()*/
                );

            return ApiResult<Dictionary<string, object>>.Success(new Dictionary<string, object>());
        }

        #endregion

        #region 内部

        /// <summary>
        /// 解析 MQTT 配置（注册时的实际生效配置）：
        /// • 调用方显式传入的 mqttConfig 优先；
        /// • 若传入配置缺 Host，则用 产线→厂 的 MQTT 字段补齐（产线优先，厂兜底）；
        /// • 完全未传时，直接取 产线→厂 的 MQTT 字段，端口缺省 1883。
        /// </summary>
        static MqttPublishConfig ResolveMqttConfig(
            MqttPublishConfig mqttConfig,
            Tb_Equipment equipment
        )
        {
            var line = equipment.ProductionLine;
            var factory = line?.Factory;

            if (mqttConfig != null)
            {
                if (string.IsNullOrWhiteSpace(mqttConfig.Host))
                {
                    mqttConfig.Host = line?.MqttHost ?? factory?.MqttHost;
                    mqttConfig.Port = line?.MqttPort ?? factory?.MqttPort ?? mqttConfig.Port;
                    mqttConfig.Username = line?.MqttUsername ?? factory?.MqttUsername;
                    mqttConfig.Password = line?.MqttPassword ?? factory?.MqttPassword;
                }
                return mqttConfig;
            }

            return new MqttPublishConfig
            {
                Host = line?.MqttHost ?? factory?.MqttHost,
                Port = line?.MqttPort ?? factory?.MqttPort ?? 1883,
                Username = line?.MqttUsername ?? factory?.MqttUsername,
                Password = line?.MqttPassword ?? factory?.MqttPassword,
            };
        }

        /// <summary>
        /// 获取/创建 MQTT 客户端：按 "host:port:user:pass" 作为 key 缓存复用，
        /// 同一 broker 只建一条连接，多个工位共享（线程安全由 MqttService 内部保证）。
        /// </summary>
        MqttService GetMqttService(MqttPublishConfig config)
        {
            var key = $"{config.Host}:{config.Port}:{config.Username}:{config.Password}";
            return _mqttServices.GetOrAdd(
                key,
                _ => new MqttService(config.Host, config.Port, config.Username, config.Password)
            );
        }

        /// <summary>
        /// 组装对外信息 DTO：
        /// 工位自身信息（编号/名称/Topic/状态/描述/快照）
        /// + 设备连表查出的厂/产线归属
        /// + 持久化记录里的启用状态。
        /// </summary>
        private async Task<StationCollectionInfo> BuildInfoAsync(StationRuntime runtime)
        {
            var station = runtime.Station;
            var equipment = await _equipmentRepository.GetWithLineAsync(x =>
                x.EquipmentId == station.StationId
            );
            var record = await _deviceCollectionRepository.GetByEquipmentIdAsync(station.StationId);

            return new StationCollectionInfo
            {
                EquipmentId = station.StationId,
                DeviceName = station.StationName,
                FactoryId = equipment?.ProductionLine?.FactoryId,
                FactoryCode = equipment?.ProductionLine?.Factory?.FactoryCode,
                FactoryName = equipment?.ProductionLine?.Factory?.FactoryName,
                LineId = equipment?.LineId,
                LineCode = equipment?.ProductionLine?.LineCode,
                LineName = equipment?.ProductionLine?.LineName,
                Topic = station.Topic,
                IsRunning = station.IsRunning,
                IsEnabled = record?.IsEnabled ?? true,
                Status = station.IsRunning ? "Running" : "Stopped",
                Description = station.Description,
                LatestSnapshot =
                    /*    station is DeviceStationCollection dsc ? dsc.GetLatestSnapshot() :*/null,
            };
        }

        /// <summary>
        /// 持久化工位的运行状态（启动/停止时调用）。
        /// 读取已有记录保留其余字段，只更新 IsRunning；无记录则新建一条。
        /// 供服务重启后 RestoreAsync 判断"上次是否在运行"。
        /// </summary>
        private async Task PersistRunStateAsync(IStationCollection station, bool running)
        {
            var record = await _deviceCollectionRepository.GetByEquipmentIdAsync(station.StationId);
            await _deviceCollectionRepository.UpsertAsync(
                new Tb_DeviceCollection
                {
                    EquipmentId = station.StationId,
                    DeviceName = station.StationName,
                    MqttHost = record?.MqttHost,
                    MqttPort = record?.MqttPort,
                    MqttUsername = record?.MqttUsername,
                    MqttPassword = record?.MqttPassword,
                    Topic = record?.Topic ?? station.Topic,
                    IsEnabled = record?.IsEnabled ?? true,
                    IsRunning = running,
                }
            );
        }

        #endregion


        /// <summary>
        /// 工位的运行时包装（管理服务内部专用）。
        /// 在 IStationCollection 之外额外记录"上次执行时间 / 是否正在执行"，
        /// 供 Worker 调度判断：够间隔了没、是不是正被别的 Worker 执行着。
        /// </summary>
        class StationRuntime
        {
            public IStationCollection Station; // 被调度的工位对象
            public DateTime LastRun; // 上次执行 DataCollectionAsync 的时间（调度间隔判断）
            public volatile bool InProgress; // 是否正在被执行（防止多 Worker 重复执行同一工位）
        }
    }
}
