using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Dtos.Request;
using SL.MLineDataPrecisionTracking.Models.Dtos.Response;
using SL.MLineDataPrecisionTracking.Models.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    public partial class Rcl_MeticulousPursuitViewModel : ObservableObject
    {
        private ObservableCollection<HeatTreatmentDataDto> _heatTreatmentData;
     
        public ObservableCollection<HeatTreatmentDataDto> HeatTreatmentData
        {
            get => _heatTreatmentData;
            set => SetProperty(ref _heatTreatmentData, value);
        }
        private PaginationPage _paginationPages;
        public PaginationPage PaginationPages
        {
            get => _paginationPages;
            set => SetProperty(ref _paginationPages, value);
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        private RefinedSearchCriteria _queryConditions;
        public RefinedSearchCriteria QueryConditions
        {
            get => _queryConditions;
            set => SetProperty(ref _queryConditions, value);
        }

        private static ObservableCollection<int> _pageSoure;
        public ObservableCollection<int> PageSoure
        {
            get => _pageSoure;
            set => SetProperty(ref _pageSoure, value);
        }

        private RelayCommand _queryCommand;
        public IRelayCommand QueryCommand
        {
            get
            {
                if (_queryCommand == null)
                {
                    _queryCommand = new RelayCommand(QuData);
                }
                return _queryCommand;
            }
        }

        private RelayCommand _markingNoQueryCommand;
        public IRelayCommand MarkingNoQueryCommand
        {
            get
            {
                if (_markingNoQueryCommand == null)
                {
                    _markingNoQueryCommand = new RelayCommand(MarkingNoQuery);
                }
                return _markingNoQueryCommand;
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

        private RelayCommand _exportCommand;
        public IRelayCommand ExportCommand
        {
            get
            {
                if (_exportCommand == null)
                {
                    _exportCommand = new RelayCommand(Export);
                }
                return _exportCommand;
            }
        }

        private readonly Rcl_MeticulousPursuitApi _meticulousPursuitApi;

        public Rcl_MeticulousPursuitViewModel(Rcl_MeticulousPursuitApi meticulousPursuitApi)
        {
            _meticulousPursuitApi = meticulousPursuitApi;
            HeatTreatmentData = new ObservableCollection<HeatTreatmentDataDto>();
            PaginationPages = new PaginationPage();
            var lastDay = DateTime.Now.Date;
            QueryConditions = new RefinedSearchCriteria()
            {
                StartDate = lastDay.Date,
                EndDate = lastDay.Date,
                StartTime = lastDay,
                EndTime = lastDay.AddTicks(-1),
            };
            PageSoure = new ObservableCollection<int>() { 100, 200, 500 };
            PaginationPages.DataCountPerPage = 100;
        }

        private async void QuData()
        {
            IsLoading = true;
            try
            {
                var result = await _meticulousPursuitApi.QueryablToPagee(GetQueryRequest());
                if (result.IsSuccess)
                {
                    HeatTreatmentData.Clear();
                    foreach (var item in result.Data.List)
                    {
                        HeatTreatmentData.Add(new HeatTreatmentDataDto(item));
                    }
                    PaginationPages.MaxPageCount = result.Data.TotalCount;
                }
                else
                {
                    HandyControl.Controls.MessageBox.Error(result.Message);
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"查询数据时发生错误: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        HeatTreatmentDataQueryRequestDto GetQueryRequest()
        {
            HeatTreatmentDataQueryRequestDto queryRequest = new HeatTreatmentDataQueryRequestDto();
            queryRequest.PageIndex = PaginationPages.PageIndex;
            queryRequest.DataCountPerPage = PaginationPages.DataCountPerPage;
            queryRequest.RefinedSearch = QueryConditions;
            return queryRequest;
        }

        private async void MarkingNoQuery() { }

        private async void SaveQuery()
        {
            IsLoading = true;
            try
            {
                var result = await _meticulousPursuitApi.SaveQuery(GetQueryRequest());
                if (result.IsSuccess)
                {
                    // 这里可以处理导出逻辑
                    HandyControl.Controls.MessageBox.Success("导出成功！");
                }
                else
                {
                    HandyControl.Controls.MessageBox.Error(result.Message);
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"导出数据时发生错误: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Export()
        {
            // 导出逻辑
        }
    }
}
