using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Core.Services.Mes;
using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_1AssemblyLine;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;
using SqlSugar.Extensions;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_3
{
    public abstract class Factory6Workshop6_3AssemblyLineAbstract : DataCollectionServiceAbstract
    {
        protected Tb_EquipmentRepository _equipmentRepository;
        protected McpCommunication _mcp;
        protected Tb_ModelNoToNameRepository _modelNoToNameRepository;

        /// <summary>
        /// plc数据集合
        /// </summary>
        protected List<DevPlcPointDto> _lineReadPlcInfo;

        /// <summary>
        /// 可以采集的信号点位
        /// </summary>
        protected DevPlcPointDto _plcCallPCCanCollectionPoint;

        /// <summary>
        /// 托盘号的点位
        /// </summary>
        protected DevPlcPointDto _plcCallPCTrayNoPoint;

        /// <summary>
        /// 上次得托盘号
        /// </summary>
        protected string _lastTrayNoPoint;

        /// <summary>
        /// 型号列表
        /// </summary>
        protected List<Tb_ModelNoToName> _models;

        /// <summary>
        /// 线体名称
        /// </summary>
        protected abstract string _lineName { get; }

        /// <summary>
        /// 模型类型
        /// </summary>
        protected abstract Type _dataModelType { get; }

        /// <summary>
        /// 托盘号的中文名称
        /// </summary>
        protected abstract string _trayPointName { get; }
        protected MesApiOp _mesApiOp;
        private Tb_EquipmentRepository equipmentRepositor;
        private Tb_ModelNoToNameRepository tb_ModelNoToNameRepository;
        protected MesOrderOP _mesOrderOP;

        protected Factory6Workshop6_3AssemblyLineAbstract(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp,
            Tb_ModelNoToNameRepository tb_ModelNoToNameRepository,
            MesOrderOP mesOrderOP,
            MesApiOp mesApiOp
        )
        {
            _mesOrderOP = mesOrderOP;
            _mesApiOp = mesApiOp;
            _equipmentRepository = equipmentRepositor;
            _mcp = mcp;
            _modelNoToNameRepository = tb_ModelNoToNameRepository;
        }

        protected override async Task<Result> InitAsync()
        {
            var linePoint = await _equipmentRepository.GetEquipmentAllAsync(x =>
                x.DeviceName == _lineName
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
            _plcCallPCTrayNoPoint = _lineReadPlcInfo.First(x => x.PointName == _trayPointName);
            _models = await _modelNoToNameRepository.QueryabletAsync(x => true);

            return Result.Success();
        }

        protected override async Task<Result> HandshakeAsync()
        {
            var canCol = _mcp.Read(_plcCallPCCanCollectionPoint);
            if (canCol.IsSuccess is false || canCol.Data.Value[0].ObjToBool() is false)
            {
                return Result.Fail("PLC未触发采集信号");
            }

            var trayRe = _mcp.Read(_plcCallPCTrayNoPoint);
            if (
                trayRe.IsSuccess is false
                || trayRe.Data.Value[0].ToString() == "0"
                || trayRe.Data.Value[0].ToString() == _lastTrayNoPoint
            )
            {
                return Result.Fail("托盘号无效或重复");
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

            return DataCollectionExpand.SugarColumnReflectAssign(readValue, _dataModelType);
        }

        protected string GetVibrationDetectionResults(string vibrationDetectionResults)
        {
            switch (vibrationDetectionResults)
            {
                case null:
                    return string.Empty;
                case "0":
                    return "未采集";

                case "1":
                    return "OK";

                case "2":
                    return "NG";
                default:
                    return vibrationDetectionResults;
            }
        }

        protected void ABToSummary(
            object sourceA,
            object sourceB,
            object summary,
            List<string> ignoreFields = null
        )
        {
            if (ignoreFields == null)
            {
                ignoreFields = new List<string>();
            }

            var summaryProperties = summary
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.IsDefined(typeof(SugarColumn), true))
                .ToList();

            foreach (var prop in summaryProperties)
            {
                var fieldName = prop.Name;

                if (
                    ignoreFields.Any(ig => ig.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                )
                    continue;

                object value = GetValue(sourceA, fieldName);

                if (value == null)
                {
                    value = GetValue(sourceB, fieldName);
                }

                if (value != null)
                {
                    prop.SetValue(summary, value);
                }
            }
        }

        protected override async Task NotifyAsync(Result<object> interact)
        {
            var data = await InsterValue(interact);
            UpLastNo(data);
        }

        /// <summary>
        /// 更新上次对比得盘号
        /// </summary>
        /// <param name="data"></param>
        protected abstract void UpLastNo(object data);

        /// <summary>
        /// 插入数据到数据库
        /// </summary>
        /// <param name="interact"></param>
        /// <returns></returns>
        protected abstract Task<object> InsterValue(Result<object> interact);

        protected static List<(int BitIndex, string EnumName, string Description)> ParseBitEnum(
            int bitValue,
            Type enumType
        )
        {
            var result = new List<(int BitIndex, string EnumName, string Description)>();
            foreach (var item in Enum.GetValues(enumType))
            {
                int enumVal = Convert.ToInt32(item);
                if ((bitValue & enumVal) == 0)
                    continue;

                int bitIndex = 0;
                int temp = enumVal;
                while ((temp >>= 1) > 0)
                    bitIndex++;

                string desc = ((Enum)item).GetDescription();
                result.Add((bitIndex, item.ToString(), desc));
            }
            return result;
        }

        protected static string GetEnumDescription(int value, Type enumType)
        {
            if (!Enum.IsDefined(enumType, value))
                return string.Empty;
            return ((Enum)Enum.ToObject(enumType, value)).GetDescription();
        }

        static object GetValue(object obj, string fieldName)
        {
            if (obj == null)
                return null;

            var prop = obj.GetType().GetProperty(fieldName);
            return prop?.CanRead == true ? prop.GetValue(obj) : null;
        }

        protected async Task PushMes(List<MesDataAcquisitionUploadItemDTO> dtos)
        {
            try
            {
                if (dtos?.Count > 0)
                {
                    await _mesApiOp.DataAcquisition(dtos);
                    Serilog.Log.Information(
                        "[数据交互]【{_serverName}】推送数据到mesc共：{count}条数据",
                        _serviceName,
                        dtos?.Count
                    );
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning("[数据交互]【{_serverName}】推送数据到mes失败", _serviceName);
            }
        }

        protected (string Field, string EquipCode, string RouteCode)[] _items61 =
        {
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.ASideSteelBallGroupDiff),
                "ZP33-10",
                "ZC XHJ_LFC1 LGZCJGX_OP40"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.ASideSteelBallGreaseVolume),
                "ZP33-10",
                "ZC XHJ_LFC1 LGZCJGX_OP40"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.BSideSteelBallGreaseVolume),
                "ZP33-08",
                "ZC_XHJ_LFC1_LGZCJGX_OP38"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.BSideSteelBallGroupDiff),
                "ZP33-08",
                "ZC_XHJ_LFC1_LGZCJGX_OP38"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.SealRingParallelDiff),
                "ZP33-05",
                "ZC_XHJ_LFC1_LGZCJGX_OP39"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.PositiveClearanceValue),
                "ZP33-12",
                "ZC_XHJ_LFC1_LGZCJGX_OP42"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.ClearancePressureValue),
                "ZP33-12",
                "ZC_XHJ_LFC1_LGZCJGX_OP42"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.DisplacementValue),
                "ZP33-12",
                "ZC_XHJ_LFC1_LGZCJGX_OP42"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.NegativeClearanceValue),
                "ZP33-12",
                "ZC_XHJ_LFC1_LGZCJGX_OP42"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.SealRingPressPressure),
                "ZP33-12",
                "ZC_XHJ_LFC1_LGZCJGX_OP42"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.MagneticRingParallelSensor1),
                "ZP33-12",
                "ZC_XHJ_LFC1_LGZCJGX_OP42"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.MagneticRingParallelSensor2),
                "ZP33-13",
                "ZC_XHJ_LFC1_LGZCJGX_OP45"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.MagneticRingParallelSensor3),
                "ZP33-13",
                "ZC_XHJ_LFC1_LGZCJGX_OP45"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.MagneticRingParallelAVG),
                "ZP33-13",
                "ZC_XHJ_LFC1_LGZCJGX_OP45"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.EndRunoutAxis),
                "ZP33-15",
                "ZC_XHJ_LFC1_LGZCJGX_OP49"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.EndRunoutPlane),
                "ZP33-15",
                "ZC_XHJ_LFC1_LGZCJGX_OP49"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.EndRunoutHeight),
                "ZP33-15",
                "ZC_XHJ_LFC1_LGZCJGX_OP49"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.RunoutValue),
                "ZP33-15",
                "ZC_XHJ_LFC1_LGZCJGX_OP49"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.TQ_1400),
                "ZP33-17",
                "ZC_XHJ_LFC1_LGZCJGX_OP51"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.M_HIGH_1400),
                "ZP33-17",
                "ZC_XHJ_LFC1_LGZCJGX_OP51"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.P_1600_M1400_ABSSINGLE_MAX),
                "ZP33-17",
                "ZC_XHJ_LFC1_LGZCJGX_OP51"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.P_1600_M1400_ABSSINGLE_min),
                "ZP33-17",
                "ZC_XHJ_LFC1_LGZCJGX_OP51"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.P_1600_M1400_ABSTOTAL),
                "ZP33-17",
                "ZC_XHJ_LFC1_LGZCJGX_OP51"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.P_1600_M1400_ABSTOOTH),
                "ZP33-17",
                "ZC_XHJ_LFC1_LGZCJGX_OP51"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.GONO_CHECK_1150),
                "ZP33-14",
                "ZC_XHJ_LFC1_LGZCJGX_OP47"
            ),
        };

        protected (string Field, string EquipCode, string RouteCode)[] _items63 =
        {
            (
                nameof(Tb_LineSummary.SmallInnerRingSortingData),
                "ZP24-03",
                "ZC_XHJ_LFC3_LGZCJGX_OP26"
            ),
            (nameof(Tb_LineSummary.OuterFlangeSortingData), "ZP24-04", "ZC_XHJ_LFC3_LGZCJGX_OP27"),
            (nameof(Tb_LineSummary.InnerFlangeSortingData), "ZP24-06", "ZC_XHJ_LFC3_LGZCJGX_OP26"),
            (nameof(Tb_LineSummary.ASideSteelBallGroupDiff), "ZP24-12", "ZC XHJ LFC3 LGZCJGX OP35"),
            (
                nameof(Tb_LineSummary.ASideSteelBallGreaseVolume),
                "ZP24-12",
                "ZC XHJ LFC3 LGZCJGX OP35"
            ),
            (nameof(Tb_LineSummary.BSideSteelBallGroupDiff), "ZP24-09", "ZC XHJ_LFC3_LGZCJGX_OP32"),
            (
                nameof(Tb_LineSummary.BSideSteelBallGreaseVolume),
                "ZP24-09",
                "ZC XHJ_LFC3_LGZCJGX_OP32"
            ),
            (nameof(Tb_LineSummary.SealRingGreaseVolume), "ZP24-10", "ZC_XHJ_LFC3_LGZCJGX_OP33"),
            (nameof(Tb_LineSummary.SealRingPressForce), "ZP24-10", "ZC_XHJ_LFC3_LGZCJGX_OP33"),
            (
                nameof(Tb_LineSummary.SealRingPressDisplacementA),
                "ZP24-10",
                "ZC_XHJ_LFC3_LGZCJGX_OP33"
            ),
            (nameof(Tb_LineSummary.SealRingParallelDiff), "ZP24-10", "ZC_XHJ_LFC3_LGZCJGX_OP33"),
            (nameof(Tb_LineSummary.WaterBafflePressForce), "ZP24-11", "ZC_XHJ_LFC3_LGZCJGX_OP34"),
            (
                nameof(Tb_LineSummary.WaterBafflePressDisplacement),
                "ZP24-11",
                "ZC_XHJ_LFC3_LGZCJGX_OP34"
            ),
            (nameof(Tb_LineSummary.WaterBaffleParallelDiff), "ZP24-11", "ZC_XHJ_LFC3_LGZCJGX_OP34"),
            (
                nameof(Tb_LineSummary.SmallInnerRingAssemblePressure),
                "ZP24-03",
                "ZC_XHJ_LFC3_LGZCJGX_OP26"
            ),
            (
                nameof(Tb_LineSummary.SmallInnerRingAssembleDisplacement),
                "ZP24-03",
                "ZC_XHJ_LFC3_LGZCJGX_OP26"
            ),
            (nameof(Tb_LineSummary.BoltPressAssemble1), "ZP24-05", "ZC_XHJ_LFC3_LGZCJGX_OP28"),
            (nameof(Tb_LineSummary.BoltPressAssemble2), "ZP24-05", "ZC_XHJ_LFC3_LGZCJGX_OP28"),
            (nameof(Tb_LineSummary.BoltPressAssemble3), "ZP24-05", "ZC_XHJ_LFC3_LGZCJGX_OP28"),
            (nameof(Tb_LineSummary.BoltPressAssemble4), "ZP24-05", "ZC_XHJ_LFC3_LGZCJGX_OP28"),
            (nameof(Tb_LineSummary.BoltPressAssemble5), "ZP24-05", "ZC_XHJ_LFC3_LGZCJGX_OP28"),
            (nameof(Tb_LineSummary.BoltPressAssembleAVG), "ZP24-05", "ZC_XHJ_LFC3_LGZCJGX_OP28"),
            (nameof(Tb_LineSummary.AbsToothCount), "ZP24-22", "ZC_XHJ_LFC3_LGZCJGX_OP46"),
            (nameof(Tb_LineSummary.TorqueValue), "ZP24-22", "ZC_XHJ_LFC3_LGZCJGX_OP46"),
            (nameof(Tb_LineSummary.AbsPeakValue), "ZP24-22", "ZC_XHJ_LFC3_LGZCJGX_OP46"),
            (nameof(Tb_LineSummary.AbsValleyValue), "ZP24-22", "ZC_XHJ_LFC3_LGZCJGX_OP46"),
            (nameof(Tb_LineSummary.PositiveClearanceValue), "ZP24-25", "ZC_XHJ_LFC3_LGZCJGX_OP37"),
            (nameof(Tb_LineSummary.DisplacementValue), "ZP24-25", "ZC_XHJ_LFC3_LGZCJGX_OP37"),
            (nameof(Tb_LineSummary.NegativeClearanceValue), "ZP24-25", "ZC_XHJ_LFC3_LGZCJGX_OP37"),
            (nameof(Tb_LineSummary.PreRivetingHeight), "ZP24-15", "ZC_XHJ_LFC3_LGZCJGX_OP38"),
            (nameof(Tb_LineSummary.RivetingForce), "ZP24-15", "ZC_XHJ_LFC3_LGZCJGX_OP38"),
            (nameof(Tb_LineSummary.RivetingDisplacement), "ZP24-15", "ZC_XHJ_LFC3_LGZCJGX_OP38"),
            (nameof(Tb_LineSummary.PostRivetingHeight), "ZP24-15", "ZC_XHJ_LFC3_LGZCJGX_OP38"),
            (nameof(Tb_LineSummary.PostRivetingShape), "ZP24-15", "ZC_XHJ_LFC3_LGZCJGX_OP38"),
            (
                nameof(Tb_LineSummary.PostRivetingOuterDiameter),
                "ZP24-15",
                "ZC_XHJ_LFC3_LGZCJGX_OP38"
            ),
            (nameof(Tb_LineSummary.SealRingPressPressure), "ZP24-17", "ZC_XHJ_LFC3_LGZCJGX_OP40"),
            (
                nameof(Tb_LineSummary.SealRingPressDisplacementB),
                "ZP24-17",
                "ZC_XHJ_LFC3_LGZCJGX_OP40"
            ),
            (
                nameof(Tb_LineSummary.MagneticRingParallelSensor1),
                "ZP24-19",
                "ZC_XHJ_LFC3_LGZCJGX_OP40"
            ),
            (
                nameof(Tb_LineSummary.MagneticRingParallelSensor2),
                "ZP24-19",
                "ZC_XHJ_LFC3_LGZCJGX_OP40"
            ),
            (
                nameof(Tb_LineSummary.MagneticRingParallelSensor3),
                "ZP24-19",
                "ZC_XHJ_LFC3_LGZCJGX_OP40"
            ),
            (
                nameof(Tb_LineSummary.MagneticRingParallelDiff),
                "ZP24-19",
                "ZC_XHJ_LFC3_LGZCJGX_OP40"
            ),
            (nameof(Tb_LineSummary.GearRingPressPressure), "ZP24-17", "ZC_XHJ_LFC3_LGZCJGX_OP40"),
            (
                nameof(Tb_LineSummary.GearRingPressDisplacement),
                "ZP24-17",
                "ZC_XHJ_LFC3_LGZCJGX_OP40"
            ),
            (nameof(Tb_LineSummary.RadialRunout), "ZP24-20", "ZC_XHJ_LFC3_LGZCJGX_OP44"),
            (nameof(Tb_LineSummary.EndRunout), "ZP24-20", "ZC_XHJ_LFC3_LGZCJGX_OP44"),
        };
    }
}
