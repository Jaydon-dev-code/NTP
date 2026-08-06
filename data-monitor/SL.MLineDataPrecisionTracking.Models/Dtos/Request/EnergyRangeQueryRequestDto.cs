using System;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Request
{
    /// <summary>
    /// 能量范围（子项）查询条件
    /// </summary>
    public class EnergyRangeQueryRequestDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int PageIndex { get; set; }
        public int DataCountPerPage { get; set; }
    }
}
