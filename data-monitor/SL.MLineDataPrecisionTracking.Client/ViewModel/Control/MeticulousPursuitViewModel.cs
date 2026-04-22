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
        private AsyncRelayCommand<System.Windows.Controls.TextBox> _markingNoQueryCommand;
        public AsyncRelayCommand<System.Windows.Controls.TextBox> MarkingNoQueryCommand
        {
            get
            {
                if (_markingNoQueryCommand == null)
                {
                    _markingNoQueryCommand = new AsyncRelayCommand<System.Windows.Controls.TextBox>(
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
        List<LineSummaryDto> _quValues;
        List<LineSummaryDto> _mkNoQuValues;

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
                Result = Models.Enum.ResultEnum.ALL,
            };
            if (_historyScanMarkingNos == null)
            {
                HistoryScanMarkingNos = new ObservableCollection<ScanRecord>();
            }
            _lineSummaryRepository = tb_LineSummaryRepository;
            QueryValue = new ObservableCollection<LineSummaryDto>();
            PageSoure = new ObservableCollection<int>() { 100, 200, 500 };
            PaginationPages.DataCountPerPage = 100;
            _quValues = new List<LineSummaryDto>();
            _mkNoQuValues = new List<LineSummaryDto>();
        }

        private void RemoveHistoryScan(ScanRecord record)
        {
            if (record != null)
            {
                HistoryScanMarkingNos.Remove(record);

                QueryValue.Remove(QueryValue.FirstOrDefault(x => x.MarkingNo == record.MarkingNo));
            }
        }

        private void OpenScanCodeModel(System.Windows.Controls.TextBox textBox)
        {
            if (QueryConditions?.IsScanCode == true)
            {
                _quValues = QueryValue.ToList();
                QueryValue.Clear();
                foreach (var item in _mkNoQuValues)
                {
                    QueryValue.Add(item);
                }
                Keyboard.Focus(textBox);
            }
            else
            { //无扫码模式
                _mkNoQuValues = QueryValue.ToList();
                QueryValue.Clear();
                foreach (var item in _quValues)
                {
                    QueryValue.Add(item);
                }
            }
        }

        private void ClearHistoryMarkingNo()
        {
            HistoryScanMarkingNos.Clear();
            _mkNoQuValues.Clear();
        }

        private async Task MarkingNoQuery(System.Windows.Controls.TextBox textBox)
        {
            if (string.IsNullOrEmpty(QueryConditions?.MarkingNo))
            {
                return;
            }
            LineSummaryDto findValue = new LineSummaryDto(
                await _lineSummaryRepository.QueryableFirstAsync(x =>
                    x.MarkingNo == QueryConditions.MarkingNo
                )
            );
            if (findValue?.MarkingNo == null)
            {
                findValue = new LineSummaryDto(
                    new Tb_LineSummary()
                    {
                        MarkingNo = QueryConditions.MarkingNo,
                        Result = Models.Enum.ResultEnum.NG,
                    }
                )
                {
                    IsHave = false,
                };
            }

            var historyScanMarkingNo = HistoryScanMarkingNos.FirstOrDefault(x =>
                x.MarkingNo == QueryConditions.MarkingNo
            );

            if (historyScanMarkingNo != null)
            {
                HistoryScanMarkingNos.Remove(historyScanMarkingNo);
                HistoryScanMarkingNos.Insert(0, historyScanMarkingNo);
                var tmp = QueryValue.FirstOrDefault(x => x.MarkingNo == QueryConditions.MarkingNo);
                if (tmp != null)
                {
                    QueryValue.Remove(tmp);
                }
            }
            else
            {
                HistoryScanMarkingNos.Insert(
                    0,
                    new ScanRecord()
                    {
                        MarkingNo = QueryConditions.MarkingNo,
                        IsHave = findValue.IsHave,
                    }
                );
            }

            QueryValue.Insert(0, findValue);

            for (int i = 0; i <= HistoryScanMarkingNos.Count - 101; i++)
            {
                HistoryScanMarkingNos.RemoveAt(HistoryScanMarkingNos.Count - 1);
                QueryValue.RemoveAt(QueryValue.Count - 1);
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
            List<LineSummaryDto> re = new List<LineSummaryDto>();
            // 扫码模式单独判断
            if (QueryConditions?.IsScanCode == true)
            {
                if (HistoryScanMarkingNos.Count == 0)
                {
                    HandyControl.Controls.MessageBox.Warning("暂无可导出的扫码数据！");
                    return;
                }

                re = QueryValue.ToList();
            }
            else
            {
                // 👇 普通条件：一行调用
                if (!BuildCommonQueryCondition(ref exp))
                    return;
                re = (await _lineSummaryRepository.QueryableAsync(exp.ToExpression()))
                    .Select(x => new LineSummaryDto(x))
                    .ToList();
            }

            if (re?.Count == 0)
            {
                HandyControl.Controls.MessageBox.Warning(
                    "未检测到导出的数据信息，请检测搜索条件后再次导出！"
                );
                return;
            }
            var ex = Expand.ExportToExcel<LineSummaryDto>(re, saveFileDialog.FileName);
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
                if (string.IsNullOrEmpty(QueryConditions.TrayNoA) is false)
                {
                    exp.And(x => x.TrayNoA == QueryConditions.TrayNoA);
                }
                if (string.IsNullOrEmpty(QueryConditions.TrayNoB) is false)
                {
                    exp.And(x => x.TrayNoB == QueryConditions.TrayNoB);
                }
                if (string.IsNullOrEmpty(QueryConditions.NgCodeA) is false)
                {
                    exp.And(x => x.NgCodeA == QueryConditions.NgCodeA);
                }
                if (string.IsNullOrEmpty(QueryConditions.NgCodeB) is false)
                {
                    exp.And(x => x.NgCodeB == QueryConditions.NgCodeB);
                }
                if (string.IsNullOrEmpty(QueryConditions.MarkingNo) is false)
                {
                    exp.And(x => x.MarkingNo == QueryConditions.MarkingNo);
                }
                if (string.IsNullOrEmpty(QueryConditions.ModelName) is false)
                {
                    exp.And(x => x.ModelName == QueryConditions.ModelName);
                }
                if (QueryConditions.Result != Models.Enum.ResultEnum.ALL)
                {
                    exp.And(x => x.Result == QueryConditions.Result);
                }
            }

            return true;
        }

        async Task PageUpdated()
        {
            await QuData(PaginationPages.PageIndex);
        }
    }
}
