using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Enum
{
    public enum Assembly_B_LinePassCodeEnum
    {
        None = 0,
        [Description("870 PASS")]
        PASS_870 = 0b0000000000000000000000001,  // Bit0
        [Description("900 IN PASS")]
        PASS_900_IN = 0b0000000000000000000000010,  // Bit1
        [Description("900 OUT PASS")]
        PASS_900_OUT = 0b0000000000000000000000100,  // Bit2
        [Description("1010 PASS")]
        PASS_1010 = 0b0000000000000000000001000,  // Bit3
        [Description("1020 IN PASS")]
        PASS_1020_IN = 0b0000000000000000000010000,  // Bit4
        [Description("1020 OUT PASS")]
        PASS_1020_OUT = 0b0000000000000000000100000,  // Bit5
        [Description("1030 PASS")]
        PASS_1030 = 0b0000000000000000001000000,  // Bit6
        [Description("1040 PASS")]
        PASS_1040 = 0b0000000000000000010000000,  // Bit7
        [Description("1110 PASS")]
        PASS_1110 = 0b0000000000000000100000000,  // Bit8
        [Description("1120 PASS")]
        PASS_1120 = 0b0000000000000001000000000,  // Bit9
        [Description("1200 PASS")]
        PASS_1200 = 0b0000000000000010000000000,  // Bit10
        [Description("1310 PASS")]
        PASS_1310 = 0b0000000000000100000000000,  // Bit11
        [Description("1320 PASS")]
        PASS_1320 = 0b0000000000001000000000000,  // Bit12
        [Description("1400-A PASS")]
        PASS_1400_A = 0b0000000000010000000000000,  // Bit13
        [Description("1400-A_1PASS")]
        PASS_1400_A1 = 0b0000000000100000000000000,  // Bit14
        [Description("1400-A_2PASS")]
        PASS_1400_A2 = 0b0000000001000000000000000,  // Bit15
        [Description("1400-B PASS")]
        PASS_1400_B = 0b0000000010000000000000000,  // Bit16
        [Description("1400-B_1 PASS")]
        PASS_1400_B1 = 0b0000000100000000000000000,  // Bit17
        [Description("1400-B_2 PASS")]
        PASS_1400_B2 = 0b0000001000000000000000000,  // Bit18
        [Description("1510 PASS")]
        PASS_1510 = 0b0000010000000000000000000,  // Bit19
        [Description("1520 PASS")]
        PASS_1520 = 0b0000100000000000000000000,  // Bit20
        [Description("1600 PASS")]
        PASS_1600 = 0b0001000000000000000000000,  // Bit21
        [Description("1710 PASS")]
        PASS_1710 = 0b0010000000000000000000000,  // Bit22
        [Description("1720 PASS")]
        PASS_1720 = 0b0100000000000000000000000,  // Bit23
        [Description("1800 PASS")]
        PASS_1800 = 0b1000000000000000000000000,  // Bit24
    }
}
