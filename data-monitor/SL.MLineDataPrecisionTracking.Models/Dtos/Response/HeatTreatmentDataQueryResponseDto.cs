using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Response
{
    /// <summary>
    /// 热处理数据返回的数据
    /// </summary>
    public class HeatTreatmentDataQueryResponseDto
    {
        /// <summary>
        /// 带所有字段的入参构造函数
        /// </summary>
        /// <param name="list">数据列表</param>
        /// <param name="totalCount">总条数</param>
        /// <param name="totalPage">总页数</param>
        public HeatTreatmentDataQueryResponseDto(List<Tb_HeatTreatmentData> list, int totalCount, int totalPage)
        {
            List = list;
            TotalCount = totalCount;
            TotalPage = totalPage;
        }
        
        /// <summary>
        /// 数据列表
        /// </summary>
        public List<Tb_HeatTreatmentData> List { get; set; }
        
        /// <summary>
        /// 总条数
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPage { get; set; }
    }
}