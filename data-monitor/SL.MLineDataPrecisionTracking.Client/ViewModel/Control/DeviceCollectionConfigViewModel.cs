using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Win32;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    public partial class DeviceCollectionConfigViewModel : ObservableObject
    {
        private List<ServiceInfo> _services = new List<ServiceInfo>();
        public List<ServiceInfo> Services
        {
            get => _services;
            set => SetProperty(ref _services, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private readonly ServiceApi _serviceApi;


        public DeviceCollectionConfigViewModel(
 
            ServiceApi serviceApi
        )
        {
          
            _serviceApi = serviceApi;
            LoadServicesAsync();
        }

        private AsyncRelayCommand _importAsyncCommand;
        public AsyncRelayCommand ImportAsyncCommand
        {
            get
            {
                if (_importAsyncCommand == null)
                {
                    _importAsyncCommand = new AsyncRelayCommand(ImportAsync);
                }
                return _importAsyncCommand;
            }
        }

        private RelayCommand _exportTemplateCommand;
        public IRelayCommand ExportTemplateCommand
        {
            get
            {
                if (_exportTemplateCommand == null)
                {
                    _exportTemplateCommand = new RelayCommand(ExportTemplate);
                }
                return _exportTemplateCommand;
            }
        }

        private AsyncRelayCommand _loadServicesCommand;
        public AsyncRelayCommand LoadServicesCommand
        {
            get
            {
                if (_loadServicesCommand == null)
                {
                    _loadServicesCommand = new AsyncRelayCommand(LoadServicesAsync);
                }
                return _loadServicesCommand;
            }
        }

        private AsyncRelayCommand<string> _startServiceCommand;
        public AsyncRelayCommand<string> StartServiceCommand
        {
            get
            {
                if (_startServiceCommand == null)
                {
                    _startServiceCommand = new AsyncRelayCommand<string>(StartServiceAsync);
                }
                return _startServiceCommand;
            }
        }

        private AsyncRelayCommand<string> _stopServiceCommand;
        public AsyncRelayCommand<string> StopServiceCommand
        {
            get
            {
                if (_stopServiceCommand == null)
                {
                    _stopServiceCommand = new AsyncRelayCommand<string>(StopServiceAsync);
                }
                return _stopServiceCommand;
            }
        }

        private AsyncRelayCommand<string> _restartServiceCommand;
        public AsyncRelayCommand<string> RestartServiceCommand
        {
            get
            {
                if (_restartServiceCommand == null)
                {
                    _restartServiceCommand = new AsyncRelayCommand<string>(RestartServiceAsync);
                }
                return _restartServiceCommand;
            }
        }

        private AsyncRelayCommand<ServiceInfo> _setServiceEnabledCommand;
        public AsyncRelayCommand<ServiceInfo> SetServiceEnabledCommand
        {
            get
            {
                if (_setServiceEnabledCommand == null)
                {
                    _setServiceEnabledCommand = new AsyncRelayCommand<ServiceInfo>(SetServiceEnabledAsync);
                }
                return _setServiceEnabledCommand;
            }
        }

        private AsyncRelayCommand _startAllServicesCommand;
        public AsyncRelayCommand StartAllServicesCommand
        {
            get
            {
                if (_startAllServicesCommand == null)
                {
                    _startAllServicesCommand = new AsyncRelayCommand(StartAllServicesAsync);
                }
                return _startAllServicesCommand;
            }
        }

        private AsyncRelayCommand _stopAllServicesCommand;
        public AsyncRelayCommand StopAllServicesCommand
        {
            get
            {
                if (_stopAllServicesCommand == null)
                {
                    _stopAllServicesCommand = new AsyncRelayCommand(StopAllServicesAsync);
                }
                return _stopAllServicesCommand;
            }
        }

        async Task ImportAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx",
                Title = "导入点位信息",
            };

            //if (openFileDialog.ShowDialog() == true)
            //{
            //    var re = await _plcAddressExcelImportService.ImportPlcPointsAsync(
            //        openFileDialog.FileName
            //    );

            //    if (re)
            //    {
            //        HandyControl.Controls.MessageBox.Success("导入成功！");
            //    }
            //    else
            //    {
            //        HandyControl.Controls.MessageBox.Warning("导入失败！");
            //    }
            //}
        }

        void ExportTemplate()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx",
                Title = "保存Excel文件",
                FileName = $"数据导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
            };
            return;

            //if (saveFileDialog.ShowDialog() == true)
            //{
            //    using (ExcelPackage package = new ExcelPackage())
            //    {
            //        // 创建工作表
            //        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("导出数据");

            //        // 写入表头和数据
            //        for (int i = 0; i < _testData.Count; i++)
            //        {
            //            string[] rowData = _testData[i].Split(',');
            //            for (int j = 0; j < rowData.Length; j++)
            //            {
            //                worksheet.Cells[i + 1, j + 1].Value = rowData[j];
            //            }
            //        }

            //        // 保存文件
            //        File.WriteAllBytes(saveFileDialog.FileName, package.GetAsByteArray());
            //    }
            //    MessageBox.Show("Excel导出成功！", "提示");
            //}
        }

        public async Task LoadServicesAsync()
        {
            IsLoading = true;
            try
            {
                var result = await _serviceApi.GetAllServicesAsync();
                if (result.IsSuccess)
                {
                    Services = result.Data;
                }
                else
                {
                    HandyControl.Controls.MessageBox.Error(result.Message);
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"获取服务状态时发生错误: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task StartServiceAsync(string id)
        {
            try
            {
                var result = await _serviceApi.StartServiceAsync(id);
                if (result.IsSuccess)
                {
                    HandyControl.Controls.MessageBox.Success(result.Message);
                    await LoadServicesAsync();
                }
                else
                {
                    HandyControl.Controls.MessageBox.Error(result.Message);
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"启动服务时发生错误: {ex.Message}");
            }
        }

        public async Task StopServiceAsync(string id)
        {
            try
            {
                var result = await _serviceApi.StopServiceAsync(id);
                if (result.IsSuccess)
                {
                    HandyControl.Controls.MessageBox.Success(result.Message);
                    await LoadServicesAsync();
                }
                else
                {
                    HandyControl.Controls.MessageBox.Error(result.Message);
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"停止服务时发生错误: {ex.Message}");
            }
        }

        public async Task RestartServiceAsync(string id)
        {
            try
            {
                var result = await _serviceApi.RestartServiceAsync(id);
                if (result.IsSuccess)
                {
                    HandyControl.Controls.MessageBox.Success(result.Message);
                    await LoadServicesAsync();
                }
                else
                {
                    HandyControl.Controls.MessageBox.Error(result.Message);
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"重启服务时发生错误: {ex.Message}");
            }
        }

        public async Task SetServiceEnabledAsync(ServiceInfo serviceInfo)
        {
            //var (id, enabled) = param;
            try
            {
                serviceInfo.IsEnabled = !serviceInfo.IsEnabled;
                var result = await _serviceApi.SetServiceEnabledAsync(serviceInfo);
                if (result.IsSuccess)
                {
                    HandyControl.Controls.MessageBox.Success(result.Message);
                    await LoadServicesAsync();
                }
                else
                {
                    HandyControl.Controls.MessageBox.Error(result.Message);
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"设置服务状态时发生错误: {ex.Message}");
            }
        }

        public async Task StartAllServicesAsync()
        {
            try
            {
                var result = await _serviceApi.StartAllServicesAsync();
                if (result.IsSuccess)
                {
                    HandyControl.Controls.MessageBox.Success(result.Message);
                    await LoadServicesAsync();
                }
                else
                {
                    HandyControl.Controls.MessageBox.Error(result.Message);
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"启动所有服务时发生错误: {ex.Message}");
            }
        }

        public async Task StopAllServicesAsync()
        {
            try
            {
                var result = await _serviceApi.StopAllServicesAsync();
                if (result.IsSuccess)
                {
                    HandyControl.Controls.MessageBox.Success(result.Message);
                    await LoadServicesAsync();
                }
                else
                {
                    HandyControl.Controls.MessageBox.Error(result.Message);
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"停止所有服务时发生错误: {ex.Message}");
            }
        }
    }
}