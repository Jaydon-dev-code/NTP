using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Enum.Mc1E
{
    public enum Mc1EFunctionCodeEnum : byte
    {
        /// <summary>
        /// 批量位读取
        /// </summary>
        BatchBitRead = 0x00,

        /// <summary>
        /// 批量字读取
        /// </summary>
        BatchWordRead = 0x01,

        /// <summary>
        /// 批量位写入
        /// </summary>
        BatchBitWrite = 0x02,

        /// <summary>
        /// 批量字写入
        /// </summary>
        BatchWordWrite = 0x03,

        /// <summary>
        /// 随机位写入
        /// </summary>
        RandomBitWrite = 0x04,

        /// <summary>
        /// 随机字写入
        /// </summary>
        RandomWordWrite = 0x05,
    }
}
