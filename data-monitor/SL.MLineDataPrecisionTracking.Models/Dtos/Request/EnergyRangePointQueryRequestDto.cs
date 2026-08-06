using System;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Request
{
    /// <summary>
    /// 能量记录（曲线点）查询条件
    /// </summary>
    public class EnergyRangePointQueryRequestDto
    {
        public DateTime EnergyRangeRecordTime { get; set; }
        public int PageIndex { get; set; }
        public int DataCountPerPage { get; set; }
    }
}
