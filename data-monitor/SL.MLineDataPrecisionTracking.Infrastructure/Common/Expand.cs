using McpXLib.Enums;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SL.MLineDataPrecisionTracking.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Common
{
    public static class Expand
    {
        private static readonly Encoding sjisEncoding;
         static Expand()
        {
#if !NETSTANDARD2_0
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            sjisEncoding = Encoding.GetEncoding("shift_jis");
        }

        public static Result ExportToExcel<T>(List<T> dataList, string saveFileName)
        { 
            try
           {
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet("Sheet1");

                // 1. 获取实体类所有属性
                PropertyInfo[] properties = typeof(T).GetProperties();

                // 2. 创建表头（读取 Description 中文）
                IRow headerRow = sheet.CreateRow(0);
                for (int i = 0; i < properties.Length; i++)
                {
                    var desc = properties[i].GetCustomAttribute<DescriptionAttribute>();
                    string headerName = desc?.Description ?? properties[i].Name; // 自动取中文

                    ICell cell = headerRow.CreateCell(i);
                    cell.SetCellValue(headerName);
                }

                // 3. 写数据
                for (int rowIndex = 0; rowIndex < dataList.Count; rowIndex++)
                {
                    IRow row = sheet.CreateRow(rowIndex + 1);
                    var data = dataList[rowIndex];

                    for (int colIndex = 0; colIndex < properties.Length; colIndex++)
                    {
                        object val = properties[colIndex].GetValue(data);
                        row.CreateCell(colIndex).SetCellValue(val?.ToString() ?? "");
                    }
                }

                // 4. 自动列宽
                for (int i = 0; i < properties.Length; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                // 5. 保存文件
                using (FileStream fs = new FileStream(saveFileName, FileMode.Create))
                {
                    workbook.Write(fs);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }
        public static int GetTypeByteLength(this Type type)
        {
            int typeSize;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                    typeSize = 1;
                    break;
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    typeSize = 2;
                    break;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Single:
                    typeSize = 4;
                    break;
                case TypeCode.Double:
                case TypeCode.Int64:
                    typeSize = 8;
                    break;
                default:
                    throw new NotSupportedException($"不支持的数据类型: {type.Name}");
            }
            return typeSize;
        }

        static DataTable _dataTable = new DataTable();

        public static object StringCompute(this string expression, params string[] param)
        {
            var format = string.Format(expression, param);
            return _dataTable.Compute(format, string.Empty);
        }

        public static int GetTypeOfShortOffset(this TypeCode type)
        {
            int result;
            switch (type)
            {
                case TypeCode.Boolean:
                    result = 1;
                    break;
                case TypeCode.String:
                    result = 1;
                    break;
                case TypeCode.Int16:
                    result = 1;
                    break;
                case TypeCode.UInt16:
                    result = 1;
                    break;
                case TypeCode.Int32:
                    result = 2;
                    break;
                case TypeCode.UInt32:
                    result = 2;
                    break;
                case TypeCode.Single:
                    result = 2;
                    break;
                case TypeCode.Double:
                    result = 4;
                    break;
                case TypeCode.Int64:
                    result = 4;
                    break;
                default:
                    throw new NotSupportedException($"不支持的数据类型: {type}");
            }
            return result;
        }

        public static int GetTypeByteLength(this TypeCode type)
        {
            int result;
            switch (type)
            {
                case TypeCode.Boolean:
                    result = 1;
                    break;
                case TypeCode.Byte:
                    result = 1;
                    break;
                case TypeCode.String:
                    result = 1;
                    break;
                case TypeCode.Int16:
                    result = 2;
                    break;
                case TypeCode.UInt16:
                    result = 2;
                    break;
                case TypeCode.Int32:
                    result = 4;
                    break;
                case TypeCode.UInt32:
                    result = 4;
                    break;
                case TypeCode.Single:
                    result = 4;
                    break;
                case TypeCode.Double:
                    result = 8;
                    break;
                case TypeCode.Int64:
                    result = 8;
                    break;
                default:
                    throw new NotSupportedException($"不支持的数据类型: {type}");
            }
            return result;
        }

        /// <summary>
        /// 大端模式 Byte数组 转 short (PLC默认大端)
        /// </summary>
        public static short ToInt16(this byte[] buffer, int startIndex)
        {
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(buffer, startIndex, 2);
            return BitConverter.ToInt16(buffer, startIndex);
        }

        /// <summary>
        /// 大端模式 Byte数组 转 int
        /// </summary>
        public static int ToInt32(this byte[] buffer, int startIndex)
        {
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(buffer, startIndex, 4);
            return BitConverter.ToInt32(buffer, startIndex);
        }

        /// <summary>
        /// 大端模式 Byte数组 转 float
        /// </summary>
        public static float ToSingle(this byte[] buffer, int startIndex)
        {
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(buffer, startIndex, 4);
            return BitConverter.ToSingle(buffer, startIndex);
        }

        public static Prefix ToPrefix(this string prefix)
        {
            if (Enum.TryParse<Prefix>(prefix, out Prefix result))
            {
                return result;
            }
            else
            {
                throw new NotSupportedException($"不支转换类型: {prefix}");
            }
        }

        /// <summary>
        /// 从字节数组解析出【数组类型】
        /// </summary>
        /// <param name="buffer">PLC原始字节数组</param>
        /// <param name="startIndex">起始索引</param>
        /// <param name="type">数据类型</param>
        /// <param name="length">数组长度（几个元素）</param>
        /// <returns>对应类型的数组（object装着）</returns>
        public static List<object> ConvertToValues(
            this byte[] buffer,
            int startIndex,
            TypeCode type,
            int length = 1
        )
        {
            switch (type)
            {
                case TypeCode.Boolean:
                    return ParseArray(buffer, startIndex, length, GetTypeByteLength(type), type);
                case TypeCode.Byte:
                    return ParseArray(buffer, startIndex, length, GetTypeByteLength(type), type);
                case TypeCode.Int16:
                    return ParseArray(buffer, startIndex, length, GetTypeByteLength(type), type);
                case TypeCode.UInt16:
                    return ParseArray(buffer, startIndex, length, GetTypeByteLength(type), type);
                case TypeCode.Int32:
                    return ParseArray(buffer, startIndex, length, GetTypeByteLength(type), type);
                case TypeCode.UInt32:
                    return ParseArray(buffer, startIndex, length, GetTypeByteLength(type), type);
                case TypeCode.Single:
                    return ParseArray(buffer, startIndex, length, GetTypeByteLength(type), type);
                case TypeCode.Double:
                    return ParseArray(buffer, startIndex, length, GetTypeByteLength(type), type);
                case TypeCode.String:
                    return ParseArray(buffer, startIndex, length, GetTypeByteLength(type), type);
                default:
                    throw new NotSupportedException($"不支持解析类型数组: {type}");
            }
        }

        /// <summary>
        /// 通用数组解析（自动步进字节）
        /// </summary>
        private static List<object> ParseArray(
            byte[] buffer,
            int startIndex,
            int count,
            int typeByteSize,
            TypeCode type
        )
        {
            List<object> list = new List<object>();

            for (int i = 0; i < count; i++)
            {
                int index = startIndex + i * typeByteSize;
                var value = ConvertToValue(buffer, index, type);
                list.Add(value);
            }
            return list;
        }

        public static object ConvertToValue(byte[] buffer, int startIndex, TypeCode type)
        {
            switch (type)
            {
                case TypeCode.Boolean:
                    return buffer[startIndex] != 0;
                case TypeCode.Byte:
                    return buffer[startIndex];
                case TypeCode.Int16:
                    return ToInt16(buffer, startIndex);
                case TypeCode.UInt16:
                    return (ushort)ToInt16(buffer, startIndex);
                case TypeCode.Int32:
                    return ToInt32(buffer, startIndex);
                case TypeCode.UInt32:
                    return (uint)ToInt32(buffer, startIndex);
                case TypeCode.Single:
                    return ToSingle(buffer, startIndex);
                case TypeCode.Double:
                    return BitConverter.ToDouble(buffer, startIndex);
                case TypeCode.String:
                    return ConvertString(new byte[] { buffer[startIndex] } );
                default:
                    throw new NotSupportedException($"不支持解析类型: {type}");
            }
        }

        internal static string ConvertString(byte[] bytes)
        {
            int length = Array.IndexOf(bytes, (byte)0x00);
            if (length == -1)
            {
                length = bytes.Length;
            }

            return sjisEncoding.GetString(
                bytes.Take(length).ToArray()
            );
        }

        /// <summary>
        /// PLC大端 Double
        /// </summary>
        public static double ToDouble(byte[] buffer, int startIndex)
        {
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(buffer, startIndex, 8);
            return BitConverter.ToDouble(buffer, startIndex);
        }

        public static TypeCode ToTypeCode(this string type)
        {
            if (Enum.TryParse<TypeCode>(type, out TypeCode result))
            {
                return result;
            }
            else
            {
                throw new NotSupportedException($"不支持解析类型: {result}");
            }
        }
    }
}
