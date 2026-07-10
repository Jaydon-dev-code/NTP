using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_1AssemblyLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Response
{
    public class Factory6Workshop6_1AssemblyLineResponseDto
    { // 你要的：带所有字段的入参构造函数
        public Factory6Workshop6_1AssemblyLineResponseDto(List<Tb_Factory6Workshop6_1AssemblyLineABSummary> list, int totalCount, int totalPage)
        {
            List = list;
            TotalCount = totalCount;
            TotalPage = totalPage;
        }
        public List<Tb_Factory6Workshop6_1AssemblyLineABSummary> List
        {
            get; set;
        }
        public int TotalCount
        {
            get; set;
        }
        public int TotalPage
        {
            get; set;
        }
    }
}
