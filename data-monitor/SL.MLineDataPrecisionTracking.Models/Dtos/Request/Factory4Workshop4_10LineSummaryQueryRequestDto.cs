using System;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Request
{
    public class Factory4Workshop4_10LineSummaryQueryRequestDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string SN { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
