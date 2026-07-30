using System;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line
{
    public class Factory4Workshop4_10Line_RivetAndCrack_RivetingDto
    {
        public string? SN { get; set; }
        public ResultEnum? RivetingResult { get; set; }
        public string? RivetingInspection1Height { get; set; }
        public string? RivetingInspectionFormingHeight { get; set; }
        public string? RivetingInspection2Height { get; set; }
        public DateTime? RivetingTime { get; set; }
    }
}
