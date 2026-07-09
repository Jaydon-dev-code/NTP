using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using SqlSugar.Extensions;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_1
{
    public class Factory6Section6_1HeatTreatment : DataCollectionServiceAbstract
    {
        Tb_EquipmentRepository _equipmentRepository;
        McpCommunication _mcp;
        Tb_HeatTreatmentDataRepository _rclRepository;

        Type _dataModelType = typeof(Tb_HeatTreatmentData);

        List<DevPlcPointDto> _lineReadPlcInfo;
        DevPlcPointDto _plcCallPCCanCollectionPoint;
        DevPlcPointDto _plcCallPCMarkingNoPoint;

        string _lastMarkingNo;

        protected override string _serviceName => "热处理";

        public Factory6Section6_1HeatTreatment(
            Tb_EquipmentRepository equipmentRepositor,
            Tb_HeatTreatmentDataRepository rclRepository,
            McpCommunication mcp
        )
        {
            _equipmentRepository = equipmentRepositor;
            _rclRepository = rclRepository;
            _mcp = mcp;
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
            _plcCallPCMarkingNoPoint = _lineReadPlcInfo.First(x => x.PointName == "序列码");

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

            return Expand.SugarColumnReflectAssign(readValue, _dataModelType);
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
    }
}
