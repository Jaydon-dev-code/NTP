using NPOI.SS.UserModel;
using NPOI.XSSF.Streaming;
using SL.MLineDataPrecisionTracking.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Expand
{
    public static class ExcelExpand
    {
        public static Result ExportToExcel<T>(List<T> dataList, string saveFileName)
        {
            try
            {
                using (ExcelExportWriter<T> writer = new ExcelExportWriter<T>(saveFileName))
                {
                    writer.WriteRows(dataList);
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }
    }
    /// <summary>
    /// 流式Excel写入器：基于 SXSSFWorkbook 边写边刷盘，
    /// 内存中只保留最近一窗口的行，避免大数据量导出时内存溢出。
    /// </summary>
    public class ExcelExportWriter<T> : IDisposable
    {
        /// <summary>
        /// 内存中保留的行窗口大小，超出即刷入临时文件
        /// </summary>
        private const int RowAccessWindowSize = 100;

        private readonly SXSSFWorkbook _workbook;
        private readonly ISheet _sheet;
        private readonly List<PropertyInfo> _properties;
        private readonly int[] _maxColLen;
        private readonly string _saveFileName;
        private int _rowIndex;

        public ExcelExportWriter(string saveFileName)
        {
            _saveFileName = saveFileName;
            _workbook = new SXSSFWorkbook(RowAccessWindowSize);
            _sheet = _workbook.CreateSheet("Sheet1");

            // 仅导出带 Description 标注的属性，顺序与实体声明一致
            _properties = typeof(T)
                .GetProperties()
                .Where(p => p.GetCustomAttribute<DescriptionAttribute>() != null)
                .ToList();
            _maxColLen = new int[_properties.Count];

            WriteHeader();
        }

        /// <summary>
        /// 追加一批数据行到 Excel（可多次调用，配合分页查询边查边写）
        /// </summary>
        public void WriteRows(IEnumerable<T> dataList)
        {
            if (dataList == null)
            {
                return;
            }

            foreach (var data in dataList)
            {
                IRow row = _sheet.CreateRow(_rowIndex++);
                for (int colIndex = 0; colIndex < _properties.Count; colIndex++)
                {
                    object val = _properties[colIndex].GetValue(data);
                    string str = val?.ToString() ?? "";
                    if (str.Length > _maxColLen[colIndex])
                    {
                        _maxColLen[colIndex] = str.Length;
                    }
                    row.CreateCell(colIndex).SetCellValue(str);
                }
            }
        }

        private void WriteHeader()
        {
            IRow headerRow = _sheet.CreateRow(_rowIndex++);
            for (int i = 0; i < _properties.Count; i++)
            {
                var desc = _properties[i].GetCustomAttribute<DescriptionAttribute>();
                string headerName = desc?.Description ?? _properties[i].Name;
                headerRow.CreateCell(i).SetCellValue(headerName);
                _maxColLen[i] = headerName.Length;
            }
        }

        public void Dispose()
        {
            try
            {
                // 按内容长度自动列宽（Excel 单列上限 255 字符）
                for (int i = 0; i < _properties.Count; i++)
                {
                    _sheet.SetColumnWidth(i, Math.Min((_maxColLen[i] + 2) * 256, 255 * 256));
                }

                using (FileStream fs = new FileStream(_saveFileName, FileMode.Create))
                {
                    _workbook.Write(fs);
                }
            }
            finally
            {
                _workbook.Dispose();
            }
        }
    }
}
