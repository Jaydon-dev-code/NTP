using SL.MLineDataPrecisionTracking.Models.Enum;
using System;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line
{
    public class Factory4Workshop4_10Line_ABS_CheckDto
    {
        public string? SN { get; set; }
        public ResultEnum? ABSCheckResult { get; set; }
        public DateTime? ABSCheckTime { get; set; }
    }
}
