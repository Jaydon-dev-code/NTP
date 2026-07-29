using System;
using System.Linq;
using System.Text.RegularExpressions;
using McpXLib.Enums;
using McpXLib.Exceptions;
using SL.MLineDataPrecisionTracking.Models.Enum.Mc1E;

namespace SL.MLineDataPrecisionTracking.Models.Domain.Mc1E
{
    /// <summary>
    /// MC QNA 1E 协议读取帧模型（二进制格式）
    /// <para>
    /// 帧结构：
    ///   请求：[功能码(1B)] [PLC编号(1B)] [超时(2B)] [起始地址(4B)] [设备码(2B)] [读取字数(2B)]
    ///   响应：[完成码(1B)] [数据(N*2B)]
    /// </para>
    /// <para>字节序：小端（Little-Endian），与主机一致。</para>
    /// </summary>
    public class Mc1ERead : Mc1EBase
    {
        /// <summary>读取字数（2 字节）</summary>
        public byte[] ReadLen { get; set; } = new byte[2] { 0x00, 0x00 };

        /// <summary>
        /// 构建 1E 二进制读帧
        /// </summary>
        /// <param name="prefix">设备前缀</param>
        /// <param name="address">起始地址（十进制 int）</param>
        /// <param name="wordCount">读取字数</param>
        /// <param name="outTime">监视定时器（ms）</param>
        /// <returns>完整的 1E 读请求帧</returns>
        public byte[] ToByte(Prefix prefix, int address, int wordCount, TypeCode dataType, int outTime = 100)
        {
            this.FunctionCode = dataType==TypeCode.Boolean
                    ? Mc1EFunctionCodeEnum.BatchBitRead:Mc1EFunctionCodeEnum.BatchWordRead;

            this.PlcNo = 0xFF;

            this.OutTime = BitConverter.GetBytes((ushort)outTime);

            this.StartAddre = ToByteAddress(prefix, IsHexDevice(prefix) ? address.ToString("X") : address.ToString());

            this.McPrefix = GetCode(prefix);

            ReadLen = BitConverter.GetBytes((ushort)wordCount);

            return BuildFrame();
        }

        /// <summary>将所有字段拼装为完整帧字节数组</summary>
        byte[] BuildFrame()
        {
            var bytes = new System.Collections.Generic.List<byte>();
            bytes.Add((byte)FunctionCode);
            bytes.Add(PlcNo);
            bytes.AddRange(OutTime);
            bytes.AddRange(StartAddre);
            bytes.AddRange(McPrefix);
            bytes.AddRange(ReadLen);
            return bytes.ToArray();
        }

      
      
    }
}
