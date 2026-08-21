using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Data;
using Microsoft.Win32;
using SL.MLineDataPrecisionTracking.Client.Http;

using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Dtos.Request;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    public partial class Factory6Workshop6_1AssemblyLineViewModel : ObservableObject
    {
        private static ObservableCollection<Factory6Workshop6_1AssemblyLineDto> _queryValue;
        public ObservableCollection<Factory6Workshop6_1AssemblyLineDto> QueryValue
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
        List<Factory6Workshop6_1AssemblyLineDto> _quValues;
        List<Factory6Workshop6_1AssemblyLineDto> _mkNoQuValues;

        Factory6Workshop6_1AssemblyLineApi _api;

        public Factory6Workshop6_1AssemblyLineViewModel(Factory6Workshop6_1AssemblyLineApi api)
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
            _api = api;
            QueryValue = new ObservableCollection<Factory6Workshop6_1AssemblyLineDto>();
            PageSoure = new ObservableCollection<int>() { 100, 200, 500 };
            PaginationPages.DataCountPerPage = 100;
            _quValues = new List<Factory6Workshop6_1AssemblyLineDto>();
            _mkNoQuValues = new List<Factory6Workshop6_1AssemblyLineDto>();
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
            {
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
            QueryValue.Clear();
        }

        private async Task MarkingNoQuery(System.Windows.Controls.TextBox textBox)
        {
            if (string.IsNullOrEmpty(QueryConditions?.MarkingNo))
            {
                return;
            }

            var request = new LineSummaryQueryRequestDto
            {
                RefinedSearch = new RefinedSearchCriteria { MarkingNo = QueryConditions.MarkingNo }
            };
            var apiResult = await _api.MarkingNoQuery(request);

            Factory6Workshop6_1AssemblyLineDto findValue;
            if (apiResult.IsSuccess && apiResult.Data?.List?.Count > 0)
            {
                findValue = new Factory6Workshop6_1AssemblyLineDto(apiResult.Data.List.First());
            }
            else
            {
                findValue = Factory6Workshop6_1AssemblyLineDto.NotFindMakringNo(QueryConditions.MarkingNo);
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
            bool flowControl = await QuData(1);
            if (!flowControl)
            {
                return;
            }
        }

        async Task<bool> QuData(int pageIndex)
        {
            var startTime = QueryConditions.StartDate.Date + QueryConditions.StartTime.TimeOfDay;
            var endTime = QueryConditions.EndDate.Date + QueryConditions.EndTime.TimeOfDay;
            if (startTime > endTime)
            {
                HandyControl.Controls.MessageBox.Warning("起始时间不能大于结束时间！");
                return false;
            }

            PaginationPages.PageIndex = pageIndex;

            var request = new LineSummaryQueryRequestDto
            {
                RefinedSearch = QueryConditions,
                PageNumber = PaginationPages.PageIndex,
                PageSize = PaginationPages.DataCountPerPage
            };

            var apiResult = await _api.QueryablToPagee(request);

            if (!apiResult.IsSuccess)
            {
                HandyControl.Controls.MessageBox.Warning($"查询失败：{apiResult.Message}");
                return false;
            }

            PaginationPages.MaxPageCount = apiResult.Data.TotalPage;
            PaginationPages.TotalCount = apiResult.Data.TotalCount;

            Application.Current.Dispatcher.Invoke(() =>
            {
                QueryValue.Clear();
                foreach (var item in apiResult.Data.List)
                {
                    QueryValue.Add(new Factory6Workshop6_1AssemblyLineDto(item));
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

            // 扫码模式：直接导出当前已加载的数据（数据量小）
            if (QueryConditions?.IsScanCode == true)
            {
                if (HistoryScanMarkingNos.Count == 0)
                {
                    HandyControl.Controls.MessageBox.Warning("暂无可导出的扫码数据！");
                    return;
                }

                var scanData = QueryValue.ToList();
                if (scanData.Count == 0)
                {
                    HandyControl.Controls.MessageBox.Warning(
                        "未检测到导出的数据信息，请检测搜索条件后再次导出！"
                    );
                    return;
                }
                var scanEx = ExcelExpand.ExportToExcel(scanData, saveFileDialog.FileName);
                if (scanEx.IsSuccess)
                {
                    HandyControl.Controls.MessageBox.Success("导出完成！");
                }
                else
                {
                    HandyControl.Controls.MessageBox.Warning($"导出失败！/r/n{scanEx.Message}");
                }
                return;
            }

            // 时间范围查询：分页导出，边查边写，避免大数据量占用内存
            var startTime = QueryConditions.StartDate.Date + QueryConditions.StartTime.TimeOfDay;
            var endTime = QueryConditions.EndDate.Date + QueryConditions.EndTime.TimeOfDay;
            if (startTime > endTime)
            {
                HandyControl.Controls.MessageBox.Warning("起始时间不能大于结束时间！");
                return;
            }

            const int exportPageSize = 5000;
            var request = new LineSummaryQueryRequestDto
            {
                RefinedSearch = QueryConditions,
                PageNumber = 1,
                PageSize = exportPageSize
            };

            var firstPage = await _api.QueryablToPagee(request);
            if (!firstPage.IsSuccess)
            {
                HandyControl.Controls.MessageBox.Warning($"查询失败：{firstPage.Message}");
                return;
            }

            if (firstPage.Data?.List == null || firstPage.Data.List.Count == 0)
            {
                HandyControl.Controls.MessageBox.Warning(
                    "未检测到导出的数据信息，请检测搜索条件后再次导出！"
                );
                return;
            }

            try
            {
                using (
                    ExcelExportWriter<Factory6Workshop6_1AssemblyLineDto> writer =
                        new ExcelExportWriter<Factory6Workshop6_1AssemblyLineDto>(
                            saveFileDialog.FileName
                        )
                )
                {
                    writer.WriteRows(
                        firstPage.Data.List.Select(
                            x => new Factory6Workshop6_1AssemblyLineDto(x)
                        )
                    );

                    for (int page = 2; page <= firstPage.Data.TotalPage; page++)
                    {
                        request.PageNumber = page;
                        var apiResult = await _api.QueryablToPagee(request);
                        if (!apiResult.IsSuccess)
                        {
                            HandyControl.Controls.MessageBox.Warning(
                                $"查询失败：{apiResult.Message}"
                            );
                            return;
                        }
                        writer.WriteRows(
                            apiResult.Data.List.Select(
                                x => new Factory6Workshop6_1AssemblyLineDto(x)
                            )
                        );
                    }
                }

                HandyControl.Controls.MessageBox.Success("导出完成！");
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Warning($"导出失败！/r/n{ex.Message}");
            }
        }

        async Task PageUpdated()
        {
            await QuData(PaginationPages.PageIndex);
        }
    }
}
