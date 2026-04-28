using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    public class Rcl_ProLineDataCollectionService : ProLineDataCollectionServiceAbstract
    {
        Tb_HeatTreatmentDataRepository _rclRepository;

        protected override string _lineName { get; set; } = "热处理";
        protected override Type DataModelType => typeof(Tb_HeatTreatmentData);

        public Rcl_ProLineDataCollectionService(
            Tb_EquipmentRepository equipmentRepositor,
            Tb_HeatTreatmentDataRepository rclRepository,
            McpCommunication mcp
        )
            : base(equipmentRepositor, mcp)
        {
            _rclRepository = rclRepository;
        }

        protected override async Task<bool> InsterCollectionData(object data)
        {
            var heatData = data as Tb_HeatTreatmentData;
            return await _rclRepository.InsertableAsync(heatData) > 0;
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