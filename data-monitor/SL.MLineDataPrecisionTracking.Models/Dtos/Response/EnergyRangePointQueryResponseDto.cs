using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_3AssemblyLine;
using System.Collections.Generic;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Response
{
    /// <summary>
    /// 能量记录（曲线点）分页返回
    /// </summary>
    public class EnergyRangePointQueryResponseDto
    {
        public EnergyRangePointQueryResponseDto() { }

        public EnergyRangePointQueryResponseDto(
            List<Tb_Factory6Workshop6_3Line_EnergyRangePoint> list,
            int totalCount,
            int totalPage
        )
        {
            List = list;
            TotalCount = totalCount;
            TotalPage = totalPage;
        }

        public List<Tb_Factory6Workshop6_3Line_EnergyRangePoint> List { get; set; }
        public int TotalCount { get; set; }
        public int TotalPage { get; set; }
    }
}
