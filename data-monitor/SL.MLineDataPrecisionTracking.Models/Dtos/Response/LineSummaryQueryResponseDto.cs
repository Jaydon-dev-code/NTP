using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Response
{
    /// <summary>
    /// 装配线返回的数据
    /// </summary>
    public class LineSummaryQueryResponseDto
    {
        // 你要的：带所有字段的入参构造函数
        public LineSummaryQueryResponseDto(List<Tb_LineSummary> list, int totalCount, int totalPage)
        {
            List = list;
            TotalCount = totalCount;
            TotalPage = totalPage;
        }
        public List<Tb_LineSummary> List { get; set; }
        public int TotalCount { get; set; }
        public int TotalPage { get; set; }
    }
}
