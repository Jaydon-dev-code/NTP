using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using McpXLib.Enums;
using McpXLib.Exceptions;
using SL.MLineDataPrecisionTracking.Models.Enum.Mc1E;

namespace SL.MLineDataPrecisionTracking.Models.Domain.Mc1E
{
    /// <summary>
    /// MC QNA 1E 协议写入帧模型（二进制格式）
    /// <para>
    /// 帧结构：
    /// [功能码(1B)] [PLC编号(1B)] [超时(2B)] [起始地址(4B)] [设备码(2B)] [写入字数(2B)] [数据(NB)]
    /// </para>
    /// </summary>
    public class Mc1EWirte : Mc1EBase
    {
        /// <summary>
        /// 写入长度（2 字节）
        /// <para>字写入时为字数，位写入时为位数</para>
        /// </summary>
        public byte[] WirteLen { get; set; } = new byte[2] { 0x00, 0x00 };

        /// <summary>写入数据</summary>
        public byte[] WirteValue { get; set; }

        /// <summary>
        /// 单值写入 → 构建 1E 二进制帧（泛型版本）
        /// </summary>
        public byte[] ToByte<T>(Prefix prefix, string address, T val, int outTime = 5000)
        {
            return ToByte(prefix, address, new[] { val }, outTime);
        }

        /// <summary>
        /// 多值写入 → 构建 1E 二进制帧（泛型版本）
        /// <para>自动识别数据类型，委托给 ToByte(Prefix, string, TypeCode, List&lt;object&gt;, int)。</para>
        /// </summary>
        public byte[] ToByte<T>(Prefix prefix, string address, T[] val, int outTime = 10000)
        {
            TypeCode typeCode = Type.GetTypeCode(typeof(T));
            var values = val?.Select(v => (object)v).ToList() ?? new List<object>();
            return ToByte(prefix, address, typeCode, values, outTime);
        }

        /// <summary>
        /// 写入 → 构建 1E 二进制帧（核心版本）
        /// <para>根据 TypeCode 自动分发：Boolean 走位写入(BatchBitWrite)，String 走 Shift-JIS 编码逐字写入，其余走字写入(BatchWordWrite)。</para>
        /// </summary>
        /// <param name="prefix">设备前缀</param>
        /// <param name="address">起始地址（十进制或十六进制字符串）</param>
        /// <param name="typeCode">数据类型</param>
        /// <param name="values">值列表</param>
        /// <param name="outTime">监视定时器（ms，转换为 250ms 单位）</param>
        /// <returns>完整的 1E 写帧字节数组</returns>
        public byte[] ToByte(
            Prefix prefix,
            string address,
            TypeCode typeCode,
            List<object> values,
            int outTime = 5000
        )
        {
            this.FunctionCode =
                typeCode == TypeCode.Boolean
                    ? Mc1EFunctionCodeEnum.BatchBitWrite
                    : Mc1EFunctionCodeEnum.BatchWordWrite;

            this.PlcNo = 0xFF;

            this.OutTime = BitConverter.GetBytes((ushort)outTime);

            this.StartAddre = ToByteAddress(prefix, address);
       
            this.McPrefix = GetCode(prefix);

            if (typeCode == TypeCode.String)
            {
                string fullStr = string.Concat(
                    values?.Select(v => v?.ToString() ?? "") ?? Array.Empty<string>()
                );
                //寄存器最小位为 2个 byte

                byte[] encoded = Encoding.GetEncoding("shift_jis").GetBytes(fullStr);
                byte[] writeBuf;
                if (encoded.Length % 2 != 0)
                {
                    writeBuf = new byte[encoded.Length + 1];
                    Array.Copy(encoded, writeBuf, encoded.Length);
                    writeBuf[writeBuf.Length - 1] = 0x00;
                }
                else
                {
                    writeBuf = encoded;
                }
                // 4. 计算字数（2字节=1个字寄存器）
                ushort wordCount = (ushort)(writeBuf.Length / 2);
                byte[] writeLen = BitConverter.GetBytes(wordCount);

                // 赋值输出变量
                this.WirteValue = writeBuf;
                this.WirteLen = writeLen;
            }
            else
            {
                this.WirteValue = ValuesToBytes(values, typeCode);
                int wordCount = (values?.Count ?? 0) * GetTypeOfShortOffset(typeCode);
                this.WirteLen = BitConverter.GetBytes((ushort)wordCount);
            }

            return ValueToByteToArray();
        }

        /// <summary>将所有字段拼装为完整帧字节数组</summary>
        byte[] ValueToByteToArray()
        {
            var bytes = new List<byte>();
            bytes.Add((byte)FunctionCode);
            bytes.Add(PlcNo);
            bytes.AddRange(OutTime);
            bytes.AddRange(StartAddre);
            bytes.AddRange(McPrefix);
            bytes.AddRange(WirteLen);
            if (WirteValue != null)
                bytes.AddRange(WirteValue);
            return bytes.ToArray();
        }

        /// <summary>值列表 → 小端字节数组（非 String 类型用）</summary>
        static byte[] ValuesToBytes(List<object> values, TypeCode typeCode)
        {
            using var ms = new System.IO.MemoryStream();
            foreach (object val in values)
            {
                byte[] bytes = ValueToBytes(val, typeCode);
                if (bytes != null)
                    ms.Write(bytes, 0, bytes.Length);
            }
            return ms.ToArray();
        }

        /// <summary>单值 → 小端字节数组</summary>
        static byte[] ValueToBytes(object val, TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return BitConverter.GetBytes(Convert.ToBoolean(val) ? (short)1 : (short)0);
                case TypeCode.Int16:
                    return BitConverter.GetBytes(Convert.ToInt16(val));
                case TypeCode.UInt16:
                    return BitConverter.GetBytes(Convert.ToUInt16(val));
                case TypeCode.Int32:
                    return BitConverter.GetBytes(Convert.ToInt32(val));
                case TypeCode.UInt32:
                    return BitConverter.GetBytes(Convert.ToUInt32(val));
                case TypeCode.Single:
                    return BitConverter.GetBytes(Convert.ToSingle(val));
                case TypeCode.Double:
                    return BitConverter.GetBytes(Convert.ToDouble(val));
                default:
                    throw new NotSupportedException($"1E 写入不支持的数据类型: {typeCode}");
            }
        }

        /// <summary>获取数据类型对应的字偏移量</summary>
        int GetTypeOfShortOffset(TypeCode type)
        {
            switch (type)
            {
                case TypeCode.Boolean:
                case TypeCode.String:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    return 1;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Single:
                    return 2;
                case TypeCode.Double:
                case TypeCode.Int64:
                    return 4;
                default:
                    throw new NotSupportedException($"不支持的数据类型: {type}");
            }
        }
    }
}
