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
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    public class B_ProLineDataCollectionService : ProLineDataCollectionServiceAbstract
    {
        Tb_LineBRepository _lineBRepository;
        Tb_LineARepository _lineARepository;
        Tb_LineSummaryRepository _lineSummaryRepository;
        Tb_ModelNoToNameRepository _modelNoToNameRepository;

        protected override string _lineName { get; set; } = "B线";
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
            _lineBRepository = tb_LineBRepository;
            _lineARepository = tb_LineARepository;
            _lineSummaryRepository = tb_LineSummaryRepository;
            _modelNoToNameRepository = tb_ModelNoToNameRepository;
        }

        protected override async Task<bool> InsterCollectionData(object data)
        {
            var lineData = data as Tb_LineB;
            var aLineInfo = await _lineARepository.QueryableFirstAsync(
                x => x.TrayNoA == lineData.LineATrayNo && x.IsUsing == false,
                o => o.RecordTime
            );
            Tb_LineSummary tb_LineSummary = new Tb_LineSummary() { Result = ResultEnum.OK };
            if (aLineInfo == null || lineData.NgCodeB != "0")
            {
                tb_LineSummary.Result = ResultEnum.NG;
            }
            await _lineBRepository.InsertableAsync(lineData);
            ABToSummary(aLineInfo, lineData, tb_LineSummary, new List<string>() { "A线托盘编号" });
            var models = await _modelNoToNameRepository.QueryabletAsync(x => true);
            tb_LineSummary.ModelNo = lineData.ModelNoB;
            var modelNameB = models
                .FirstOrDefault(x => x.ModelNo == tb_LineSummary.ModelNo)
                ?.ModelName;
            tb_LineSummary.ModelName = modelNameB == null ? "" : modelNameB;
            if (aLineInfo != null)
            {
                aLineInfo.IsUsing = true;
                await _lineARepository.UpdateableAsync(aLineInfo);
            }

            return await _lineSummaryRepository.InsertableAsync(tb_LineSummary) > 0;
        }

        protected override bool OtherCanCollection()
        {
            return true;
        }

        protected override void OtherInit()
        {
          
        }
    }
}