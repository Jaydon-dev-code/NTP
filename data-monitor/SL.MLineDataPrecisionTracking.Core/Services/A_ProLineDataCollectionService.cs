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
    public class A_ProLineDataCollectionService: ProLineDataCollectionServiceAbstract<Tb_LineA>
    {
        protected override string _lineName { get; set; } = "A线";
        Tb_LineARepository _lineARepository;
        public A_ProLineDataCollectionService(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp,Tb_LineARepository tb_LineARepository
        )
            : base(equipmentRepositor, mcp) {
            _lineARepository = tb_LineARepository;
        }

        protected override async Task<bool> InsterCollectionData(Tb_LineA data)
        {
            return await _lineARepository.InsertableAsync(data) > 0;
        }
    }
}
