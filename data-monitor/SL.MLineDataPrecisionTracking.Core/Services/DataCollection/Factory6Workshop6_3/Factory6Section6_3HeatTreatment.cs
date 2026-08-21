using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.POIFS.FileSystem;
using NPOI.SS.Formula.Functions;
using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage.Factory6Workshop6_3AssemblyLine;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using SqlSugar.Extensions;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_3
{
    public class Factory6Section6_3HeatTreatment : DataCollectionServiceAbstract
    {
        Tb_EquipmentRepository _equipmentRepository;
        McpCommunication _mcp;
        Tb_HeatTreatmentDataRepository _rclRepository;
        Tb_Factory6Workshop6_3Line_EnergyRangeRepository _energyRangeRepository;
        IHubContext _chatHub;
        Tb_Factory6Workshop6_3Line_EnergyRangePointRepository _pointRepository;

        Type _dataModelType = typeof(Tb_HeatTreatmentData);

        List<DevPlcPointDto> _lineReadPlcInfo;
        DevPlcPointDto _plcCallPCCanCollectionPoint;
        DevPlcPointDto _plcCallPCMarkingNoPoint;
        DevPlcPointDto _plcEnergyCollectionPermitPoint;
        DevPlcPointDto _plcEnergyValuePoint;
        DevPlcPointDto _plcEnergyTimePoint;
        List<DevPlcPointDto> _plcEnergyPoint;
        string _lastMarkingNo;
        List<PointData<double, double>> _historyData;

        CancellationTokenSource _energyCts;

        double _lastEnergyTime;
        protected override string _serviceName => "六分厂6-3热处理";

        public Factory6Section6_3HeatTreatment(
            Tb_EquipmentRepository equipmentRepositor,
            Tb_HeatTreatmentDataRepository rclRepository,
            McpCommunication mcp,
            IHubContext chatHub,
            Tb_Factory6Workshop6_3Line_EnergyRangeRepository energyRangeRepository,
            Tb_Factory6Workshop6_3Line_EnergyRangePointRepository pointRepository
        )
        {
            _equipmentRepository = equipmentRepositor;
            _rclRepository = rclRepository;
            _historyData = new List<PointData<double, double>>();
            _energyRangeRepository = energyRangeRepository;
            _pointRepository = pointRepository;
            _mcp = mcp;
            _chatHub = chatHub;
        }

        protected override async Task<Result> InitAsync()
        {
            var linePoint = await _equipmentRepository.GetEquipmentAllAsync(x =>
                x.DeviceName == "热处理"
            );
            if (linePoint is null)
            {
                return Result.Fail("未找到设备点位信息");
            }

            _lineReadPlcInfo = new List<DevPlcPointDto>();
            foreach (var plcLinkeInfo in linePoint.PlcConnections)
            {
                foreach (var plcAddres in plcLinkeInfo.Points)
                {
                    _lineReadPlcInfo.Add(
                        new DevPlcPointDto(
                            linePoint.DeviceName,
                            plcAddres.PointName,
                            plcLinkeInfo.IpAddress,
                            plcLinkeInfo.Port,
                            plcAddres.Area,
                            plcAddres.DataType.ToTypeCode(),
                            plcAddres.Address,
                            plcAddres.Length,
                            plcAddres.ReadFormula,
                            plcAddres.WriteFormula
                        )
                    );
                }
            }

            if (_lineReadPlcInfo == null || _lineReadPlcInfo.Count <= 0)
            {
                return Result.Fail("点位数据异常");
            }

            _plcCallPCCanCollectionPoint = _lineReadPlcInfo.First(x => x.PointName == "采集开始");
            _lineReadPlcInfo.Remove(_plcCallPCCanCollectionPoint);
            _plcEnergyCollectionPermitPoint = _lineReadPlcInfo.First(x =>
                x.PointName == "能量采集许可"
            );
            _lineReadPlcInfo.Remove(_plcEnergyCollectionPermitPoint);

            _plcCallPCMarkingNoPoint = _lineReadPlcInfo.First(x => x.PointName == "序列码");

            _plcEnergyValuePoint = _lineReadPlcInfo.First(x => x.PointName == "能量");
            _plcEnergyTimePoint = _lineReadPlcInfo.First(x => x.PointName == "能量时间");
            _plcEnergyPoint = new List<DevPlcPointDto>()
            {
                _plcEnergyValuePoint,
                _plcEnergyTimePoint,
            };
            _lineReadPlcInfo.Remove(_plcEnergyValuePoint);
            _lineReadPlcInfo.Remove(_plcEnergyTimePoint);
            _energyCts = new CancellationTokenSource();
            _ = Task.Run(() => EnergyCollectionLoopAsync(_energyCts.Token));

            return Result.Success();
        }

        protected override async Task<Result> HandshakeAsync()
        {
            var canCol = _mcp.Read(_plcCallPCCanCollectionPoint);
            if (canCol.IsSuccess is false || canCol.Data.Value[0].ObjToBool() is false)
            {
                return Result.Fail("PLC未触发采集信号");
            }

            var markRe = _mcp.Read(_plcCallPCMarkingNoPoint);
            if (markRe.IsSuccess is false)
            {
                return Result.Fail("序列码读取失败");
            }

            var markingNo = string.Concat(markRe.Data.Value);
            if (string.IsNullOrEmpty(markingNo) || markingNo == _lastMarkingNo)
            {
                return Result.Fail("序列码为空或重复");
            }

            return Result.Success();
        }

        protected override async Task<Result<object>> InteractAsync()
        {
            var readValue = _mcp.Read(_lineReadPlcInfo);
            if (readValue.IsSuccess is false)
            {
                return Result<object>.Fail(readValue.Message);
            }

            var result = DataCollectionExpand.SugarColumnReflectAssign(readValue, _dataModelType);
            if (result.IsSuccess is false)
            {
                return result;
            }

            return result;
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
            var heatData = (Tb_HeatTreatmentData)interact.Data;

            heatData.Energy = $"{heatData.HeatingTime}*{heatData.OutputPower}"
                .StringCompute()
                .ToString();

            await _rclRepository.InsertableAsync(heatData);
            _lastMarkingNo = heatData.MarkingNo;
        }

        async Task EnergyCollectionLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_sleepTimeSpan, token);

                try
                {
                    var permitResult = _mcp.Read(_plcEnergyCollectionPermitPoint);
                    if (permitResult.IsSuccess)
                    {
                        if (permitResult.Data.Value[0].ObjToBool())
                        {
                            var valueResult = _mcp.Read(_plcEnergyPoint);
                            if (valueResult.IsSuccess)
                            {
                                if (
                                    double.TryParse(
                                        _plcEnergyValuePoint.Value[0].ToString(),
                                        out double energyValue
                                    )
                                    && double.TryParse(
                                        _plcEnergyTimePoint.Value[0].ToString(),
                                        out double energyTime
                                    )
                                    && energyTime > 0&& _lastEnergyTime != energyTime
                                )
                                {
                                    _lastEnergyTime= energyTime;
                                    var energy = new PointData<double, double>
                                    {
                                        X = energyTime,
                                        Y = energyValue,
                                    };
                                    _historyData.Add(energy);
                                    ((IClientProxy)_chatHub.Clients.All).Invoke(
                                        "Factory6Section6_3HeatTreatEnergyMgmt",
                                        energy
                                    );
                                }
                            }
                        }
                        else
                        {
                            if (_historyData.Count > 0)
                            {
                                DateTime now = DateTime.Now;
                                // 舍弃毫秒，只保留到秒
                                DateTime dtSec = now.AddTicks(
                                    -(now.Ticks % TimeSpan.TicksPerSecond)
                                );

                                var energyRang =
                                    new Models.Entities.Factory6Workshop6_3AssemblyLine.Tb_Factory6Workshop6_3Line_EnergyRange()
                                    {
                                        RecordTime = dtSec,Count= _historyData.Count
                                    };
                                await _energyRangeRepository.InsertableAsync(energyRang);
                                // TODO: 将 _historyData 存入数据库
                                await _pointRepository.InsertableAsync(
                                    _historyData
                                        .Select(
                                            p => new Models.Entities.Factory6Workshop6_3AssemblyLine.Tb_Factory6Workshop6_3Line_EnergyRangePoint()
                                            {
                                                EnergyRange_RecordTime = energyRang.RecordTime,
                                                Time = p.X,
                                                Value = p.Y,
                                            }
                                        )
                                        .ToList()
                                );
                                _historyData.Clear();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Serilog.Log.Warning(
                        "[能量采集]【{_serviceName}】能量采集异常:{Message}",
                        _serviceName,
                        ex.Message
                    );
                }
            }
        }

        public override void Stop()
        {
            _energyCts?.Cancel();
            _energyCts?.Dispose();
            _energyCts = null;
            base.Stop();
        }
    }
}
