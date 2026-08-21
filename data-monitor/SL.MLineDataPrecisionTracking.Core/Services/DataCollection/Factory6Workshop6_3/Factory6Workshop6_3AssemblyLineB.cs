using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_3
{
    public class Factory6Workshop6_3AssemblyLineB : Factory6Workshop6_3AssemblyLineAbstract
    {
        protected override string _lineName => "B线";
        protected override Type _dataModelType => typeof(Tb_LineB);
        protected override string _trayPointName => "托盘号B";
        protected override string _serviceName => "六分厂6-3装配B线";
        protected virtual Type _passCodeEnumType => typeof(Assembly_B_LinePassCodeEnum);
        protected virtual Type _ngCodeEnumType => typeof(Assembly_B_LineNgCodeEnum);

        Tb_LineBRepository _LineBRepository;
        Tb_LineARepository _lineARepository;
        Tb_LineSummaryRepository _lineSummaryRepository;

        public Factory6Workshop6_3AssemblyLineB(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp,
            Tb_LineBRepository tb_LineBRepository,
            Tb_LineARepository tb_LineARepository,
            Tb_LineSummaryRepository tb_LineSummaryRepository,
            Tb_ModelNoToNameRepository tb_ModelNoToNameRepository
        )
            : base(equipmentRepositor, mcp, tb_ModelNoToNameRepository)
        {
            _LineBRepository = tb_LineBRepository;
            _lineARepository = tb_LineARepository;
            _lineSummaryRepository = tb_LineSummaryRepository;
        }

        protected override void UpLastNo(object data)
        {
            _lastTrayNoPoint = ((Tb_LineB)data).TrayNoB;
        }

        protected override async Task<object> InsterValue(Result<object> interact)
        {
            var lineData = (Tb_LineB)interact.Data;

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
            if (aLineInfo == null && lineData.LineATrayNo != "0" && !string.IsNullOrEmpty(lineData.LineATrayNo))
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

            return lineData;
        }
    }
}
