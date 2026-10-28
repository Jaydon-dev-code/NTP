using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using NPOI.POIFS.Crypt.Standard;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_3;
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

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_1
{
    public class Factory6Workshop6_1AssemblyLineB : Factory6Workshop6_3AssemblyLineAbstract
    {
        protected virtual Type _passCodeEnumType => typeof(Assembly_6Factory6_1BLine_PassCodeEnum);
        protected virtual Type _ngCodeEnumType => typeof(Assembly_6Factory6_1BLine_NgCodeEnum);

        Tb_Factory6Workshop6_1AssemblyLineARepository _factory6Workshop6_1AssemblyLineARepository;
        Tb_Factory6Workshop6_1AssemblyLineABSummaryRepository _factory6Workshop6_1AssemblyLineABSummaryRepository;
        Tb_Factory6Workshop6_1AssemblyLineBRepository _factory6Workshop6_1AssemblyLineBRepository;
        MesOrderOP _mesOrderOP;
        MesApiOp _mesApiOp;
        Dictionary<string, SugarColumn> _fieldAttributeDic;
        protected override string _lineName => "六分厂6-1装配B线";

        protected override Type _dataModelType => typeof(Tb_Factory6Workshop6_1AssemblyLineB);

        protected override string _trayPointName => "托盘号B";

        protected override string _serviceName => "六分厂6-1装配B线";

        public Factory6Workshop6_1AssemblyLineB(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp,
            Tb_ModelNoToNameRepository tb_ModelNoToNameRepository,
            Tb_Factory6Workshop6_1AssemblyLineARepository factory6Workshop6_1AssemblyLineARepository,
            Tb_Factory6Workshop6_1AssemblyLineABSummaryRepository factory6Workshop6_1AssemblyLineABSummaryRepository,
            Tb_Factory6Workshop6_1AssemblyLineBRepository tb_Factory6Workshop6_1AssemblyLineBRepository,
            MesOrderOP mesOrderOP,
            MesApiOp mesApiOp
        )
            : base(equipmentRepositor, mcp, tb_ModelNoToNameRepository)
        {
            _factory6Workshop6_1AssemblyLineARepository =
                factory6Workshop6_1AssemblyLineARepository;
            _factory6Workshop6_1AssemblyLineABSummaryRepository =
                factory6Workshop6_1AssemblyLineABSummaryRepository;
            _factory6Workshop6_1AssemblyLineBRepository =
                tb_Factory6Workshop6_1AssemblyLineBRepository;
            _mesOrderOP = mesOrderOP;
            _mesApiOp = mesApiOp;
            _fieldAttributeDic = (
                new Tb_Factory6Workshop6_1AssemblyLineABSummary()
            ).GetFieldAttributeDict<SugarColumn>();
            _sleepTimeSpan = TimeSpan.FromMilliseconds(200);
        }

        protected override async Task<object> InsterValue(Result<object> interact)
        {
            var lineData = (Tb_Factory6Workshop6_1AssemblyLineB)interact.Data;

            lineData.VibrationDetectionResults = GetVibrationDetectionResults(
                lineData?.VibrationDetectionResults?.Trim()
            );

            Tb_Factory6Workshop6_1AssemblyLineABSummary tb_LineSummary =
                new Tb_Factory6Workshop6_1AssemblyLineABSummary() { Result = ResultEnum.OK };
            Tb_Factory6Workshop6_1AssemblyLineA aLineInfo = null;

            // 从数据库查 a 线已绑定（a b 托盘相同）的最新未使用数据
            aLineInfo = await _factory6Workshop6_1AssemblyLineARepository.QueryableFirstAsync(
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
            var bLineFid =
                await _factory6Workshop6_1AssemblyLineBRepository.InsertableReturnIdentityAsync(
                    lineData
                );
            ABToSummary(aLineInfo, lineData, tb_LineSummary, new List<string>() { "A线托盘编号" });
            if (
                aLineInfo == null
                && lineData.LineATrayNo != "0"
                && !string.IsNullOrEmpty(lineData.LineATrayNo)
            )
            {
                // 未找到绑定：只搞b线，并标记a线托盘号为0
                tb_LineSummary.LineATrayNo = tb_LineSummary.LineATrayNo;
            }
            tb_LineSummary.ModelNo = lineData.ModelNoB;
            var modelNameB = _models
                .FirstOrDefault(x => x.ModelNo == tb_LineSummary.ModelNo)
                ?.ModelName;
            tb_LineSummary.ModelName = modelNameB ?? "";
            tb_LineSummary.BLineFID = bLineFid;
            await _factory6Workshop6_1AssemblyLineABSummaryRepository.InsertableAsync(
                tb_LineSummary
            );
            // 把 a 线数据标记为已使用
            if (aLineInfo != null)
            {
                aLineInfo.IsUsed = true;
                await _factory6Workshop6_1AssemblyLineARepository.UpdateableAsync(aLineInfo);
            }
            var buildValues = BuildMesUploadItems(tb_LineSummary);
            if (buildValues?.Count > 0)
            {
                await _mesApiOp.DataAcquisition(buildValues);
            }

  

            return lineData;
        }

        private List<MesDataAcquisitionUploadItemDTO> BuildMesUploadItems(
            Tb_Factory6Workshop6_1AssemblyLineABSummary tb_LineSummary
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
                    typeof(Tb_Factory6Workshop6_1AssemblyLineABSummary)
                        .GetProperty(item.Field)
                        ?.GetValue(tb_LineSummary) as string;
                if (string.IsNullOrEmpty(val))
                {
                    continue;
                }
                re.Add(
                    new MesDataAcquisitionUploadItemDTO
                    {
                        PlantCode = "61",
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
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.ASideSteelBallGroupDiff),
                "ZP33-10",
                "OP700"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.ASideSteelBallGreaseVolume),
                "ZP33-10",
                "OP700"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.BSideSteelBallGreaseVolume),
                "ZP33-08",
                "OP510"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.BSideSteelBallGroupDiff),
                "ZP33-08",
                "OP510"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.SealRingParallelDiff),
                "ZP33-05",
                "OP230"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.PositiveClearanceValue),
                "ZP33-12",
                "OP900"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.ClearancePressureValue),
                "ZP33-12",
                "OP900"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.DisplacementValue),
                "ZP33-12",
                "OP900"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.NegativeClearanceValue),
                "ZP33-12",
                "OP900"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.SealRingPressPressure),
                "ZP33-12",
                "OP900"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.MagneticRingParallelSensor1),
                "ZP33-12",
                "OP900"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.MagneticRingParallelSensor2),
                "ZP33-13",
                "0P1000"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.MagneticRingParallelSensor3),
                "ZP33-13",
                "0P1000"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.MagneticRingParallelAVG),
                "ZP33-13",
                "0P1000"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.EndRunoutAxis),
                "ZP33-15",
                "OP1200"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.EndRunoutPlane),
                "ZP33-15",
                "OP1200"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.EndRunoutHeight),
                "ZP33-15",
                "OP1200"
            ),
            (nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.RunoutValue), "ZP33-15", "OP1200"),
            (nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.TQ_1400), "ZP33-17", "OP1400"),
            (nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.M_HIGH_1400), "ZP33-17", "OP1400"),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.P_1600_M1400_ABSSINGLE_MAX),
                "ZP33-17",
                "OP1400"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.P_1600_M1400_ABSSINGLE_min),
                "ZP33-17",
                "OP1400"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.P_1600_M1400_ABSTOTAL),
                "ZP33-17",
                "OP1400"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.P_1600_M1400_ABSTOOTH),
                "ZP33-17",
                "OP1400"
            ),
            (
                nameof(Tb_Factory6Workshop6_1AssemblyLineABSummary.GONO_CHECK_1150),
                "ZP33-14",
                "OP1150"
            ),
        };

        protected override void UpLastNo(object data)
        {
            _lastTrayNoPoint = ((Tb_Factory6Workshop6_1AssemblyLineB)data).TrayNoB;
        }
    }
}
