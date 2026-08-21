using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using McpXLib.Enums;
using NPOI.XSSF.UserModel;
using SL.MLineDataPrecisionTracking.Core.Mqtt;
using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection
{
    /// <summary>
    /// 通用设备工位采集 — 实现 IStationCollection。
    /// 读取设备全部 PLC 点位（普通点位 + 警报点位一起采集、一起推送），
    /// 新旧对比后把变化点位发布到 MQTT；警报点位 on(true) 且相对上次变化时，
    /// 将对应点位名称组成 string[] 一并放入同一条 MQTT 消息。
    /// 读操作同步执行并用 Task.Run + 超时包裹，超时跳过，避免阻塞管理服务 Worker。
    /// </summary>
    public class DeviceStationCollection : IStationCollection
    {
        private readonly string _equipmentId;
        private readonly string _deviceName;
        private readonly Tb_EquipmentRepository _equipmentRepository;
        private readonly McpCommunication _mcp;
        private readonly MqttService _mqttService;
        private readonly string _topic;

        private readonly object _lock = new object();
        private List<DevPlcPointDto> _points = new List<DevPlcPointDto>();
        private bool _lastOnlieStuts;
        private bool _initialized;

        /// <summary>读操作超时（PLC 卡死时跳过该工位，不占死 Worker）</summary>
        private static readonly TimeSpan ReadTimeout = TimeSpan.FromSeconds(5);

        public string StationId => _equipmentId;
        public string StationName => _deviceName;
        public bool IsRunning { get; set; }
        public string Description { get; set; } = "未初始化";
        public TimeSpan Interval { get; set; } = TimeSpan.FromMilliseconds(500);
        public string Topic => _topic;
        CollectionInfo _collectionInfo;
        string _lineId;

        public DeviceStationCollection(
            string lineId,
            string equipmentId,
            string deviceName,
            Tb_EquipmentRepository equipmentRepository,
            McpCommunication mcp,
            MqttService mqttService,
            string topic
        )
        {
            _lineId = lineId;
            _equipmentId = equipmentId;
            _deviceName = deviceName;
            _equipmentRepository = equipmentRepository;
            _mcp = mcp;
            _mqttService = mqttService;
            _topic = topic;
        }

        public async Task<Result> InitAsync(object initData)
        {
            var equipment = await _equipmentRepository.GetEquipmentAllAsync(x =>
                x.EquipmentId == _equipmentId
            );
            if (equipment == null)
                return Result.Fail("未找到设备点位信息");

            _points = new List<DevPlcPointDto>();

            var aramPoint = new List<DevPlcPointDto>();
            foreach (var plc in equipment.PlcConnections ?? new List<Tb_PlcConnection>())
            {
                foreach (var point in plc.Points ?? new List<Tb_PlcPoint>())
                {
                    var dto = new DevPlcPointDto(
                        equipment.DeviceName,
                        point.PointName,
                        plc.IpAddress,
                        plc.Port,
                        point.Area,
                        point.DataType.ToTypeCode(),
                        point.Address,
                        point.Length,
                        point.ReadFormula,
                        point.WriteFormula
                    );
                    _points.Add(dto);
                    if (point.FunctionType == "警报")
                    {
                        aramPoint.Add(dto);
                    }
                }
            }

            if (_points.Count <= 0)
                return Result.Fail("点位数据异常");
            _collectionInfo = new CollectionInfo(_points, aramPoint);
            lock (_lock)
            {
                _initialized = true;
            }

            Description = "初始化完成";
            return Result.Success();
        }

        public async Task<Result> DataCollectionAsync()
        {
            if (!_initialized)
                return Result.Fail("工位未初始化");

            try
            {
                // 同步读放入线程池执行，整体加超时：PLC 卡死时跳过，避免阻塞 Worker
                var readTask = Task.Run(() => _mcp.Read(_points));
                var completed = await Task.WhenAny(readTask, Task.Delay(ReadTimeout));

                if (completed != readTask)
                {
                    Description = "读超时（PLC无响应）";
                    return Result.Fail("读取PLC超时");
                }

                var read = await readTask;
             
                    // 离线信号推送
                    if (_lastOnlieStuts != read.IsSuccess)
                    {
                        try
                        {
                            await _mqttService.PublishAsync(
                                _topic + @$"{_equipmentId}/event/push",
                                new { connect = read.IsSuccess, device = _equipmentId }
                            );
                        }
                        catch (Exception ex)
                        {
                            Serilog.Log.Warning(
                                "[设备采集]【{StationName}】MQTT 设备在线状态推送失败:{Message}",
                                StationName,
                                ex.Message
                            );
                        }
                 
                    _lastOnlieStuts = read.IsSuccess;
                    if (_lastOnlieStuts is false)
                    {
                        Description = "读取失败";
                        return read;
                    }
                   
                }
   
                if (_collectionInfo.IsChang())
                {
                    try
                    {
                        var mqttData = new MachineData()
                        {
                            gatewayMac = _lineId,
                            deviceId = _equipmentId,
                            deviceName = _deviceName,
                            time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            @params = new ParamsData()
                            {
                                Status = new ParamItem()
                                {
                                    name = "设备状态",
                                    value = _collectionInfo.Status,
                                },
                                 Beat= new ParamItem()
                                 {
                                     name = "生产节拍",
                                     value = _collectionInfo.Beat,
                                 },
                                 ProducedTotal = new ParamItem()
                                {
                                    name = "生产总数",
                                    value = _collectionInfo.ProducedTotal,
                                },
                                ProducedOK = new ParamItem()
                                {
                                    name = "生产OK数",
                                    value = _collectionInfo.ProducedOK,
                                },
                                ProducedNG = new ParamItem()
                                {
                                    name = "生产NG数",
                                    value = _collectionInfo.ProducedTotal,
                                },
                                Alarm = new ParamItem()
                                {
                                    name = "警报",
                                    value = _collectionInfo.WarInfo,
                                },
                            },
                        };
                        await _mqttService.PublishAsync(
                            _topic + @$"{_equipmentId}/data/push",
                            mqttData
                        );
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Warning(
                            "[设备采集]【{StationName}】MQTT 推送失败:{Message}",
                            StationName,
                            ex.Message
                        );
                    }
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                Description = $"采集异常: {ex.Message}";
                Serilog.Log.Warning(
                    "[设备采集]【{StationName}】采集异常:{Message}",
                    StationName,
                    ex.Message
                );
                return Result.Fail(ex.Message);
            }
        }

        public Task StopAsync()
        {
            lock (_lock)
            {
                _initialized = false;
            }
            Description = "已停止";
            return Task.CompletedTask;
        }
    }

    public class MachineData
    {
        public string gatewayMac { get; set; }
        public string deviceId { get; set; }
        public string deviceName { get; set; }
        public long time { get; set; }
        public ParamsData @params { get; set; }
    }

    public class ParamsData
    {
        public ParamItem Status { get; set; }
        public ParamItem Beat { get; set; }
        public ParamItem ProducedTotal { get; set; }
        public ParamItem ProducedOK { get; set; }
        public ParamItem ProducedNG { get; set; }
        public ParamItem Alarm { get; set; }
    }

    public class ParamItem
    {
        public string name { get; set; }
        public object value { get; set; }
    }
}
