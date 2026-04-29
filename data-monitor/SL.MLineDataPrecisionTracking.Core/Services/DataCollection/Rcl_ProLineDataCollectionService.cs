using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    public class Rcl_ProLineDataCollectionService : ProLineDataCollectionServiceAbstract
    {
        Tb_HeatTreatmentDataRepository _rclRepository;
        private DevPlcPointMcDto _plcCallPCMarkingNoPoint;
        private string _lastMarkingNoPoint;

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

        protected override void OtherInit()
        {
            _plcCallPCMarkingNoPoint = _lineReadPlcInfo.First(x => x.PointName == "序列码");
        }

        protected override bool OtherCanCollection()
        {
            var re = _mcp.Read(_plcCallPCMarkingNoPoint);
            if (
                re.IsSuccess is false
                ||  String.IsNullOrEmpty(re.Data.Value[0].ToString())
                || re.Data.Value[0].ToString() == _lastMarkingNoPoint
            )
            {
                return false;
            }
            else
            {
                _lastMarkingNoPoint = re.Data.Value[0].ToString();
                return true;
            }
        }
    }
}
