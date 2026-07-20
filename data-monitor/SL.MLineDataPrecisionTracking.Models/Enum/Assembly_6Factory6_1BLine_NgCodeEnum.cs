using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Enum
{
    public enum Assembly_6Factory6_1BLine_NgCodeEnum
    {
        [Description("900 1st 正间隙 N.G")]
        NG_900_1st_Clearance = 1,

        [Description("900 2nd TQ N.G")]
        NG_900_2nd_TQ = 2,

        [Description("900 2nd 移動量 N.G")]
        NG_900_2nd_MoveAmount = 3,

        [Description("900 2nd 间隙 N.G")]
        NG_900_2nd_Clearance = 4,

        [Description("1010内密封圈压入异常")]
        NG_1010_InnerSealPress = 5,

        [Description("1020 内密封圈扁平度")]
        NG_1020_InnerSealFlatness = 6,

        [Description("1110 BROACH #1 N.G")]
        NG_1110_Broach1 = 7,

        [Description("1120 BROACH #2 N.G")]
        NG_1120_Broach2 = 8,

        [Description("1150 GO/NO CHECK N.G")]
        NG_1150_GoNoCheck = 9,

        [Description("1200 径向跳动检测 N.G")]
        NG_1200_RadialRunout = 10,

        [Description("1200 平面跳动检测 N.G")]
        NG_1200_FaceRunout = 11,

        [Description("1200 高度 N.G")]
        NG_1200_Height = 12,

        [Description("1300 下部 振动 检查 N.G")]
        NG_1300_LowerVibration = 13,

        [Description("1300 上部 振动 检查 N.G")]
        NG_1300_UpperVibration = 14,

        [Description("1400 ABS扭矩测试 N.G")]
        NG_1400_AbsTorqueTest = 15,

        [Description("1400 ABS测试 N.G")]
        NG_1400_AbsTest = 16,

        [Description("成品码扫码 N.G")]
        NG_成品码_扫码 = 17,

        [Description("作业者 N.G")]
        NG_Worker = 99,
    }
}
