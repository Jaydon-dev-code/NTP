using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Enum
{
    public enum Assembly_A_LinePassCodeEnum
    {
        None = 0,
        [Description("000 PASS")]
        PASS_000 = 0b0000000000000001,  // Bit0  1
        [Description("310 IN PASS")]
        PASS_IN_310 = 0b0000000000000010,  // Bit1  2
        [Description("320 OUT PASS")]
        PASS_OUT_320 = 0b0000000000000100,  // Bit2  4
        [Description("300 PASS")]
        PASS_300 = 0b0000000000001000,  // Bit3  8
        [Description("460 IN PASS")]
        PASS_IN_460 = 0b0000000000010000,  // Bit4  16
        [Description("4B0 OUT PASS")]
        PASS_OUT_4B0 = 0b0000000000100000,  // Bit5  32
        [Description("500 IN PASS")]
        PASS_IN_500 = 0b0000000001000000,  // Bit6  64
        [Description("500A IN PASS")]
        PASS_IN_500A = 0b0000000010000000,  // Bit7  128
        [Description("500A OUT PASS")]
        PASS_OUT_500A = 0b0000000100000000,  // Bit8  256
        [Description("500B IN PASS")]
        PASS_IN_500B = 0b0000001000000000,  // Bit9  512
        [Description("500B OUT PASS")]
        PASS_OUT_500B = 0b0000010000000000,  // Bit10 1024
        [Description("610 PASS")]
        PASS_610 = 0b0000100000000000,  // Bit11 2048
        [Description("620 PASS")]
        PASS_620 = 0b0001000000000000,  // Bit12 4096
        [Description("640 PASS")]
        PASS_640 = 0b0010000000000000,  // Bit13 8192
        [Description("650 PASS")]
        PASS_650 = 0b0100000000000000,  // Bit14 16384
        [Description("660 PASS")]
        PASS_660 = 0b1000000000000000,  // Bit15 32768
        [Description("680 PASS")]
        PASS_680 = 0b0000000000000001 << 16, // Bit16
        [Description("690 PASS")]
        PASS_690 = 0b0000000000000001 << 17, // Bit17
        [Description("710 PASS")]
        PASS_710 = 0b0000000000000001 << 18, // Bit18
        [Description("720 PASS")]
        PASS_720 = 0b0000000000000001 << 19, // Bit19
        [Description("730 PASS")]
        PASS_730 = 0b0000000000000001 << 20, // Bit20
        [Description("810 PASS")]
        PASS_810 = 0b0000000000000001 << 21  // Bit21
    }
}
