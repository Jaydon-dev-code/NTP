using NPOI.POIFS.Crypt.Dsig;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_3;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_1AssemblyLine;
using SL.MLineDataPrecisionTracking.Models.Enum;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_1
{
    public class Factory6Workshop6_1AssemblyLineA : Factory6Workshop6_3AssemblyLineAbstract
    {
        Tb_Factory6Workshop6_1AssemblyLineARepository _factory6Workshop6_1AssemblyLineARepository;
        Tb_Factory6Workshop6_1AssemblyLineABSummaryRepository _factory6Workshop6_1AssemblyLineABSummaryRepository;
        public Factory6Workshop6_1AssemblyLineA(Tb_EquipmentRepository equipmentRepositor, McpCommunication mcp,  Tb_ModelNoToNameRepository tb_ModelNoToNameRepository, Tb_Factory6Workshop6_1AssemblyLineARepository tb_Factory6Workshop6_1AssemblyLineA, Tb_Factory6Workshop6_1AssemblyLineABSummaryRepository factory6Workshop6_1AssemblyLineABSummaryRepository) : base(equipmentRepositor, mcp,  tb_ModelNoToNameRepository)
        {
            _factory6Workshop6_1AssemblyLineARepository = tb_Factory6Workshop6_1AssemblyLineA;
            _factory6Workshop6_1AssemblyLineABSummaryRepository = factory6Workshop6_1AssemblyLineABSummaryRepository;
            _sleepTimeSpan = TimeSpan.FromMilliseconds(200);
        }
        protected virtual Type _passCodeEnumType => typeof(Assembly_6Factory6_1ALine_PassCodeEnum);
        protected virtual Type _ngCodeEnumType => typeof(Assembly_6Factory6_1ALine_NgCodeEnum);
        protected override string _lineName => "六分厂6-1装配A线";

        protected override Type _dataModelType => typeof(Tb_Factory6Workshop6_1AssemblyLineA);
        protected override string _trayPointName => "托盘号A";

        protected override string _serviceName => "六分厂6-1装配A线";

        protected override async Task<object> InsterValue(Result<object> interact)
        {
        
              var lineData = (Tb_Factory6Workshop6_1AssemblyLineA)interact.Data;

            if (lineData.ShieldStationA != "0")
            {
                if (int.TryParse(lineData.ShieldStationA, out var shieldStationA))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"({lineData.ShieldStationA}) ");
                    foreach (var item in ParseBitEnum(shieldStationA, _passCodeEnumType))
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
                        $"({ngCodeA}) {GetEnumDescription(ngCodeA, _ngCodeEnumType)}";
                }

                Tb_Factory6Workshop6_1AssemblyLineABSummary tb_LineSummary = new Tb_Factory6Workshop6_1AssemblyLineABSummary() { Result = ResultEnum.NG };
                ABToSummary(lineData, null, tb_LineSummary);
                tb_LineSummary.ModelNo = lineData.ModelNoA;
                var modelName = _models
                    .FirstOrDefault(x => x.ModelNo == tb_LineSummary.ModelNo)
                    ?.ModelName;
                tb_LineSummary.ModelName = modelName ?? "";
                await  _factory6Workshop6_1AssemblyLineABSummaryRepository.InsertableAsync(tb_LineSummary);
            }

            await _factory6Workshop6_1AssemblyLineARepository.InsertableAsync(lineData);
            return lineData;
        }

        protected override void UpLastNo(object data)
        {
            _lastTrayNoPoint = ((Tb_Factory6Workshop6_1AssemblyLineA)data).TrayNoA;
        }
    }
}
