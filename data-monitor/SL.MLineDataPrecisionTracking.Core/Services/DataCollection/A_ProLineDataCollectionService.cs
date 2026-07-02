using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.SymbolStore;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.Ocsp;
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
        List<Tb_ModelNoToName> _models;
        protected override string[] _lineName { get; set; } = new string[] { "A线", "六分厂6-1装配A线" };
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
            ServiceName = "A线";
            _lineARepository = tb_LineARepository;
            _ineSummaryRepository = tb_LineSummaryRepository;
            _modelNoToNameRepository = tb_ModelNoToNameRepository;
        }

        protected override async Task<bool> InsterCollectionData(object data)
        {
            var lineData = data as Tb_LineA;

         

            if (lineData.ShieldStationA!="0")
            {
              
                if (int.TryParse(lineData.ShieldStationA, out var shieldStationA))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"({lineData.ShieldStationA}) ");
                    if (ServiceName == "A线")
                    {
                        foreach (var item in shieldStationA.ParseBitEnum<Assembly_A_LinePassCodeEnum>())
                        {
                            stringBuilder.Append(item.Description + " ");
                        }
                    }
                    else
                    {
                        foreach (var item in shieldStationA.ParseBitEnum<Assembly_6Factory6_1ALine_PassCodeEnum>())
                        {
                            stringBuilder.Append(item.Description + " ");
                        }

                    }
                   
                    lineData.ShieldStationA=stringBuilder.ToString();

                }
            }

            if (lineData.NgCodeA != "0")
            {
                if (int.TryParse(lineData.NgCodeA, out var ngCodeA))
                {
                    if (ServiceName == "A线")
                    {
                        lineData.NgCodeA =
                        $"({ngCodeA}) {((Assembly_A_LineNgCodeEnum)ngCodeA).GetDescription()}";
                    }
                    else
                    {
                        lineData.NgCodeA =
               $"({ngCodeA}) {((Assembly_6Factory6_1ALine_NgCodeEnum)ngCodeA).GetDescription()}";
                    }
                      
                }
                Tb_LineSummary tb_LineSummary = new Tb_LineSummary() { Result = ResultEnum.NG };
                ABToSummary(lineData, null, tb_LineSummary);

                tb_LineSummary.ModelNo = lineData.ModelNoA;
                var modelNameB = _models
                    .FirstOrDefault(x => x.ModelNo == tb_LineSummary.ModelNo)
                    ?.ModelName;
                tb_LineSummary.ModelName = modelNameB == null ? "" : modelNameB;
                await _ineSummaryRepository.InsertableAsync(tb_LineSummary);
            }
            return await _lineARepository.InsertableAsync(lineData) > 0;
        }

        protected override async Task OtherInitAsync()
        {
            _plcCallPCTrayNoPoint = _lineReadPlcInfo.First(x => x.PointName == "托盘号A");
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
