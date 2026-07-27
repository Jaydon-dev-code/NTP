using SL.MLineDataPrecisionTracking.Models.Enum;
using System;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line
{
    public class Factory4Workshop4_10Line_Clearance_Station2Dto
    {
        public string? SN { get; set; }
        public ResultEnum? Clearance2Result { get; set; }
        public string? Offset { get; set; }
        public string? BeforePressIn { get; set; }
        public string? AfterPressIn { get; set; }
        public string? Gap { get; set; }
        public DateTime? Clearance2Time { get; set; }
    }
}
