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
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    public class B_ProLineDataCollectionService : ProLineDataCollectionServiceAbstract<Tb_LineB>
    {
        Tb_LineBRepository _lineBRepository;
        Tb_LineARepository _lineARepository;
        Tb_LineSummaryRepository _lineSummaryRepository;
        Tb_ModelNoToNameRepository _modelNoToNameRepository;

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

        protected override string _lineName { get; set; } = "B线";

        protected override async Task<bool> InsterCollectionData(Tb_LineB data)
        {
            var aLineInfo = await _lineARepository.QueryableFirstAsync(
                x => x.TrayNoA == data.LineATrayNo && x.IsUsing == false,
                o => o.RecordTime
            );
            Tb_LineSummary tb_LineSummary = new Tb_LineSummary() { Result = ResultEnum.OK };
            //找不到A 托盘或者 自己 ng了就是ng
            if (aLineInfo == null || data.NgCodeB != "0")
            {
                tb_LineSummary.Result = ResultEnum.NG;
            }
            await _lineBRepository.InsertableAsync(data);
            //通过 a托盘号 找到 a的信息 后生成记录
            ABToSummary(aLineInfo, data, tb_LineSummary, new List<string>() { "A线托盘编号" });
            //查询模型对应的信息
            var models = await _modelNoToNameRepository.QueryabletAsync(x => true);
            //给模型名称
            tb_LineSummary.ModelNo = data.ModelNoB;
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
    }
}
