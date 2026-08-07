using System;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Request
{
    /// <summary>
    /// 能量记录（曲线点）查询条件
    /// </summary>
    public class EnergyRangePointQueryRequestDto
    {
        public DateTime EnergyRangeRecordTime { get; set; }

        /// <summary>该子项的点数（由 Tb_Factory6Workshop6_3Line_EnergyRange.Count 提供），查询时 Take(count) 限定，避免全表扫描</summary>
        public int Count { get; set; }
    }
}
