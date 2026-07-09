using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Enum;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_1
{
    public class Factory6Workshop6_1AssemblyLineA : Factory6Workshop6_1AssemblyLineAbstract
    {
        protected override string _lineName => "A线";
        protected override Type _dataModelType => typeof(Tb_LineA);
        protected override string _trayPointName => "托盘号A";
        protected override string _serviceName => "六分厂6-1装配A线";

        public Factory6Workshop6_1AssemblyLineA(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp,
            Tb_LineARepository tb_LineARepository,
            Tb_LineSummaryRepository tb_LineSummaryRepository,
            Tb_ModelNoToNameRepository tb_ModelNoToNameRepository
        ) : base(equipmentRepositor, mcp, tb_LineARepository, tb_LineSummaryRepository, tb_ModelNoToNameRepository)
        {
        }

        protected override void UpLastNo(object data)
        {
            _lastTrayNoPoint = ((Tb_LineA)data).TrayNoA;
        }

        protected override async Task<object> InsterValue(Result<object> interact)
        {
            var lineData = (Tb_LineA)interact.Data;

            if (lineData.ShieldStationA != "0")
            {
                if (int.TryParse(lineData.ShieldStationA, out var shieldStationA))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"({lineData.ShieldStationA}) ");
                    foreach (var item in shieldStationA.ParseBitEnum<Assembly_6Factory6_1ALine_PassCodeEnum>())
                    {
                        stringBuilder.Append(item.Description + " ");
                    }
                    lineData.ShieldStationA = stringBuilder.ToString();
                }
            }

            if (lineData.NgCodeA != "0")
            {
                if (int.TryParse(lineData.NgCodeA, out var ngCodeA))
                {
                    lineData.NgCodeA =
                        $"({ngCodeA}) {((Assembly_6Factory6_1ALine_NgCodeEnum)ngCodeA).GetDescription()}";
                }

                Tb_LineSummary tb_LineSummary = new Tb_LineSummary() { Result = ResultEnum.NG };
                ABToSummary(lineData, null, tb_LineSummary);
                tb_LineSummary.ModelNo = lineData.ModelNoA;
                var modelName = _models
                    .FirstOrDefault(x => x.ModelNo == tb_LineSummary.ModelNo)
                    ?.ModelName;
                tb_LineSummary.ModelName = modelName ?? "";
                await _lineSummaryRepository.InsertableAsync(tb_LineSummary);
            }

            await _lineARepository.InsertableAsync(lineData);
            return lineData;
        }
    }
}
