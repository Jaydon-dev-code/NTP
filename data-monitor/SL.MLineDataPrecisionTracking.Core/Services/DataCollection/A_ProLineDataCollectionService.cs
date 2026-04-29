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
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;
using SqlSugar.Extensions;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    public class A_ProLineDataCollectionService : ProLineDataCollectionServiceAbstract
    {
        DevPlcPointMcDto _plcCallPCTrayNoPoint;
        string _lastTrayNoPoint;
        protected override string _lineName { get; set; } = "A线";
        protected override Type DataModelType => typeof(Tb_LineA);

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

        protected override async Task<bool> InsterCollectionData(object data)
        {
            var lineData = data as Tb_LineA;
            if (lineData.NgCodeA != "0")
            {
                Tb_LineSummary tb_LineSummary = new Tb_LineSummary() { Result = ResultEnum.NG };
                ABToSummary(lineData, null, tb_LineSummary);
                var models = await _modelNoToNameRepository.QueryabletAsync(x => true);
                tb_LineSummary.ModelNo = lineData.ModelNoA;
                var modelNameB = models
                    .FirstOrDefault(x => x.ModelNo == tb_LineSummary.ModelNo)
                    ?.ModelName;
                tb_LineSummary.ModelName = modelNameB == null ? "" : modelNameB;
                await _ineSummaryRepository.InsertableAsync(tb_LineSummary);
                lineData.IsUsing = true;
            }
            return await _lineARepository.InsertableAsync(lineData) > 0;
        }

        protected override void OtherInit()
        {
            _plcCallPCTrayNoPoint = _lineReadPlcInfo.First(x => x.PointName == "托盘号A");
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
