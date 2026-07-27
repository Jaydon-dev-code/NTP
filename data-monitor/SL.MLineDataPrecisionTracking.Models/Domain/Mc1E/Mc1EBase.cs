using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using McpXLib.Enums;
using McpXLib.Exceptions;
using SL.MLineDataPrecisionTracking.Models.Enum.Mc1E;

namespace SL.MLineDataPrecisionTracking.Models.Domain.Mc1E
{
    public class Mc1EBase
    {
        /// <summary>功能码（BatchWordRead=0x01）</summary>
        public Mc1EFunctionCodeEnum FunctionCode { get; set; }

        /// <summary>PLC 编号</summary>
        public byte PlcNo { get; set; } = 0xFF;

        /// <summary>监视定时器（2 字节，单位：250ms）</summary>
        public byte[] OutTime { get; set; } = new byte[2] { 0x00, 0x00 };

        /// <summary>
        /// 起始地址（4 字节）
        /// <para>字节 0-2 = 地址值（小端），字节 3 = 设备码</para>
        /// </summary>
        public byte[] StartAddre { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// 设备码（2 字节）
        /// <para>字节 0 = Prefix 枚举值，字节 1 = 0x00</para>
        /// </summary>
        public byte[] McPrefix { get; set; } = new byte[2] { 0x00, 0x00 };

        /// <summary>
        /// 地址 → 4 字节：地址值（小端 3 字节）+ 设备码（1 字节）
        /// <para>十进制设备（D/M/L/S/Z/R 等）：Parse 为十进制 uint</para>
        /// <para>十六进制设备（X/Y/B/W/SB/SW/DX/DY）：Parse 为十六进制 uint</para>
        /// </summary>
        protected byte[] ToByteAddress(Prefix prefix, string address)
        {
            if (!ValidateAddress(prefix, address))
                throw new DeviceAddressException($"{prefix}{address} is invalid.");

            byte[] bytes;
            if (!IsHexDevice(prefix))
            {
                bytes = BitConverter.GetBytes(uint.Parse(address));
            
                return bytes;
            }

            // 1. 把录入的数字转为字符串 "10"
            string hexStr = address.ToString();
            // 2. 将字符串按十六进制解析

            bytes = BitConverter.GetBytes(Convert.ToUInt32(hexStr, 16));
            return bytes;
        }

        bool ValidateAddress(Prefix prefix, string address)
        {
            if (!IsHexDevice(prefix))
                return uint.TryParse(address, out _);
            return Regex.IsMatch(address, "^[0-9A-Fa-f]+$");
        }

        protected bool IsHexDevice(Prefix prefix)
        {
            if (
                prefix != Prefix.X
                && prefix != Prefix.Y
                && prefix != Prefix.B
                && prefix != Prefix.W
                && prefix != Prefix.SB
                && prefix != Prefix.SW
                && prefix != Prefix.DX
            )
                return prefix == Prefix.DY;
            return true;
        }

        protected byte[] GetCode(Prefix prefix, bool isLittleEnd = true)
        {
            byte[] buf = new byte[2] { 0x00, 0x00 };
            switch (prefix)
            {
                case Prefix.X:
                    buf = new byte[2] { 0x58, 0x20 };
                    break;
                case Prefix.Y:
                    buf = new byte[2] { 0x59, 0x20 };
                    break;
                case Prefix.M:
                    buf = new byte[2] { 0x4D, 0x20 };
                    break;
                case Prefix.L:
                    break;
                case Prefix.F:

                    break;
                case Prefix.V:
                    break;
                case Prefix.B:
                    break;
                case Prefix.D:
                    buf = new byte[2] { 0x44, 0x20 };
                    break;
                case Prefix.W:
                    buf = new byte[2] { 0x57, 0x20 };
                    break;
                case Prefix.TS:
                    break;
                case Prefix.TC:
                    break;
                case Prefix.TN:
                    break;
                case Prefix.SS:
                    break;
                case Prefix.SC:
                    break;
                case Prefix.SN:
                    break;
                case Prefix.CS:
                    break;
                case Prefix.CC:
                    break;
                case Prefix.CN:
                    break;
                case Prefix.SB:
                    break;
                case Prefix.SW:
                    break;
                case Prefix.S:
                    break;
                case Prefix.DX:
                    break;
                case Prefix.DY:
                    break;
                case Prefix.SM:
                    break;
                case Prefix.SD:
                    break;
                case Prefix.Z:
                    break;
                case Prefix.R:
                    break;
                case Prefix.ZR:
                    break;
                default:
                    break;
            }
            if (isLittleEnd)
            {
                Array.Reverse(buf);
            }

            return buf;
        }
    }
}
