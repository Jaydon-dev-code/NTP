using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    public partial class MeticulousPursuitViewModel : ObservableObject
    {
        private PaginationPage _paginationPage;
        public PaginationPage PaginationPage
        {
            get => _paginationPage;
            set => SetProperty(ref _paginationPage, value);
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

        private RelayCommand _queryCommand;
        public IRelayCommand QueryCommand
        {
            get
            {
                if (_queryCommand == null)
                {
                    _queryCommand = new RelayCommand(Query);
                }
                return _queryCommand;
            }
        }

        private RelayCommand _saveQueryCommand;
        public IRelayCommand SaveQueryCommand
        {
            get
            {
                if (_saveQueryCommand == null)
                {
                    _saveQueryCommand = new RelayCommand(SaveQuery);
                }
                return _saveQueryCommand;
            }
        }
        private RelayCommand<System.Windows.Controls.TextBox> _markingNoQueryCommand;
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

        private RelayCommand _pageUpdatedCommand;
        public IRelayCommand PageUpdatedCommand
        {
            get
            {
                if (_pageUpdatedCommand == null)
                {
                    _pageUpdatedCommand = new RelayCommand(PageUpdated);
                }
                return _pageUpdatedCommand;
            }
        }
        private RelayCommand<System.Windows.Controls.TextBox> _openScanCodeModelCommand;
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
                    _removeHistoryScanCommand = new RelayCommand<ScanRecord> (RemoveHistoryScan);
                }
                return _removeHistoryScanCommand;
            }
        }

    

       

        public MeticulousPursuitViewModel()
        {
            PaginationPage = new PaginationPage();
            var lastDay = DateTime.Now.Date.AddDays(-1);
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
        }
        private void RemoveHistoryScan(ScanRecord record)
        {
            if (record!=null)
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
            if (QueryConditions?.MarkingNo==null || QueryConditions.MarkingNo==string.Empty)
            {
                return;
            }
            var historyScanMarkingNo = HistoryScanMarkingNos.FirstOrDefault(x =>
                x.MarkingNo == QueryConditions.MarkingNo
            );
            if (historyScanMarkingNo != null)
            {
                HistoryScanMarkingNos.Remove(historyScanMarkingNo);
                HistoryScanMarkingNos.Insert(0,historyScanMarkingNo);
            }
            else
            {
                HistoryScanMarkingNos.Insert(0,
                    new ScanRecord() { MarkingNo = QueryConditions.MarkingNo }
                );
            }
            textBox.Text = string.Empty;
        }

        void Query()
        { //{//扫描模式关注 条码
            //    if (QueryConditions?.IsScanCode == true) { }
            //    else { }
        }

        void SaveQuery() { }

        void PageUpdated() { }
    }
}
