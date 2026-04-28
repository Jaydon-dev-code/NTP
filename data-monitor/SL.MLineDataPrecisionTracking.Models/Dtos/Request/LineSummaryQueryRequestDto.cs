using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Request
{
    /// <summary>
    /// 装配线的查询条件
    /// </summary>
    public class LineSummaryQueryRequestDto
    {
        // 查询条件
        public RefinedSearchCriteria RefinedSearch { get; set; }

        // 页码
        public int PageNumber { get; set; }

        // 每页条数
        public int PageSize { get; set; }

    }
}
