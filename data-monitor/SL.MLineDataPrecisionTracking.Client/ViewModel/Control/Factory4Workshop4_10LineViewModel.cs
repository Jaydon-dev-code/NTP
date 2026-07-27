using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Dtos.Request;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    public partial class Factory4Workshop4_10LineViewModel : ObservableObject
    {
        private bool _isOnlieClearance;
        public bool IsOnlieClearance
        {
            get => _isOnlieClearance;
            set => SetProperty(ref _isOnlieClearance, value);
        }

        private bool _isOnlieRivetAndCrack;
        public bool IsOnlieRivetAndCrack
        {
            get => _isOnlieRivetAndCrack;
            set => SetProperty(ref _isOnlieRivetAndCrack, value);
        }

        private bool _isOnlieVib;
        public bool IsOnlieVib
        {
            get => _isOnlieVib;
            set => SetProperty(ref _isOnlieVib, value);
        }

        private bool _isOnlieVibScan;
        public bool IsOnlieVibScan
        {
            get => _isOnlieVibScan;
            set => SetProperty(ref _isOnlieVibScan, value);
        }

        private bool _isOnlieABS;
        public bool IsOnlieABS
        {
            get => _isOnlieABS;
            set => SetProperty(ref _isOnlieABS, value);
        }

        private Factory4Workshop4_10Line_VibDto _vibData;
        public Factory4Workshop4_10Line_VibDto VibData
        {
            get => _vibData;
            set => SetProperty(ref _vibData, value);
        }

        private Factory4Workshop4_10Line_RivetAndCrack_RivetingDto _rivetingData;
        public Factory4Workshop4_10Line_RivetAndCrack_RivetingDto RivetingData
        {
            get => _rivetingData;
            set => SetProperty(ref _rivetingData, value);
        }

        private Factory4Workshop4_10Line_RivetAndCrack_SpinRivetingDto _spinRivetingData;
        public Factory4Workshop4_10Line_RivetAndCrack_SpinRivetingDto SpinRivetingData
        {
            get => _spinRivetingData;
            set => SetProperty(ref _spinRivetingData, value);
        }

        private Factory4Workshop4_10Line_Clearance_Station1Dto _clearanceStation1Data;
        public Factory4Workshop4_10Line_Clearance_Station1Dto ClearanceStation1Data
        {
            get => _clearanceStation1Data;
            set => SetProperty(ref _clearanceStation1Data, value);
        }

        private Factory4Workshop4_10Line_Clearance_Station2Dto _clearanceStation2Data;
        public Factory4Workshop4_10Line_Clearance_Station2Dto ClearanceStation2Data
        {
            get => _clearanceStation2Data;
            set => SetProperty(ref _clearanceStation2Data, value);
        }

        private Factory4Workshop4_10Line_ABS_PressDownDto _aBSPressDownData;
        public Factory4Workshop4_10Line_ABS_PressDownDto ABSPressDownData
        {
            get => _aBSPressDownData;
            set => SetProperty(ref _aBSPressDownData, value);
        }

        private Factory4Workshop4_10Line_ABS_CheckDto _aBSCheckData;
        public Factory4Workshop4_10Line_ABS_CheckDto ABSCheckData
        {
            get => _aBSCheckData;
            set => SetProperty(ref _aBSCheckData, value);
        }

        private DateTime _queryStartDate = DateTime.Now.Date;
        public DateTime QueryStartDate
        {
            get => _queryStartDate;
            set => SetProperty(ref _queryStartDate, value);
        }

        private DateTime _queryStartTime = DateTime.Now.Date;
        public DateTime QueryStartTime
        {
            get => _queryStartTime;
            set => SetProperty(ref _queryStartTime, value);
        }

        private DateTime _queryEndDate = DateTime.Now.Date;
        public DateTime QueryEndDate
        {
            get => _queryEndDate;
            set => SetProperty(ref _queryEndDate, value);
        }

        private DateTime _queryEndTime = DateTime.Now.Date.AddDays(1).AddTicks(-1);
        public DateTime QueryEndTime
        {
            get => _queryEndTime;
            set => SetProperty(ref _queryEndTime, value);
        }

        private string _querySN;
        public string QuerySN
        {
            get => _querySN;
            set => SetProperty(ref _querySN, value);
        }

        private bool _isScanCode;
        public bool IsScanCode
        {
            get => _isScanCode;
            set => SetProperty(ref _isScanCode, value);
        }

        private PaginationPage _paginationPages = new();
        public PaginationPage PaginationPages
        {
            get => _paginationPages;
            set => SetProperty(ref _paginationPages, value);
        }

        private ObservableCollection<Tb_Factory4Workshop4_10LineSummary> _queryValue = new();
        public ObservableCollection<Tb_Factory4Workshop4_10LineSummary> QueryValue
        {
            get => _queryValue;
            set => SetProperty(ref _queryValue, value);
        }

        private ObservableCollection<int> _pageSoure = new() { 100, 200, 500 };
        public ObservableCollection<int> PageSoure
        {
            get => _pageSoure;
            set => SetProperty(ref _pageSoure, value);
        }

        private ObservableCollection<ScanRecord> _historyScanMarkingNos = new();
        public ObservableCollection<ScanRecord> HistoryScanMarkingNos
        {
            get => _historyScanMarkingNos;
            set => SetProperty(ref _historyScanMarkingNos, value);
        }

        Factory4Workshop4_10LineSummaryApi _summaryApi;
        List<Tb_Factory4Workshop4_10LineSummary> _quValues = new();
        List<Tb_Factory4Workshop4_10LineSummary> _mkNoQuValues = new();

        private AsyncRelayCommand _queryCommand;
        public AsyncRelayCommand QueryCommand
        {
            get
            {
                if (_queryCommand == null)
                    _queryCommand = new AsyncRelayCommand(Query);
                return _queryCommand;
            }
        }

        private AsyncRelayCommand _pageUpdatedCommand;
        public AsyncRelayCommand PageUpdatedCommand
        {
            get
            {
                if (_pageUpdatedCommand == null)
                    _pageUpdatedCommand = new AsyncRelayCommand(PageUpdated);
                return _pageUpdatedCommand;
            }
        }

        private AsyncRelayCommand _saveQueryCommand;
        public AsyncRelayCommand SaveQueryCommand
        {
            get
            {
                if (_saveQueryCommand == null)
                    _saveQueryCommand = new AsyncRelayCommand(SaveQueryAsync);
                return _saveQueryCommand;
            }
        }

        private IRelayCommand<System.Windows.Controls.TextBox> _openScanCodeModelCommand;
        public IRelayCommand<System.Windows.Controls.TextBox> OpenScanCodeModelCommand
        {
            get
            {
                if (_openScanCodeModelCommand == null)
                    _openScanCodeModelCommand = new RelayCommand<System.Windows.Controls.TextBox>(OpenScanCodeModel);
                return _openScanCodeModelCommand;
            }
        }

        private AsyncRelayCommand<System.Windows.Controls.TextBox> _markingNoQueryCommand;
        public AsyncRelayCommand<System.Windows.Controls.TextBox> MarkingNoQueryCommand
        {
            get
            {
                if (_markingNoQueryCommand == null)
                    _markingNoQueryCommand = new AsyncRelayCommand<System.Windows.Controls.TextBox>(MarkingNoQuery, x => IsScanCode == true);
                return _markingNoQueryCommand;
            }
        }

        private RelayCommand _clearHistoryMarkingNoCommand;
        public RelayCommand ClearHistoryMarkingNoCommand
        {
            get
            {
                if (_clearHistoryMarkingNoCommand == null)
                    _clearHistoryMarkingNoCommand = new RelayCommand(ClearHistoryMarkingNo);
                return _clearHistoryMarkingNoCommand;
            }
        }

        private RelayCommand<ScanRecord> _removeHistoryScanCommand;
        public RelayCommand<ScanRecord> RemoveHistoryScanCommand
        {
            get
            {
                if (_removeHistoryScanCommand == null)
                    _removeHistoryScanCommand = new RelayCommand<ScanRecord>(RemoveHistoryScan);
                return _removeHistoryScanCommand;
            }
        }

        public Factory4Workshop4_10LineViewModel(HubClien hubClien, Factory4Workshop4_10LineSummaryApi summaryApi)
        {
            _summaryApi = summaryApi;
            PaginationPages.DataCountPerPage = 100;
            UpValue(hubClien);
        }

        void UpValue(HubClien hubClien)
        {
            hubClien.Start<bool>(nameof(IsOnlieClearance), x => IsOnlieClearance = x);
            hubClien.Start<bool>(nameof(IsOnlieRivetAndCrack), x => IsOnlieRivetAndCrack = x);
            hubClien.Start<bool>(nameof(IsOnlieVib), x => IsOnlieVib = x);
            hubClien.Start<bool>(nameof(IsOnlieVibScan), x => IsOnlieVibScan = x);
            hubClien.Start<bool>(nameof(IsOnlieABS), x => IsOnlieABS = x);
            hubClien.Start<Factory4Workshop4_10Line_VibDto>(nameof(VibData), x => VibData = x);
            hubClien.Start<Factory4Workshop4_10Line_RivetAndCrack_RivetingDto>(nameof(RivetingData), x => RivetingData = x);
            hubClien.Start<Factory4Workshop4_10Line_RivetAndCrack_SpinRivetingDto>(nameof(SpinRivetingData), x => SpinRivetingData = x);
            hubClien.Start<Factory4Workshop4_10Line_Clearance_Station1Dto>(nameof(ClearanceStation1Data), x => ClearanceStation1Data = x);
            hubClien.Start<Factory4Workshop4_10Line_Clearance_Station2Dto>(nameof(ClearanceStation2Data), x => ClearanceStation2Data = x);
            hubClien.Start<Factory4Workshop4_10Line_ABS_PressDownDto>(nameof(ABSPressDownData), x => ABSPressDownData = x);
            hubClien.Start<Factory4Workshop4_10Line_ABS_CheckDto>(nameof(ABSCheckData), x => ABSCheckData = x);
        }

        async Task Query()
        {
            await QuData(1);
        }

        async Task QuData(int pageIndex)
        {
            var startTime = QueryStartDate.Date + QueryStartTime.TimeOfDay;
            var endTime = QueryEndDate.Date + QueryEndTime.TimeOfDay;
            if (startTime > endTime)
            {
                HandyControl.Controls.MessageBox.Warning("起始时间不能大于结束时间！");
                return;
            }

            PaginationPages.PageIndex = pageIndex;

            var request = new Factory4Workshop4_10LineSummaryQueryRequestDto
            {
                StartTime = startTime,
                EndTime = endTime,
                SN = QuerySN,
                PageNumber = PaginationPages.PageIndex,
                PageSize = PaginationPages.DataCountPerPage
            };

            var apiResult = await _summaryApi.QueryablToPagee(request);

            if (!apiResult.IsSuccess)
            {
                HandyControl.Controls.MessageBox.Warning($"查询失败：{apiResult.Message}");
                return;
            }

            PaginationPages.MaxPageCount = apiResult.Data.TotalPage;
            PaginationPages.TotalCount = apiResult.Data.TotalCount;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                QueryValue.Clear();
                foreach (var item in apiResult.Data.List)
                {
                    QueryValue.Add(item);
                }
            });
        }

        async Task PageUpdated()
        {
            await QuData(PaginationPages.PageIndex);
        }

        async Task SaveQueryAsync()
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx",
                Title = "保存Excel文件",
                FileName = $"4-10线数据导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            List<Tb_Factory4Workshop4_10LineSummary> re;
            if (IsScanCode)
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
                var startTime = QueryStartDate.Date + QueryStartTime.TimeOfDay;
                var endTime = QueryEndDate.Date + QueryEndTime.TimeOfDay;
                var request = new Factory4Workshop4_10LineSummaryQueryRequestDto
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    SN = QuerySN,
                };
                var apiResult = await _summaryApi.SaveQuery(request);

                if (!apiResult.IsSuccess)
                {
                    HandyControl.Controls.MessageBox.Warning($"导出查询失败：{apiResult.Message}");
                    return;
                }
                re = apiResult.Data.List;
            }

            if (re?.Count == 0)
            {
                HandyControl.Controls.MessageBox.Warning("未检测到导出的数据信息，请检测搜索条件后再次导出！");
                return;
            }
            var ex = Expand.ExportToExcel(re, saveFileDialog.FileName);
            if (ex.IsSuccess)
            {
                HandyControl.Controls.MessageBox.Success("导出完成！");
            }
            else
            {
                HandyControl.Controls.MessageBox.Warning($"导出失败！\r\n{ex.Message}");
            }
        }

        void OpenScanCodeModel(System.Windows.Controls.TextBox textBox)
        {
            if (IsScanCode)
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

        async Task MarkingNoQuery(System.Windows.Controls.TextBox textBox)
        {
            if (string.IsNullOrEmpty(QuerySN))
                return;

            var request = new Factory4Workshop4_10LineSummaryQueryRequestDto
            {
                SN = QuerySN,
            };
            var apiResult = await _summaryApi.SNQuery(request);

            Tb_Factory4Workshop4_10LineSummary findValue;
            if (apiResult.IsSuccess && apiResult.Data?.List?.Count > 0)
            {
                findValue = apiResult.Data.List.First();
            }
            else
            {
                findValue = new Tb_Factory4Workshop4_10LineSummary { SN = QuerySN };
            }

            var historyScan = HistoryScanMarkingNos.FirstOrDefault(x => x.MarkingNo == QuerySN);
            if (historyScan != null)
            {
                HistoryScanMarkingNos.Remove(historyScan);
                HistoryScanMarkingNos.Insert(0, historyScan);
                var tmp = QueryValue.FirstOrDefault(x => x.SN == QuerySN);
                if (tmp != null)
                    QueryValue.Remove(tmp);
            }
            else
            {
                HistoryScanMarkingNos.Insert(0, new ScanRecord
                {
                    MarkingNo = QuerySN,
                    IsHave = apiResult.IsSuccess && apiResult.Data?.List?.Count > 0,
                });
            }

            QueryValue.Insert(0, findValue);

            while (HistoryScanMarkingNos.Count > 100)
            {
                HistoryScanMarkingNos.RemoveAt(HistoryScanMarkingNos.Count - 1);
                QueryValue.RemoveAt(QueryValue.Count - 1);
            }
            textBox.Text = string.Empty;
            QuerySN = null;
        }

        void ClearHistoryMarkingNo()
        {
            HistoryScanMarkingNos.Clear();
            _mkNoQuValues.Clear();
            QueryValue.Clear();
        }

        void RemoveHistoryScan(ScanRecord record)
        {
            if (record != null)
            {
                HistoryScanMarkingNos.Remove(record);
                var tmp = QueryValue.FirstOrDefault(x => x.SN == record.MarkingNo);
                if (tmp != null)
                    QueryValue.Remove(tmp);
            }
        }
    }
}
