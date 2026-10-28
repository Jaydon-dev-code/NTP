using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Core.Services.Mes;
using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_1AssemblyLine;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_3
{
    public class Factory6Workshop6_3AssemblyLineB : Factory6Workshop6_3AssemblyLineAbstract
    {
        protected override string _lineName => "六分厂6-3装配B线";
        protected override Type _dataModelType => typeof(Tb_LineB);
        protected override string _trayPointName => "托盘号B";
        protected override string _serviceName => "六分厂6-3装配B线";
        protected virtual Type _passCodeEnumType => typeof(Assembly_B_LinePassCodeEnum);
        protected virtual Type _ngCodeEnumType => typeof(Assembly_B_LineNgCodeEnum);

        Tb_LineBRepository _LineBRepository;
        Tb_LineARepository _lineARepository;
        Tb_LineSummaryRepository _lineSummaryRepository;
        MesOrderOP _mesOrderOP;
        MesApiOp _mesApiOp;
        Dictionary<string, SugarColumn> _fieldAttributeDic;

        public Factory6Workshop6_3AssemblyLineB(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp,
            Tb_LineBRepository tb_LineBRepository,
            Tb_LineARepository tb_LineARepository,
            Tb_LineSummaryRepository tb_LineSummaryRepository,
            Tb_ModelNoToNameRepository tb_ModelNoToNameRepository,
            MesOrderOP mesOrderOP,
            MesApiOp mesApiOp
        )
            : base(equipmentRepositor, mcp, tb_ModelNoToNameRepository)
        {
            _LineBRepository = tb_LineBRepository;
            _lineARepository = tb_LineARepository;
            _lineSummaryRepository = tb_LineSummaryRepository;
            _mesOrderOP = mesOrderOP;
            _mesApiOp = mesApiOp;
            _fieldAttributeDic = (new Tb_LineSummary()).GetFieldAttributeDict<SugarColumn>();
        }

        protected override void UpLastNo(object data)
        {
            _lastTrayNoPoint = ((Tb_LineB)data).TrayNoB;
        }

        protected override async Task<object> InsterValue(Result<object> interact)
        {
            var lineData = (Tb_LineB)interact.Data;
            lineData.VibrationDetectionResults = GetVibrationDetectionResults(
                lineData?.VibrationDetectionResults?.Trim()
            );
            Tb_LineSummary tb_LineSummary = new Tb_LineSummary() { Result = ResultEnum.OK };
            Tb_LineA aLineInfo = null;

            // 从数据库查 a 线已绑定（a b 托盘相同）的最新未使用数据
            aLineInfo = await _lineARepository.QueryableFirstAsync(
                x =>
                    x.TrayNoA == lineData.LineATrayNo
                    && x.TrayNoB == lineData.TrayNoB
                    && x.IsUsed == false,
                o => o.RecordTime
            );

            if (lineData.ShieldStationB != "0")
            {
                if (int.TryParse(lineData.ShieldStationB, out var shieldStationB))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"({lineData.ShieldStationB}) ");
                    foreach (var item in ParseBitEnum(shieldStationB, _passCodeEnumType))
                    {
                        stringBuilder.Append(item.Description + " ");
                    }
                    lineData.ShieldStationB = stringBuilder.ToString();
                }
            }

            if (lineData.NgCodeB != "0")
            {
                if (int.TryParse(lineData.NgCodeB, out var ngCodeB))
                {
                    lineData.NgCodeB =
                        $"({ngCodeB}) {GetEnumDescription(ngCodeB, _ngCodeEnumType)}";
                }
                lineData.MarkingNo = "";
                tb_LineSummary.Result = ResultEnum.NG;
            }

            if (aLineInfo != null)
            {
                lineData.ALineFID = aLineInfo.Id;
                lineData.ALineRecordTime = aLineInfo.RecordTime;
            }
            if (aLineInfo == null)
            {
                // 未找到绑定：只搞b线，并标记a线托盘号为0
                lineData.LineATrayNo = "!" + lineData.LineATrayNo;
            }

            var bLineFid = await _LineBRepository.InsertableReturnIdentityAsync(lineData);
            ABToSummary(aLineInfo, lineData, tb_LineSummary, new List<string>() { "A线托盘编号" });
            if (
                aLineInfo == null
                && lineData.LineATrayNo != "0"
                && !string.IsNullOrEmpty(lineData.LineATrayNo)
            )
            {
                tb_LineSummary.LineATrayNo = tb_LineSummary.LineATrayNo;
            }

            tb_LineSummary.ModelNo = lineData.ModelNoB;
            var modelNameB = _models
                .FirstOrDefault(x => x.ModelNo == tb_LineSummary.ModelNo)
                ?.ModelName;
            tb_LineSummary.ModelName = modelNameB ?? "";
            tb_LineSummary.BLineFID = bLineFid;
            await _lineSummaryRepository.InsertableAsync(tb_LineSummary);

            // 把 a 线数据标记为已使用
            if (aLineInfo != null)
            {
                aLineInfo.IsUsed = true;
                await _lineARepository.UpdateableAsync(aLineInfo);
            }
            var buildValue = BuildMesUploadItems(tb_LineSummary);
            if (buildValue?.Count > 0)
            {
                await _mesApiOp.DataAcquisition(buildValue);
            }

            return lineData;
        }

        private List<MesDataAcquisitionUploadItemDTO> BuildMesUploadItems(
            Tb_LineSummary tb_LineSummary
        )
        {
            var serialNo = tb_LineSummary.MarkingNo;
            var testResult = tb_LineSummary.Result.ToString();
            var workOrderNo = _mesOrderOP.OrderNo;
            var testTime = tb_LineSummary.RecordTime.ToString("yyyy-MM-dd HH:mm:ss");
            var re = new List<MesDataAcquisitionUploadItemDTO>();
            foreach (var item in _items)
            {
                var val =
                    typeof(Tb_LineSummary).GetProperty(item.Field)?.GetValue(tb_LineSummary)
                    as string;
                if (string.IsNullOrEmpty(val))
                {
                    continue;
                }
                re.Add(
                    new MesDataAcquisitionUploadItemDTO
                    {
                        PlantCode = "63",
                        EquipCode = item.EquipCode,
                        RouteCode = item.RouteCode,
                        SerialNo = serialNo,
                        TestResult = testResult,
                        TestCode = _fieldAttributeDic[item.Field].ColumnDescription,
                        WorkOrderNo = workOrderNo,
                        TestValue = val,
                        TestTime = testTime,
                    }
                );
            }

            return re;
        }

        // 每项仅 字段名/设备编码/工序编码 不同，其余取值共用
        (string Field, string EquipCode, string RouteCode)[] _items =
        {
            (nameof(Tb_LineSummary.SmallInnerRingSortingData), "ZP24-03", "OP310"),
            (nameof(Tb_LineSummary.OuterFlangeSortingData), "ZP24-04", "OP320"),
            (nameof(Tb_LineSummary.InnerFlangeSortingData), "ZP24-06", "OP330"),
            (nameof(Tb_LineSummary.ASideSteelBallGroupDiff), "ZP24-12", "OP700"),
            (nameof(Tb_LineSummary.ASideSteelBallGreaseVolume), "ZP24-12", "OP700"),
            (nameof(Tb_LineSummary.BSideSteelBallGroupDiff), "ZP24-09", "OP610"),
            (nameof(Tb_LineSummary.BSideSteelBallGreaseVolume), "ZP24-09", "OP610"),
            (nameof(Tb_LineSummary.SealRingGreaseVolume), "ZP24-10", "OP630"),
            (nameof(Tb_LineSummary.SealRingPressForce), "ZP24-10", "OP630"),
            (nameof(Tb_LineSummary.SealRingPressDisplacementA), "ZP24-10", "OP630"),
            (nameof(Tb_LineSummary.SealRingParallelDiff), "ZP24-10", "OP630"),
            (nameof(Tb_LineSummary.WaterBafflePressForce), "ZP24-11", "OP680"),
            (nameof(Tb_LineSummary.WaterBafflePressDisplacement), "ZP24-11", "OP680"),
            (nameof(Tb_LineSummary.WaterBaffleParallelDiff), "ZP24-11", "OP680"),
            (nameof(Tb_LineSummary.SmallInnerRingAssemblePressure), "ZP24-03", "OP310"),
            (nameof(Tb_LineSummary.SmallInnerRingAssembleDisplacement), "ZP24-03", "OP310"),
            (nameof(Tb_LineSummary.BoltPressAssemble1), "ZP24-05", "OP200"),
            (nameof(Tb_LineSummary.BoltPressAssemble2), "ZP24-05", "OP200"),
            (nameof(Tb_LineSummary.BoltPressAssemble3), "ZP24-05", "OP200"),
            (nameof(Tb_LineSummary.BoltPressAssemble4), "ZP24-05", "OP200"),
            (nameof(Tb_LineSummary.BoltPressAssemble5), "ZP24-05", "OP200"),
            (nameof(Tb_LineSummary.BoltPressAssembleAVG), "ZP24-05", "OP200"),
            (nameof(Tb_LineSummary.AbsToothCount), "ZP24-22", "OP1700"),
            (nameof(Tb_LineSummary.TorqueValue), "ZP24-22", "OP1700"),
            (nameof(Tb_LineSummary.AbsPeakValue), "ZP24-22", "OP1700"),
            (nameof(Tb_LineSummary.AbsValleyValue), "ZP24-22", "OP1700"),
            (nameof(Tb_LineSummary.PositiveClearanceValue), "ZP24-25", "OP900"),
            (nameof(Tb_LineSummary.DisplacementValue), "ZP24-25", "OP900"),
            (nameof(Tb_LineSummary.NegativeClearanceValue), "ZP24-25", "OP900"),
            (nameof(Tb_LineSummary.PreRivetingHeight), "ZP24-15", "OP1000"),
            (nameof(Tb_LineSummary.RivetingForce), "ZP24-15", "OP1000"),
            (nameof(Tb_LineSummary.RivetingDisplacement), "ZP24-15", "OP1000"),
            (nameof(Tb_LineSummary.PostRivetingHeight), "ZP24-15", "OP1000"),
            (nameof(Tb_LineSummary.PostRivetingShape), "ZP24-15", "OP1000"),
            (nameof(Tb_LineSummary.PostRivetingOuterDiameter), "ZP24-15", "OP1000"),
            (nameof(Tb_LineSummary.SealRingPressPressure), "ZP24-17", "OP1100"),
            (nameof(Tb_LineSummary.SealRingPressDisplacementB), "ZP24-17", "OP1100"),
            (nameof(Tb_LineSummary.MagneticRingParallelSensor1), "ZP24-19", "OP1300"),
            (nameof(Tb_LineSummary.MagneticRingParallelSensor2), "ZP24-19", "OP1300"),
            (nameof(Tb_LineSummary.MagneticRingParallelSensor3), "ZP24-19", "OP1300"),
            (nameof(Tb_LineSummary.MagneticRingParallelDiff), "ZP24-19", "OP1300"),
            (nameof(Tb_LineSummary.GearRingPressPressure), "ZP24-17", "OP1100"),
            (nameof(Tb_LineSummary.GearRingPressDisplacement), "ZP24-17", "OP1100"),
            (nameof(Tb_LineSummary.RadialRunout), "OP24-20", "OP1500"),
            (nameof(Tb_LineSummary.EndRunout), "OP24-20", "OP1500"),
        };
    }
}
