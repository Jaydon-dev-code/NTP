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
using McpXLib.Enums;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SqlSugar;

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

        static Type _dfStringtype = typeof(string);

        public static Result ExportToExcel<T>(List<T> dataList, string saveFileName)
        {
            try
            {
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet("Sheet1");

                // 1. 获取实体类所有属性
                List<PropertyInfo> properties = typeof(T).GetProperties().ToList();

                // 2. 创建表头（读取 Description 中文）
                IRow headerRow = sheet.CreateRow(0);
                List<PropertyInfo> ignoreInfo = new List<PropertyInfo>();
                for (int i = 0; i < properties.Count; i++)
                {
                    var desc = properties[i].GetCustomAttribute<DescriptionAttribute>();
                    if (desc == null)
                    {
                        ignoreInfo.Add(properties[i]);
                        continue;
                    }
                    string headerName = desc?.Description ?? properties[i].Name; // 自动取中文

                    ICell cell = headerRow.CreateCell(i - ignoreInfo.Count);
                    cell.SetCellValue(headerName);
                }
                foreach (var item in ignoreInfo)
                {
                    properties.Remove(item);
                }
                // 3. 写数据
                for (int rowIndex = 0; rowIndex < dataList.Count; rowIndex++)
                {
                    IRow row = sheet.CreateRow(rowIndex + 1);
                    var data = dataList[rowIndex];

                    for (int colIndex = 0; colIndex < properties.Count; colIndex++)
                    {
                        object val = properties[colIndex].GetValue(data);
                        row.CreateCell(colIndex).SetCellValue(val?.ToString() ?? "");
                    }
                }

                // 4. 自动列宽
                for (int i = 0; i < properties.Count; i++)
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

        /// <summary>
        /// 判断当前是否MSTest单元测试环境(.NET Framework 4.8)
        /// </summary>
        public static bool IsRunningInMSTest()
        {
            // MSTest v2 核心程序集
          
            var a = AppDomain.CurrentDomain.GetAssemblies();
            
            return AppDomain.CurrentDomain.GetAssemblies()
                .Any(asm =>asm.FullName.Contains("MSTest"));
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

        public static Type GetTypeByTypeCode(this TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Empty:
                    return null;
                case TypeCode.Object:
                    return typeof(object);
                case TypeCode.DBNull:
                    return typeof(DBNull);
                case TypeCode.Boolean:
                    return typeof(bool);
                case TypeCode.Char:
                    return typeof(char);
                case TypeCode.SByte:
                    return typeof(sbyte);
                case TypeCode.Byte:
                    return typeof(byte);
                case TypeCode.Int16:
                    return typeof(short);
                case TypeCode.UInt16:
                    return typeof(ushort);
                case TypeCode.Int32:
                    return typeof(int);
                case TypeCode.UInt32:
                    return typeof(uint);
                case TypeCode.Int64:
                    return typeof(long);
                case TypeCode.UInt64:
                    return typeof(ulong);
                case TypeCode.Single:
                    return typeof(float);
                case TypeCode.Double:
                    return typeof(double);
                case TypeCode.Decimal:
                    return typeof(decimal);
                case TypeCode.DateTime:
                    return typeof(DateTime);
                case TypeCode.String:
                    return typeof(string);
                default:
                    throw new NotSupportedException($"未支持的TypeCode：{code}");
            }
        }

        public static Result<object> SugarColumnReflectAssign(
            Result<List<DevPlcPointDto>> readValue,
            Type dataModelType
        )
        {
            object t = Activator.CreateInstance(dataModelType);
            var props = dataModelType.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                var attr = prop.GetCustomAttribute<SugarColumn>();
                if (attr?.ColumnDescription == null)
                {
                    continue;
                }
                try
                {
                    var readInfo = readValue.Data.FirstOrDefault(x =>
                        x.PointName == attr.ColumnDescription
                    );
                    if (readInfo == null)
                    {
                        if (_dfStringtype == prop.PropertyType)
                        {
                            prop.SetValue(t, "");
                        }
                    }
                    else
                    {
                        if (readInfo.Length == 1)
                        {
                            if (readInfo.ReadFormula == null || readInfo.ReadFormula.Length == 0)
                            {
                                prop.SetValue(t, readInfo.Value[0].ToString());
                            }
                            else
                            {
                                var val = readInfo.ReadFormula.StringCompute(
                                    readInfo.Value[0].ToString()
                                );
                                prop.SetValue(t, val.ToString());
                            }
                        }
                        else
                        {
                            if (readInfo.DataType == TypeCode.String)
                            {
                                var val = string.Concat(readInfo.Value);
                                prop.SetValue(t, val);
                            }
                            else
                            {
                                prop.SetValue(t, readInfo.Value.ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Result<object>.Fail($"反射数据失败:{ex.Message}");
                }
            }

            return Result<object>.Success(t);
        }

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

        /// <soure>
        /// 大端模式 Byte数组 转 short (PLC默认大端)
        /// </soure>
        public static short ToInt16(this byte[] buffer, int startIndex)
        {
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(buffer, startIndex, 2);
            return BitConverter.ToInt16(buffer, startIndex);
        }

        /// <soure>
        /// 大端模式 Byte数组 转 int
        /// </soure>
        public static int ToInt32(this byte[] buffer, int startIndex)
        {
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(buffer, startIndex, 4);
            return BitConverter.ToInt32(buffer, startIndex);
        }

        /// <soure>
        /// 大端模式 Byte数组 转 float
        /// </soure>
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

        public static byte[] RemoveStartBytes(this byte[] source, int removeCount)
        {
            if (source == null)
                return Array.Empty<byte>();

            // 移除数量≥数组长度，直接返回空
            if (removeCount >= source.Length)
                return Array.Empty<byte>();

            int newLen = source.Length - removeCount;
            byte[] result = new byte[newLen];
            Array.Copy(source, removeCount, result, 0, newLen);
            return result;
        }
        public static string BytesToAscii(this byte[] data, int length)
        {
            if (data == null || length <= 0)
                return string.Empty;

            // ASCII编码
            Encoding asciiEnc = Encoding.ASCII;
            return asciiEnc.GetString(data, 0, length);
        }
        /// <soure>
        /// 从字节数组解析出【数组类型】
        /// </soure>
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

        public static bool[] ByteToBits(this byte value)
        {
            bool[] bits = new bool[8];

            for (int i = 0; i < 8; i++)
            {
                // 依次取出第 0~7 位
                bits[i] = (value & (1 << i)) != 0;
            }

            return bits;
        }

        /// <soure>
        /// 通用数组解析（自动步进字节）
        /// </soure>
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
                    return buffer[startIndex].ByteToBits()[0];
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
                    return ConvertString(new byte[] { buffer[startIndex] });
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

            return sjisEncoding.GetString(bytes.Take(length).ToArray());
        }

        /// <soure>
        /// PLC大端 Double
        /// </soure>
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

        /// <soure>
        /// 获取枚举描述
        /// </soure>
        public static string GetDescription(this Enum enumValue)
        {
            if (enumValue == null)
                return string.Empty;

            FieldInfo field = enumValue.GetType().GetField(enumValue.ToString());
            var descAttr = field.GetCustomAttribute<DescriptionAttribute>();
            return descAttr?.Description ?? enumValue.ToString();
        }

        /// <soure>
        /// 解析位域枚举：根据int位值，获取所有激活的枚举项信息（泛型通用版）
        /// </soure>
        /// <typeparam name="TEnum">位枚举类型</typeparam>
        /// <param name="bitValue">原始整型位值</param>
        /// <returns>位索引、枚举名称、描述</returns>
        public static List<(int BitIndex, string EnumName, string Description)> ParseBitEnum<TEnum>(
            this int bitValue
        )
            where TEnum : Enum // 约束为枚举类型
        {
            var result = new List<(int BitIndex, string EnumName, string Description)>();
            var enumType = typeof(TEnum);

            foreach (var item in Enum.GetValues(enumType).Cast<TEnum>())
            {
                int enumVal = Convert.ToInt32(item);
                // 按位与判断当前位是否激活
                if ((bitValue & enumVal) == 0)
                    continue;

                // 计算当前位所在索引
                int bitIndex = 0;
                int temp = enumVal;
                while ((temp >>= 1) > 0)
                    bitIndex++;

                // 获取枚举描述
                string desc = GetDescription(item);
                result.Add((bitIndex, item.ToString(), desc));
            }

            return result;
        }

        static object GetValue(object obj, string fieldName)
        {
            if (obj == null)
                return null;

            var prop = obj.GetType().GetProperty(fieldName);
            return prop?.CanRead == true ? prop.GetValue(obj) : null;
        }

        public static void ItemToSoure(
            object soure,
            List<string> ignoreFields = null,
            params object[] sourceItem
        )
        {
            if (ignoreFields == null)
            {
                ignoreFields = new List<string>();
            }

            var summaryProperties = soure
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.IsDefined(typeof(SugarColumn), true))
                .ToList();

            foreach (var prop in summaryProperties)
            {
                var fieldName = prop.Name;

                if (
                    ignoreFields.Any(ig => ig.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                )
                    continue;
                foreach (var item in sourceItem)
                {
                    object value = GetValue(item, fieldName);
                    if (value == null)
                        continue;
                    else
                    {
                        prop.SetValue(soure, value);
                        break;
                    }
                }
            }
        }
    }
}
