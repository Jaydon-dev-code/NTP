using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Dtos.Request;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Dtos.Response;
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    public partial class Rcl_MeticulousPursuitViewModel : ObservableObject
    {
        private ObservableCollection<Tb_HeatTreatmentData> _lineSummaryDataList = new ObservableCollection<Tb_HeatTreatmentData>();
        public ObservableCollection<Tb_HeatTreatmentData> LineSummaryDataList
        {
            get => _lineSummaryDataList;
            set => SetProperty(ref _lineSummaryDataList, value);
        }

        private int _totalCount = 0;
        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => SetProperty(ref _pageSize, value);
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private readonly Rcl_MeticulousPursuitApi _meticulousPursuitApi;

        public Rcl_MeticulousPursuitViewModel(
            Rcl_MeticulousPursuitApi meticulousPursuitApi
        )
        {
            _meticulousPursuitApi = meticulousPursuitApi;
            var lastDay = DateTime.Now.Date;
            HeatTreatmentDataQueryRequest = new HeatTreatmentDataQueryRequestDto
            {
                PageNumber = 1,
                PageSize = 10,
                RefinedSearch = new RefinedSearchCriteria
                {
                    StartDate = lastDay.Date,
                    EndDate = lastDay.Date,
                    StartTime = lastDay,
                    EndTime = lastDay.AddTicks(-1),
                    Result = SL.MLineDataPrecisionTracking.Models.Enum.ResultEnum.ALL
                }
            };
        }

        private HeatTreatmentDataQueryRequestDto _heatTreatmentDataQueryRequest;
        public HeatTreatmentDataQueryRequestDto HeatTreatmentDataQueryRequest
        {
            get => _heatTreatmentDataQueryRequest;
            set => SetProperty(ref _heatTreatmentDataQueryRequest, value);
        }

        private RelayCommand _quDataCommand;
        public IRelayCommand QuDataCommand
        {
            get
            {
                if (_quDataCommand == null)
                {
                    _quDataCommand = new RelayCommand(QuData);
                }
                return _quDataCommand;
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

        private async void QuData()
        {
            IsLoading = true;
            try
            {
                HeatTreatmentDataQueryRequest.PageNumber = CurrentPage;
                HeatTreatmentDataQueryRequest.PageSize = PageSize;

                var result = await _meticulousPursuitApi.QueryablToPagee(HeatTreatmentDataQueryRequest);
                if (result.IsSuccess)
                {
                    LineSummaryDataList.Clear();
                    foreach (var item in result.Data.List)
                    {
                        LineSummaryDataList.Add(item);
                    }
                    TotalCount = result.Data.TotalCount;
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

        private async void MarkingNoQuery()
        {
            IsLoading = true;
            try
            {
                var result = await _meticulousPursuitApi.MarkingNoQuery(HeatTreatmentDataQueryRequest);
                if (result.IsSuccess)
                {
                    LineSummaryDataList.Clear();
                    foreach (var item in result.Data.List)
                    {
                        LineSummaryDataList.Add(item);
                    }
                    TotalCount = result.Data.TotalCount;
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

        private async void SaveQuery()
        {
            IsLoading = true;
            try
            {
                var result = await _meticulousPursuitApi.SaveQuery(HeatTreatmentDataQueryRequest);
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