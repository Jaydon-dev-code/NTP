using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Enum
{
    public enum Assembly_6Factory6_1Line_PassCodeEnum
    {
        [Description("000 PASS")]
        PASS_000 = 0b0000000000000001, // Bit0  1

        [Description("210 IN PASS")]
        PASS_210_IN = 0b0000000000000010, // Bit1  2

        [Description("220 OUT PASS")]
        PASS_220_OUT = 0b0000000000000100, // Bit2  4

        [Description("230 PASS")]
        PASS_230 = 0b0000000000001000, // Bit3  8

        [Description("360 IN PASS")]
        PASS_360_IN = 0b0000000000010000, // Bit4  16

        [Description("3B0 OUT PASS")]
        PASS_3B0_OUT = 0b0000000000100000, // Bit5  32

        [Description("400 IN PASS")]
        PASS_400_IN = 0b0000000001000000, // Bit6  64

        [Description("400A IN PASS")]
        PASS_400A_IN = 0b0000000010000000, // Bit7  128

        [Description("400A OUT PASS")]
        PASS_400A_OUT = 0b0000000100000000, // Bit8  256

        [Description("400B IN PASS")]
        PASS_400B_IN = 0b0000001000000000, // Bit9  512

        [Description("400B OUT PASS")]
        PASS_400B_OUT = 0b0000010000000000, // Bit10 1024

        [Description("510 PASS")]
        PASS_510 = 0b0000100000000000, // Bit11 2048

        [Description("520 PASS")]
        PASS_520 = 0b0001000000000000, // Bit12 4096

        [Description("620 PASS")]
        PASS_620 = 0b0010000000000000, // Bit13 8192

        [Description("630 PASS")]
        PASS_630 = 0b0100000000000000, // Bit14 16384

        [Description("640 PASS")]
        PASS_640 = 0b1000000000000000, // Bit15 32768

        [Description("710 PASS")]
        PASS_710 = 0b10000000000000000, // Bit16 65536

        [Description("720 PASS")]
        PASS_720 = 0b100000000000000000, // Bit17 131072

        [Description("730 PASS")]
        PASS_730 = 0b1000000000000000000, // Bit18 262144

        [Description("810 PASS")]
        PASS_810 = 0b10000000000000000000, // Bit19 524288
    }
}
