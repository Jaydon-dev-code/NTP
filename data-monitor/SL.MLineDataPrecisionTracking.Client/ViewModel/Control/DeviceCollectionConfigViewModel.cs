using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Win32;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Models.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    public partial class DeviceCollectionConfigViewModel : ObservableObject
    {

        PlcAddressExcelImportService _plcAddressExcelImportService;

        private RelayCommand _importAsyncCommand;
        public IRelayCommand ImportAsyncCommand
        {
            get
            {
                if (_importAsyncCommand == null)
                {
                    _importAsyncCommand = new RelayCommand(async () => await ImportAsync());
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
        public DeviceCollectionConfigViewModel(PlcAddressExcelImportService plcAddressExcelImportService)
        {
            _plcAddressExcelImportService=plcAddressExcelImportService;


        }

     
        async Task ImportAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx",
                Title = "导入点位信息",
              
            };

            if (openFileDialog.ShowDialog() == true)
            {                                               
            var re=await _plcAddressExcelImportService.ImportPlcPointsAsync(openFileDialog.FileName);


            }
        

    }


        void ExportTemplate()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx",
                Title = "保存Excel文件",
                FileName = $"数据导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            };
            return ;

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

    }
}

