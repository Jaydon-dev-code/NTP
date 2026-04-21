using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using MathNet.Numerics.Distributions;
using Microsoft.Win32;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    public partial class MeticulousPursuitViewModel : ObservableObject
    {
        private static ObservableCollection<LineSummaryDto> _queryValue;
        public ObservableCollection<LineSummaryDto> QueryValue
        {
            get => _queryValue;
            set => SetProperty(ref _queryValue, value);
        }

        private PaginationPage _paginationPages;
        public PaginationPage PaginationPages
        {
            get => _paginationPages;
            set => SetProperty(ref _paginationPages, value);
        }

        private RefinedSearchCriteria _queryConditions;
        public RefinedSearchCriteria QueryConditions
        {
            get => _queryConditions;
            set => SetProperty(ref _queryConditions, value);
        }
        private static ObservableCollection<ScanRecord> _historyScanMarkingNos;
        public ObservableCollection<ScanRecord> HistoryScanMarkingNos
        {
            get => _historyScanMarkingNos;
            set => SetProperty(ref _historyScanMarkingNos, value);
        }
        private static ObservableCollection<int> _pageSoure;
        public ObservableCollection<int> PageSoure
        {
            get => _pageSoure;
            set => SetProperty(ref _pageSoure, value);
        }

        private AsyncRelayCommand _queryCommand;
        public AsyncRelayCommand QueryCommand
        {
            get
            {
                if (_queryCommand == null)
                {
                    _queryCommand = new AsyncRelayCommand(Query);
                }
                return _queryCommand;
            }
        }

        private AsyncRelayCommand _saveQueryCommand;
        public AsyncRelayCommand SaveQueryValueCommand
        {
            get
            {
                if (_saveQueryCommand == null)
                {
                    _saveQueryCommand = new AsyncRelayCommand(SaveQueryAsync);
                }
                return _saveQueryCommand;
            }
        }
        private IRelayCommand<System.Windows.Controls.TextBox> _markingNoQueryCommand;
        public IRelayCommand<System.Windows.Controls.TextBox> MarkingNoQueryCommand
        {
            get
            {
                if (_markingNoQueryCommand == null)
                {
                    _markingNoQueryCommand = new RelayCommand<System.Windows.Controls.TextBox>(
                        MarkingNoQuery,
                        (x) => QueryConditions?.IsScanCode == true
                    );
                }
                return _markingNoQueryCommand;
            }
        }

        private AsyncRelayCommand _pageUpdatedCommand;
        public IAsyncRelayCommand PageUpdatedCommand
        {
            get
            {
                if (_pageUpdatedCommand == null)
                {
                    _pageUpdatedCommand = new AsyncRelayCommand(PageUpdated);
                }
                return _pageUpdatedCommand;
            }
        }
        private IRelayCommand<System.Windows.Controls.TextBox> _openScanCodeModelCommand;
        public IRelayCommand<System.Windows.Controls.TextBox> OpenScanCodeModelCommand
        {
            get
            {
                if (_openScanCodeModelCommand == null)
                {
                    _openScanCodeModelCommand = new RelayCommand<System.Windows.Controls.TextBox>(
                        OpenScanCodeModel
                    );
                }
                return _openScanCodeModelCommand;
            }
        }
        private RelayCommand _clearHistoryMarkingNoCommand;
        public IRelayCommand ClearHistoryMarkingNoCommand
        {
            get
            {
                if (_clearHistoryMarkingNoCommand == null)
                {
                    _clearHistoryMarkingNoCommand = new RelayCommand(ClearHistoryMarkingNo);
                }
                return _clearHistoryMarkingNoCommand;
            }
        }
        private RelayCommand<ScanRecord> _removeHistoryScanCommand;
        public IRelayCommand<ScanRecord> RemoveHistoryScanCommand
        {
            get
            {
                if (_removeHistoryScanCommand == null)
                {
                    _removeHistoryScanCommand = new RelayCommand<ScanRecord>(RemoveHistoryScan);
                }
                return _removeHistoryScanCommand;
            }
        }

        Tb_LineSummaryRepository _lineSummaryRepository;

        public MeticulousPursuitViewModel(Tb_LineSummaryRepository tb_LineSummaryRepository)
        {
            PaginationPages = new PaginationPage();
            var lastDay = DateTime.Now.Date;
            QueryConditions = new RefinedSearchCriteria()
            {
                StartDate = lastDay.Date,
                EndDate = lastDay.Date,
                StartTime = lastDay,
                EndTime = lastDay.AddTicks(-1),
                Op = "ALL",
                Result = "ALL",
            };
            if (_historyScanMarkingNos == null)
            {
                HistoryScanMarkingNos = new ObservableCollection<ScanRecord>();
            }
            _lineSummaryRepository = tb_LineSummaryRepository;

            QueryValue = new ObservableCollection<LineSummaryDto>();
            PageSoure = new ObservableCollection<int>() { 100, 200, 500 };
            PaginationPages.DataCountPerPage = 100;
        }

        private void RemoveHistoryScan(ScanRecord record)
        {
            if (record != null)
            {
                HistoryScanMarkingNos.Remove(record);
            }
        }

        private void OpenScanCodeModel(System.Windows.Controls.TextBox textBox)
        {
            Keyboard.Focus(textBox);
        }

        private void ClearHistoryMarkingNo()
        {
            HistoryScanMarkingNos.Clear();
        }

        private void MarkingNoQuery(System.Windows.Controls.TextBox textBox)
        {
            if (QueryConditions?.MarkingNo == null || QueryConditions.MarkingNo == string.Empty)
            {
                return;
            }
            var historyScanMarkingNo = HistoryScanMarkingNos.FirstOrDefault(x =>
                x.MarkingNo == QueryConditions.MarkingNo
            );
            if (historyScanMarkingNo != null)
            {
                HistoryScanMarkingNos.Remove(historyScanMarkingNo);
                HistoryScanMarkingNos.Insert(0, historyScanMarkingNo);
            }
            else
            {
                HistoryScanMarkingNos.Insert(
                    0,
                    new ScanRecord() { MarkingNo = QueryConditions.MarkingNo }
                );
            }
            textBox.Text = string.Empty;
        }

        async Task Query()
        {
            //默认查询第一页
            bool flowControl = await QuData(1);
            if (!flowControl)
            {
                return;
            }
        }

        //async Task<bool> QuData(int pageIndex)
        //{
        //    //扫描模式关注 条码
        //    List<LineSummaryDto> queryValue = new List<LineSummaryDto>();
        //    var exp = Expressionable.Create<Tb_LineSummary>();
        //    if (QueryConditions?.IsScanCode == true)
        //    {
        //        if (QueryConditions.MarkingNo.Length > 0)
        //        {
        //            exp.And(x => x.MarkingNo == QueryConditions.MarkingNo);
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        if (
        //            QueryConditions.StartDate.Date + QueryConditions.StartTime.TimeOfDay
        //            > QueryConditions.EndDate.Date + QueryConditions.EndTime.TimeOfDay
        //        )
        //        {
        //            HandyControl.Controls.MessageBox.Warning("起始时间不能大于结束时间！");
        //            return false;
        //        }
        //        exp.And(x =>
        //            x.RecordTime
        //                >= QueryConditions.StartDate.Date + QueryConditions.StartTime.TimeOfDay
        //            && x.RecordTime
        //                <= QueryConditions.EndDate.Date + QueryConditions.EndTime.TimeOfDay
        //        );
        //        BuildIntCondition(QueryConditions.TrayNoA, ref exp, x => x.TrayNoA, "托盘号A");
        //        BuildIntCondition(QueryConditions.TrayNoA, ref exp, x => x.TrayNoB, "托盘号B");
        //        BuildIntCondition(QueryConditions.TrayNoA, ref exp, x => x.NgCodeA, "NG编码A");
        //        BuildIntCondition(QueryConditions.TrayNoA, ref exp, x => x.NgCodeB, "VG编码B");
        //    }
        //    PaginationPages.PageIndex = pageIndex;
        //    var re = await _lineSummaryRepository.QueryableAsync(
        //        exp.ToExpression(),
        //        x => x.RecordTime,
        //        PaginationPages.PageIndex,
        //        pageSize: PaginationPages.DataCountPerPage
        //    );
        //    PaginationPages.MaxPageCount = re.TotalPage;
        //    PaginationPages.TotalCount = re.TotalCount;
        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        QueryValue.Clear();
        //        if (re.List.Count > 0)
        //        {
        //            foreach (var item in re.List)
        //            {
        //                QueryValue.Add(new LineSummaryDto(item));
        //            }
        //        }
        //    });
        //    return true;
        //}

        //async Task SaveQueryAsync()
        //{
        //    SaveFileDialog saveFileDialog = new SaveFileDialog
        //    {
        //        Filter = "Excel文件 (*.xlsx)|*.xlsx",
        //        Title = "保存Excel文件",
        //        FileName = $"数据导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
        //    };

        //    var exp = Expressionable.Create<Tb_LineSummary>();
        //    if (QueryConditions?.IsScanCode == true)
        //    {
        //        if (HistoryScanMarkingNos.Count == 0)
        //        {
        //            HandyControl.Controls.MessageBox.Warning("暂无可导出的扫码数据");
        //        }
        //        else
        //        {
        //            exp.And(x => HistoryScanMarkingNos.Any(h => h.MarkingNo == x.MarkingNo));
        //        }
        //    }
        //    else
        //    {
        //        if (
        //            QueryConditions.StartDate.Date + QueryConditions.StartTime.TimeOfDay
        //            > QueryConditions.EndDate.Date + QueryConditions.EndTime.TimeOfDay
        //        )
        //        {
        //            HandyControl.Controls.MessageBox.Warning("起始时间不能大于结束时间！");
        //            return;
        //        }
        //        exp.And(x =>
        //            x.RecordTime
        //                >= QueryConditions.StartDate.Date + QueryConditions.StartTime.TimeOfDay
        //            && x.RecordTime
        //                <= QueryConditions.EndDate.Date + QueryConditions.EndTime.TimeOfDay
        //        );
        //        BuildIntCondition(QueryConditions.TrayNoA, ref exp, x => x.TrayNoA, "托盘号A");
        //        BuildIntCondition(QueryConditions.TrayNoA, ref exp, x => x.TrayNoB, "托盘号B");
        //        BuildIntCondition(QueryConditions.TrayNoA, ref exp, x => x.NgCodeA, "NG编码A");
        //        BuildIntCondition(QueryConditions.TrayNoA, ref exp, x => x.NgCodeB, "VG编码B");
        //    }

        //    var re = await _lineSummaryRepository.QueryableAsync(exp.ToExpression());
        //    Expand.ExportToExcel<LineSummaryDto>(
        //        re.Select(x => new LineSummaryDto(x)).ToList(),
        //        saveFileDialog.FileName
        //    );
        //}

        async Task<bool> QuData(int pageIndex)
        {
            List<LineSummaryDto> queryValue = new List<LineSummaryDto>();
            var exp = Expressionable.Create<Tb_LineSummary>();

            // 👇 一行调用封装好的条件
            if (!BuildCommonQueryCondition(ref exp))
                return false;

            PaginationPages.PageIndex = pageIndex;
            var re = await _lineSummaryRepository.QueryableAsync(
                exp.ToExpression(),
                x => x.RecordTime,
                PaginationPages.PageIndex,
                pageSize: PaginationPages.DataCountPerPage
            );

            PaginationPages.MaxPageCount = re.TotalPage;
            PaginationPages.TotalCount = re.TotalCount;

            Application.Current.Dispatcher.Invoke(() =>
            {
                QueryValue.Clear();
                foreach (var item in re.List)
                {
                    QueryValue.Add(new LineSummaryDto(item));
                }
            });

            return true;
        }

        async Task SaveQueryAsync()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx",
                Title = "保存Excel文件",
                FileName = $"数据导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            var exp = Expressionable.Create<Tb_LineSummary>();

            // 扫码模式单独判断
            if (QueryConditions?.IsScanCode == true)
            {
                if (HistoryScanMarkingNos.Count == 0)
                {
                    HandyControl.Controls.MessageBox.Warning("暂无可导出的扫码数据");
                    return;
                }
                exp.And(x => HistoryScanMarkingNos.Any(h => h.MarkingNo == x.MarkingNo));
            }
            else
            {
                // 👇 普通条件：一行调用
                if (!BuildCommonQueryCondition(ref exp))
                    return;
            }

            var re = await _lineSummaryRepository.QueryableAsync(exp.ToExpression());
            var ex = Expand.ExportToExcel<LineSummaryDto>(
                re.Select(x => new LineSummaryDto(x)).ToList(),
                saveFileDialog.FileName
            );
            if (ex.IsSuccess)
            {
                HandyControl.Controls.MessageBox.Success("导出完成！");
            }
            else
            {
                HandyControl.Controls.MessageBox.Warning($"导出失败！/r/n{ex.Message}");
            }
        }

        /// <summary>
        /// 统一构建查询条件（Query + Save 共用）
        /// </summary>
        private bool BuildCommonQueryCondition(ref Expressionable<Tb_LineSummary> exp)
        {
            // 扫码模式
            if (QueryConditions?.IsScanCode == true)
            {
                if (string.IsNullOrWhiteSpace(QueryConditions.MarkingNo))
                    return false;

                exp.And(x => x.MarkingNo == QueryConditions.MarkingNo);
            }
            // 时间 + 普通条件模式
            else
            {
                var startTime =
                    QueryConditions.StartDate.Date + QueryConditions.StartTime.TimeOfDay;
                var endTime = QueryConditions.EndDate.Date + QueryConditions.EndTime.TimeOfDay;

                if (startTime > endTime)
                {
                    HandyControl.Controls.MessageBox.Warning("起始时间不能大于结束时间！");
                    return false;
                }

                exp.And(x => x.RecordTime >= startTime && x.RecordTime <= endTime);

                // 统一公共条件
                exp.And(x => x.TrayNoA == QueryConditions.TrayNoA);
                exp.And(x => x.TrayNoB == QueryConditions.TrayNoB);
                exp.And(x => x.NgCodeA == QueryConditions.NgCodeA);
                exp.And(x => x.NgCodeB == QueryConditions.NgCodeB);
            }

            return true;
        }

        async Task PageUpdated()
        {
            await QuData(PaginationPages.PageIndex);
        }
    }
}
