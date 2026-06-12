using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Enum
{
    public enum Assembly_B_LineNgCodeEnum
    {
        None = 0,
        [Description("900 1st 正间隙 N.G")]
        NG_900_1st_Gap = 1,
        [Description("1010 成型机 前 高度 N.G")]
        NG_1010_Front_Height = 2,
        [Description("1020 成型机 LOAD N.G")]
        NG_1020_Load = 3,
        [Description("1020 成型机 SCALE N.G")]
        NG_1020_Scale = 4,
        [Description("1030 裂缝 N.G")]
        NG_1030_Crack = 5,
        [Description("1040 成型机 后 高度 N.G")]
        NG_1040_Rear_Height = 6,
        [Description("1040 成型机 后 形象 高度 N.G")]
        NG_1040_Rear_Figure_Height = 7,
        [Description("1040 成型机 后 外径 N.G")]
        NG_1040_Rear_OuterDiameter = 8,
        [Description("1110 INBOARD SEAL 压入 N.G")]
        NG_1110_InboardSeal_Press = 9,
        [Description("1120 INBOARD SEAL 扁平度 N.G")]
        NG_1120_InboardSeal_Flatness = 10,
        [Description("1310 SENSOR RING 压入 N.G")]
        NG_1310_SensorRing_Press = 11,
        [Description("1320 SENSOR RING 扁平度 N.G")]
        NG_1320_SensorRing_Flatness = 12,
        [Description("1400-A_1 N.G 排出")]
        NG_1400A1_Eject = 13,
        [Description("1400-A_1 自主检查 排出")]
        NG_1400A1_SelfCheck_Eject = 14,
        [Description("1400-A_2 N.G 排出")]
        NG_1400A2_Eject = 15,
        [Description("1400-A_2 自主检查 排出")]
        NG_1400A2_SelfCheck_Eject = 16,
        [Description("1400-B_1 N.G 排出")]
        NG_1400B1_Eject = 17,
        [Description("1400-B_1 自主检查 排出")]
        NG_1400B1_SelfCheck_Eject = 18,
        [Description("1400-B_2 N.G 排出")]
        NG_1400B2_Eject = 19,
        [Description("1400-B_2 自主检查 排出")]
        NG_1400B2_SelfCheck_Eject = 20,
        [Description("1510 直角 RUNOUT N.G")]
        NG_1510_RightAngle_Runout = 21,
        [Description("1520 平面 RUNOUT N.G")]
        NG_1520_Plane_Runout = 22,
        [Description("1600 下部 检查 N.G")]
        NG_1600_Lower_Check = 23,
        [Description("1600 上部 检查 N.G")]
        NG_1600_Upper_Check = 24,
        [Description("1710 ABS RING 压入 N.G")]
        NG_1710_AbsRing_Press = 25,
        [Description("1720 TORQUE TEST N.G")]
        NG_1720_Torque_Test = 26,
        [Description("1720 ABS TEST N.G")]
        NG_1720_Abs_Test = 27,
        [Description("900 2nd TQ N.G")]
        NG_900_2nd_TQ = 28,
        [Description("900 2nd 移動量 N.G")]
        NG_900_2nd_Movement = 29,
        [Description("900 2nd 间隙 N.G")]
        NG_900_2nd_Gap = 30,
        [Description("作业者 N.G")]
        NG_Artificial = 99
    }
}
