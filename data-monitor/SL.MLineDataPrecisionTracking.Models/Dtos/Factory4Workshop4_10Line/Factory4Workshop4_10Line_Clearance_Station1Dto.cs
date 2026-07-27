using SL.MLineDataPrecisionTracking.Models.Enum;
using System;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line
{
    public class Factory4Workshop4_10Line_Clearance_Station1Dto
    {
        public string? SN { get; set; }
        public ResultEnum? Clearance1Result { get; set; }
        public string? PositiveGap { get; set; }
        public string? LowerLoad { get; set; }
        public string? UpperLoad { get; set; }
        public DateTime? Clearance1Time { get; set; }
    }
}
