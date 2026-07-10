using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_3;
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
    public class Factory6Workshop6_1AssemblyLineA : Factory6Workshop6_3AssemblyLineAbstract
    {
        public Factory6Workshop6_1AssemblyLineA(Tb_EquipmentRepository equipmentRepositor, McpCommunication mcp,  Tb_ModelNoToNameRepository tb_ModelNoToNameRepository) : base(equipmentRepositor, mcp,  tb_ModelNoToNameRepository)
        {
        }

        protected override string _lineName => throw new NotImplementedException();

        protected override Type _dataModelType => throw new NotImplementedException();

        protected override string _trayPointName => throw new NotImplementedException();

        protected override string _serviceName => "六分厂6-1装配A线";

        protected override Task<object> InsterValue(Result<object> interact)
        {
            return null;
        }

        protected override void UpLastNo(object data)
        {
            return ;
        }
    }
}
