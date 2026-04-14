using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    public class PaginationPage
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 最大页数
        /// </summary>
        public int MaxPageCount { get; set; }
        /// <summary>
        ///   每页的数据量
        /// </summary>
        public int DataCountPerPage { get; set; }
    }
}
