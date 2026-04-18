using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    public class PaginationPage : ObservableObject
    {

        private int _pageIndex;
        public int PageIndex
        {
            get => _pageIndex;
            set => SetProperty(ref _pageIndex, value);
        }
        private int _maxPageCount;
        /// <summary>
        /// 最大页数
        /// </summary>
        public int MaxPageCount
        {
            get => _maxPageCount;
            set => SetProperty(ref _maxPageCount, value);
        }

        private int _totalCount;
        /// <summary>
        ///   每页的数据量
        /// </summary>
        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }
        

        private int _dataCountPerPage;
        /// <summary>
        ///   每页的数据量
        /// </summary>
        public int DataCountPerPage
        {
            get => _dataCountPerPage;
            set => SetProperty(ref _dataCountPerPage, value);
        }
    }
}
