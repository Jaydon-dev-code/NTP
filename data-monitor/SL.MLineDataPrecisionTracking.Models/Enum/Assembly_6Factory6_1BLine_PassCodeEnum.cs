using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Enum
{
    public enum Assembly_6Factory6_1BLine_PassCodeEnum
    {
        [Description("870 PASS")]
        PASS_870 = 0b0000000000000001, // Bit0

        [Description("900 IN PASS")]
        PASS_900_IN = 0b0000000000000010, // Bit1

        [Description("900 OUT PASS")]
        PASS_900_OUT = 0b0000000000000100, // Bit2

        [Description("1010 PASS")]
        PASS_1010 = 0b0000000000001000, // Bit3

        [Description("1020 PASS")]
        PASS_1020 = 0b0000000000010000, // Bit4

        [Description("1100 PASS")]
        PASS_1100 = 0b0000000000100000, // Bit5

        [Description("1110 PASS")]
        PASS_1110 = 0b0000000001000000, // Bit6

        [Description("1120 PASS")]
        PASS_1120 = 0b0000000010000000, // Bit7

        [Description("1150 PASS")]
        PASS_1150 = 0b0000000100000000, // Bit8

        [Description("1200 PASS")]
        PASS_1200 = 0b0000001000000000, // Bit9

        [Description("1300 PASS")]
        PASS_1300 = 0b0000010000000000, // Bit10

        [Description("1400 PASS")]
        PASS_1400 = 0b0000100000000000, // Bit11

        [Description("1500 PASS")]
        PASS_1500 = 0b0001000000000000, // Bit12

        [Description("1600 PASS")]
        PASS_1600 = 0b0010000000000000, // Bit13
    }
}
