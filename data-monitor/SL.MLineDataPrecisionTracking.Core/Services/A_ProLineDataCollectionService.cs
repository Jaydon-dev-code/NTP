using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.Ocsp;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    public class A_ProLineDataCollectionService : ProLineDataCollectionServiceAbstract<Tb_LineA>
    {
        protected override string _lineName { get; set; } = "A线";
        Tb_LineARepository _lineARepository;
        Tb_LineSummaryRepository _ineSummaryRepository;
        Tb_ModelNoToNameRepository _modelNoToNameRepository;

        public A_ProLineDataCollectionService(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp,
            Tb_LineARepository tb_LineARepository,
            Tb_LineSummaryRepository tb_LineSummaryRepository,
            Tb_ModelNoToNameRepository tb_ModelNoToNameRepository
        )
            : base(equipmentRepositor, mcp)
        {
            _lineARepository = tb_LineARepository;
            _ineSummaryRepository = tb_LineSummaryRepository;
            _modelNoToNameRepository = tb_ModelNoToNameRepository;
        }

        protected override async Task<bool> InsterCollectionData(Tb_LineA data)
        {
            if (data.NgCodeA != "0")
            {
                Tb_LineSummary tb_LineSummary = new Tb_LineSummary() { Result = ResultEnum.NG };
                ABToSummary(data, null, tb_LineSummary); //查询模型对应的信息
                var models = await _modelNoToNameRepository.QueryabletAsync(x => true);
                //给模型名称
                tb_LineSummary.ModelNo = data.ModelNoA;
                var modelNameB = models
                    .FirstOrDefault(x => x.ModelNo == tb_LineSummary.ModelNo)
                    ?.ModelName;
                tb_LineSummary.ModelName = modelNameB == null ? "" : modelNameB;
                await _ineSummaryRepository.InsertableAsync(tb_LineSummary);
                data.IsUsing = true;
            }
            return await _lineARepository.InsertableAsync(data) > 0;
        }
    }
}
