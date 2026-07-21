using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_3;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_1AssemblyLine;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_1
{
    public class Factory6Workshop6_1AssemblyLineB : Factory6Workshop6_3AssemblyLineAbstract
    {
        protected virtual Type _passCodeEnumType => typeof(Assembly_6Factory6_1BLine_PassCodeEnum);
        protected virtual Type _ngCodeEnumType => typeof(Assembly_6Factory6_1BLine_NgCodeEnum);

        Tb_Factory6Workshop6_1AssemblyLineARepository _factory6Workshop6_1AssemblyLineARepository;
        Tb_Factory6Workshop6_1AssemblyLineABSummaryRepository _factory6Workshop6_1AssemblyLineABSummaryRepository;
        Tb_Factory6Workshop6_1AssemblyLineBRepository _factory6Workshop6_1AssemblyLineBRepository;

        public Factory6Workshop6_1AssemblyLineB(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp,
            Tb_ModelNoToNameRepository tb_ModelNoToNameRepository,
            Tb_Factory6Workshop6_1AssemblyLineARepository factory6Workshop6_1AssemblyLineARepository,
            Tb_Factory6Workshop6_1AssemblyLineABSummaryRepository factory6Workshop6_1AssemblyLineABSummaryRepository,
            Tb_Factory6Workshop6_1AssemblyLineBRepository tb_Factory6Workshop6_1AssemblyLineBRepository
        )
            : base(equipmentRepositor, mcp, tb_ModelNoToNameRepository)
        {
            _factory6Workshop6_1AssemblyLineARepository =
                factory6Workshop6_1AssemblyLineARepository;
            _factory6Workshop6_1AssemblyLineABSummaryRepository =
                factory6Workshop6_1AssemblyLineABSummaryRepository;
            _factory6Workshop6_1AssemblyLineBRepository =
                tb_Factory6Workshop6_1AssemblyLineBRepository;
            _sleepTimeSpan = TimeSpan.FromMilliseconds(200);
        }

        protected override string _lineName => "六分厂6-1装配B线";

        protected override Type _dataModelType => typeof(Tb_Factory6Workshop6_1AssemblyLineB);

        protected override string _trayPointName => "托盘号B";

        protected override string _serviceName => "六分厂6-1装配B线";

        protected override async Task<object> InsterValue(Result<object> interact)
        {
            var lineData = (Tb_Factory6Workshop6_1AssemblyLineB)interact.Data;

            lineData.VibrationDetectionResults= GetVibrationDetectionResults(lineData?.VibrationDetectionResults?.Trim());

            Tb_Factory6Workshop6_1AssemblyLineABSummary tb_LineSummary =
              new Tb_Factory6Workshop6_1AssemblyLineABSummary() { Result = ResultEnum.OK };
            Tb_Factory6Workshop6_1AssemblyLineA aLineInfo = null;
            if (lineData.LineATrayNo != "0" && !string.IsNullOrEmpty(lineData.LineATrayNo))
            {
                aLineInfo = await _factory6Workshop6_1AssemblyLineARepository.QueryableFirstAsync(
                    x => x.TrayNoA == lineData.LineATrayNo,
                    o => o.RecordTime
                );
            }
            else
            {
                tb_LineSummary.TrayNoA = lineData.LineATrayNo;
            }

          

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

            var bLineFid =
                await _factory6Workshop6_1AssemblyLineBRepository.InsertableReturnIdentityAsync(
                    lineData
                );
            ABToSummary(aLineInfo, lineData, tb_LineSummary, new List<string>() { "A线托盘编号" });

            tb_LineSummary.ModelNo = lineData.ModelNoB;
            var modelNameB = _models
                .FirstOrDefault(x => x.ModelNo == tb_LineSummary.ModelNo)
                ?.ModelName;
            tb_LineSummary.ModelName = modelNameB ?? "";
            tb_LineSummary.BLineFID = bLineFid;
            await _factory6Workshop6_1AssemblyLineABSummaryRepository.InsertableAsync(
                tb_LineSummary
            );
            return lineData;
        }

        private string GetVibrationDetectionResults(string vibrationDetectionResults)
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

        protected override void UpLastNo(object data)
        {
            _lastTrayNoPoint = ((Tb_Factory6Workshop6_1AssemblyLineB)data).TrayNoB;
        }
    }
}
