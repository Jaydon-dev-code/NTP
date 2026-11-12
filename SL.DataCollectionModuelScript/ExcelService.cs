using System;
using System.Collections.Generic;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace SL.DataCollectionModuelScript
{
    /// <summary>
    /// Excel 读写（NPOI）：读取设备列表、配置成功标绿、还原成功清绿
    /// 表头：设备编号 | 生产线号 | IP | mac地址 | 网关 | 模块(可选)
    /// 注意：「mac地址」列内容实际是子网掩码（如 255.255.255.0），不是 MAC。
    /// </summary>
    public class ExcelService
    {
        private readonly string _path;

        public ExcelService(string path) => _path = path;

        /// <summary>读取全部设备</summary>
        public List<DeviceInfo> ReadAll()
        {
            var list = new List<DeviceInfo>();
            if (!File.Exists(_path))
                throw new FileNotFoundException("Excel文件不存在：" + _path);

            using var fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var wb = new XSSFWorkbook(fs);
            var sheet = wb.GetSheetAt(0);
            var header = sheet.GetRow(0) ?? throw new InvalidDataException("Excel第1行为空，缺少表头");

            int cId = Col(header, "设备编号");
            int cLine = Col(header, "生产线号");
            int cIp = Col(header, "IP地址");
            // 表头叫「mac地址」，内容实际是子网掩码
            int cMask = Col(header, "子网掩码");
            int cGw = Col(header, "网关");
            int cMod = TryCol(header, "模块"); // 可选列

            for (int r = 1; r <= sheet.LastRowNum; r++)
            {
                var row = sheet.GetRow(r);
                if (row == null) continue;
                string id = Text(row, cId);
                if (string.IsNullOrWhiteSpace(id)) continue;

                // 模块列解析，失败默认 BCNet_Q
                var mod = ModuelTypeEnum.BCNet_Q;
                if (cMod >= 0)
                    Enum.TryParse(Text(row, cMod), true, out mod);

                list.Add(new DeviceInfo
                {
                    ExcelRowIndex = r,
                    DeviceId = id.Trim(),
                    Line = Text(row, cLine).Trim(),
                    Ip = Text(row, cIp).Trim(),
                    Mask = Text(row, cMask).Trim(),
                    Gateway = Text(row, cGw).Trim(),
                    ModuleType = mod,
                    IsSuccess = IsGreen(row.GetCell(cId)),
                    Status = "待执行"
                });
            }
            return list;
        }

        /// <summary>设备编号单元格标绿（配置成功）</summary>
        public void MarkSuccess(DeviceInfo d) => SetGreen(d, true);

        /// <summary>清除设备编号单元格绿色（还原成功）</summary>
        public void ClearSuccess(DeviceInfo d) => SetGreen(d, false);

        // ========== 内部 ==========

        private void SetGreen(DeviceInfo d, bool green)
        {
            // 先读取，读完释放文件流
            XSSFWorkbook wb;
            using (var fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                wb = new XSSFWorkbook(fs);
            }

            var sheet = wb.GetSheetAt(0);
            var header = sheet.GetRow(0);
            int cId = Col(header, "设备编号");
            var row = sheet.GetRow(d.ExcelRowIndex)
                ?? throw new InvalidDataException($"找不到Excel第{d.ExcelRowIndex + 1}行");
            var cell = row.GetCell(cId) ?? row.CreateCell(cId);

            var style = (XSSFCellStyle)wb.CreateCellStyle();
            if (green)
            {
                // ✅ XSSF RGB 正确写法，删掉 FillForegroundColor=10
                style.FillForegroundColorColor = new XSSFColor(new byte[] { 0, 200, 0 });
                style.FillPattern = FillPattern.SolidForeground;
            }
            else
            {
                style.FillPattern = FillPattern.NoFill;
            }
            cell.CellStyle = style;

            // ✅ 优先写到内存流，再写入文件，避免文件包损坏
            using var ms = new MemoryStream();
            wb.Write(ms);
            ms.Position = 0;

            using var wfs = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.Read);
            ms.CopyTo(wfs);

            wb.Close();
            wb.Dispose();
        }

        private static int Col(IRow header, string name)
        {
            int i = TryCol(header, name);
            return i < 0 ? throw new InvalidDataException($"Excel表头缺少列：{name}") : i;
        }

        private static int TryCol(IRow header, string name)
        {
            for (int i = 0; i < header.LastCellNum; i++)
            {
                string v = header.GetCell(i)?.ToString()?.Trim() ?? "";
                if (v.Equals(name, StringComparison.OrdinalIgnoreCase)) return i;
            }
            return -1;
        }

        private static string Text(IRow row, int col)
        {
            if (col < 0) return "";
            var c = row.GetCell(col);
            return c?.CellType switch
            {
                CellType.String => c.StringCellValue,
                CellType.Numeric => c.NumericCellValue.ToString(),
                _ => c?.ToString() ?? ""
            };
        }

        private static bool IsGreen(ICell? cell)
        {
            try
            {
                if (cell?.CellStyle == null) return false;
                if (cell.CellStyle.FillPattern != FillPattern.SolidForeground) return false;
                if (cell is XSSFCell)
                {
                    var rgb = (cell.CellStyle.FillForegroundColorColor as XSSFColor)?.RGB;
                    if (rgb is { Length: >= 3 })
                        return rgb[1] > 150 && rgb[1] > rgb[0] + 50 && rgb[1] > rgb[2] + 50;
                }
                short idx = cell.CellStyle.FillForegroundColor;
                return idx is 10 or 17 or 51;
            }
            catch { return false; }
        }
    }
}
