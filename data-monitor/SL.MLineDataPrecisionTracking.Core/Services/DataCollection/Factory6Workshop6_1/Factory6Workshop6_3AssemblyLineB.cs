using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_3;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_1
{
    public class Factory6Workshop6_1AssemblyLineB : Factory6Workshop6_3AssemblyLineAbstract
    {
        public Factory6Workshop6_1AssemblyLineB(Tb_EquipmentRepository equipmentRepositor, McpCommunication mcp, Tb_LineARepository tb_LineARepository, Tb_LineSummaryRepository tb_LineSummaryRepository, Tb_ModelNoToNameRepository tb_ModelNoToNameRepository) : base(equipmentRepositor, mcp, tb_LineARepository, tb_LineSummaryRepository, tb_ModelNoToNameRepository)
        {
        }

        protected override string _lineName => throw new NotImplementedException();

        protected override Type _dataModelType => throw new NotImplementedException();

        protected override string _trayPointName => throw new NotImplementedException();

        protected override string _serviceName => throw new NotImplementedException();

        protected override Task<object> InsterValue(Result<object> interact)
        {
            throw new NotImplementedException();
        }

        protected override void UpLastNo(object data)
        {
            throw new NotImplementedException();
        }
    }
}
