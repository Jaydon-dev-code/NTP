using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Expand
{
    public static class DataConvertExpand
    {
        private static readonly Encoding sjisEncoding;

        static DataTable _dataTable = new DataTable();
        static DataConvertExpand()
        {
#if !NETSTANDARD2_0
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            sjisEncoding = Encoding.GetEncoding("shift_jis");
        }

        public static object StringCompute(this string expression, params string[] param)
        {
            var format = string.Format(expression, param);
            return _dataTable.Compute(format, string.Empty);
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
                    return new List<object>()
                    {
                        string.Join(
                            "",
                            ParseArray(buffer, startIndex, length, GetTypeByteLength(type), type)
                        ),
                    };
                    ;
                default:
                    throw new NotSupportedException($"不支持解析类型数组: {type}");
            }
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
        /// 将输入byte数组中每个字节，拆分为高低4位，每1字节输出2字节
        /// 拆分规则：byte = [高4位][低4位]
        /// 输出顺序：低4位字节，高4位字节
        /// </summary>
        /// <param name="inputBytes">原始字节数组</param>
        /// <returns>拆分后的新byte数组</returns>
        public static byte[] SplitByteHighLow4Bit(this byte[] inputBytes, bool isInvert)
        {
            // 空值判断
            if (inputBytes == null || inputBytes.Length == 0)
                return Array.Empty<byte>();

            // 原数组1字节拆2字节，长度翻倍
            byte[] output = new byte[inputBytes.Length * 2];
            int outIndex = 0;

            foreach (byte b in inputBytes)
            {
                if (isInvert)
                {
                    // 提取低4位：& 0x0F
                    byte low4 = (byte)(b & 0x0F);
                    // 提取高4位：右移4位
                    byte high4 = (byte)((b >> 4) & 0x0F);

                    // 先存高4位，再存低4位
                    output[outIndex++] = high4;
                    output[outIndex++] = low4;
                }
                else
                {
                    // 提取低4位：& 0x0F
                    byte low4 = (byte)(b & 0x0F);
                    // 提取高4位：右移4位
                    byte high4 = (byte)((b >> 4) & 0x0F);

                    // 先存低4位，再存高4位
                    output[outIndex++] = low4;
                    output[outIndex++] = high4;
                }
            }

            return output;
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

        /// <summary>
        /// byte数组转bool数组（按bit顺序，低位在前）
        /// </summary>
        public static bool[] BytesToBools(this byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            bool[] result = new bool[buffer.Length * 8];
            int index = 0;

            foreach (byte b in buffer)
            {
                // 遍历当前byte的8个bit（0~7位）
                for (int bit = 0; bit < 8; bit++)
                {
                    // 取出第bit位
                    result[index++] = (b & (1 << bit)) != 0;
                }
            }
            return result;
        }

        /// <summary>
        /// bool数组转单个byte（最多8位）
        /// </summary>
        /// <param name="bools">布尔数组，长度不能超过8</param>
        /// <returns>合并后的byte</returns>
        public static byte BoolArrayToByte(bool[] bools)
        {
            if (bools == null)
                throw new ArgumentNullException(nameof(bools));
            if (bools.Length > 8)
                throw new ArgumentException("数组长度不能超过8，超出无法存入单个byte");

            byte result = 0;
            for (int i = 0; i < bools.Length; i++)
            {
                if (bools[i])
                {
                    // 将第i位置1
                    result |= (byte)(1 << i);
                }
            }
            return result;
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
        public static float ObjToFloat(this object thisValue)
        {
            float result = 0;
            if (thisValue == null)
            {
                return 0;
            }

            if (
                thisValue != null
                && thisValue != DBNull.Value
                && float.TryParse(thisValue.ToString(), out result)
            )
            {
                return result;
            }

            return result;
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
    }
}
