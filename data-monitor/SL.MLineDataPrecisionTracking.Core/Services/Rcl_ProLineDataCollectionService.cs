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
    public class Rcl_ProLineDataCollectionService
        : ProLineDataCollectionServiceAbstract<Tb_HeatTreatmentData>
    {
        Tb_HeatTreatmentDataRepository _rclRepository;

        public Rcl_ProLineDataCollectionService(
            Tb_EquipmentRepository equipmentRepositor,
            Tb_HeatTreatmentDataRepository rclRepository,
            McpCommunication mcp
        )
            : base(equipmentRepositor, mcp)
        {
            _rclRepository = rclRepository;
        }

        protected override string _lineName { get; set; } = "热处理";

        protected override async Task<bool> InsterCollectionData(Tb_HeatTreatmentData data)
        {
            return await _rclRepository.InsertableAsync(data) > 0;
        }
    }
}
