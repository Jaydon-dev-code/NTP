using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    public class B_ProLineDataCollectionService : ProLineDataCollectionServiceAbstract
    {
        DevPlcPointDto _plcCallPCTrayNoPoint;
        string _lastTrayNoPoint;
        Tb_LineBRepository _lineBRepository;
        Tb_LineARepository _lineARepository;
        Tb_LineSummaryRepository _lineSummaryRepository;
        Tb_ModelNoToNameRepository _modelNoToNameRepository;
        List<Tb_ModelNoToName> _models;
        protected override string[] _lineName { get; set; } = new string[] { "B线", "六分厂6-1装配B线" };
        protected override Type DataModelType => typeof(Tb_LineB);

        public B_ProLineDataCollectionService(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp,
            Tb_LineBRepository tb_LineBRepository,
            Tb_LineARepository tb_LineARepository,
            Tb_LineSummaryRepository tb_LineSummaryRepository,
            Tb_ModelNoToNameRepository tb_ModelNoToNameRepository
        )
            : base(equipmentRepositor, mcp)
        {
            ServiceName = "B线";
            _lineBRepository = tb_LineBRepository;
            _lineARepository = tb_LineARepository;
            _lineSummaryRepository = tb_LineSummaryRepository;
            _modelNoToNameRepository = tb_ModelNoToNameRepository;
        }

        protected override async Task<bool> InsterCollectionData(object data)
        {
            var lineData = data as Tb_LineB;
            Tb_LineA aLineInfo = null;
            if (lineData.LineATrayNo != "0" && !string.IsNullOrEmpty(lineData.LineATrayNo))
            {
                aLineInfo = await _lineARepository.QueryableFirstAsync(
                    x => x.TrayNoA == lineData.LineATrayNo,
                    o => o.RecordTime
                );
            }

            Tb_LineSummary tb_LineSummary = new Tb_LineSummary() { Result = ResultEnum.OK };
            if (lineData.ShieldStationB != "0")
            {
                if (int.TryParse(lineData.ShieldStationB, out var shieldStationB))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"({lineData.ShieldStationB}) ");
                    if (ServiceName == "B线")
                    {
                        foreach (var item in shieldStationB.ParseBitEnum<Assembly_B_LinePassCodeEnum>())
                        {
                            stringBuilder.Append(item.Description + " ");
                        }
                    }
                    else
                    {
                        foreach (var item in shieldStationB.ParseBitEnum<Assembly_6Factory6_1BLine_PassCodeEnum>())
                        {
                            stringBuilder.Append(item.Description + " ");
                        }
                    }
                       
                    lineData.ShieldStationB = stringBuilder.ToString();
                }
            }
            if (lineData.NgCodeB != "0")
            {
                if (int.TryParse(lineData.NgCodeB, out var ngCodeB))
                {
                    if (ServiceName == "B线")
                    {
                        lineData.NgCodeB =
                        $"({ngCodeB}) {((Assembly_B_LineNgCodeEnum)ngCodeB).GetDescription()}";
                    }
                    else
                    {
                        lineData.NgCodeB =
                        $"({ngCodeB}) {((Assembly_6Factory6_1BLine_NgCodeEnum)ngCodeB).GetDescription()}";

                    }
                }
                lineData.MarkingNo = "";
                tb_LineSummary.Result = ResultEnum.NG;
            }
            if (aLineInfo != null)
            {
                lineData.ALineFID = aLineInfo.Id;
                lineData.ALineRecordTime = aLineInfo.RecordTime;
            }
            var bLineFid = await _lineBRepository.InsertableReturnIdentityAsync(lineData);
            ABToSummary(aLineInfo, lineData, tb_LineSummary, new List<string>() { "A线托盘编号" });

            tb_LineSummary.ModelNo = lineData.ModelNoB;
            var modelNameB = _models
                .FirstOrDefault(x => x.ModelNo == tb_LineSummary.ModelNo)
                ?.ModelName;
            tb_LineSummary.ModelName = modelNameB == null ? "" : modelNameB;
            tb_LineSummary.BLineFID = bLineFid;
            return await _lineSummaryRepository.InsertableAsync(tb_LineSummary) > 0;
        }

        protected override async Task OtherInitAsync()
        {
            _plcCallPCTrayNoPoint = _lineReadPlcInfo.First(x => x.PointName == "托盘号B");
            _models = await _modelNoToNameRepository.QueryabletAsync(x => true);
        }

        protected override bool OtherCanCollection()
        {
            var re = _mcp.Read(_plcCallPCTrayNoPoint);
            if (
                re.IsSuccess is false
                || re.Data.Value[0].ToString() == "0"
                || re.Data.Value[0].ToString() == _lastTrayNoPoint
            )
            {
                return false;
            }
            else
            {
                _lastTrayNoPoint = re.Data.Value[0].ToString();

                return true;
            }
        }
    }
}
