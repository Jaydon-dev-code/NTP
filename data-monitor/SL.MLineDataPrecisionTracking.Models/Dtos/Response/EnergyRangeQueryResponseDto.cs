using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_3AssemblyLine;
using System.Collections.Generic;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Response
{
    /// <summary>
    /// 能量范围（子项）分页返回
    /// </summary>
    public class EnergyRangeQueryResponseDto
    {
        public EnergyRangeQueryResponseDto() { }

        public EnergyRangeQueryResponseDto(
            List<Tb_Factory6Workshop6_3Line_EnergyRange> list,
            int totalCount,
            int totalPage
        )
        {
            List = list;
            TotalCount = totalCount;
            TotalPage = totalPage;
        }

        public List<Tb_Factory6Workshop6_3Line_EnergyRange> List { get; set; }
        public int TotalCount { get; set; }
        public int TotalPage { get; set; }
    }
}
