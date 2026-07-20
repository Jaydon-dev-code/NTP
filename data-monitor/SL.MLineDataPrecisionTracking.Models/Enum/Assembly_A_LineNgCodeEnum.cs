using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Enum
{
    /// <summary>
    /// 6-3装配线NG代码枚举 采集的数字 代表对应的NG
    /// </summary>
    public enum Assembly_A_LineNgCodeEnum
    {
        None = 0,

        [Description("460 HUB侧 BALL MATCH N.G")]
        NG_460 = 1,

        [Description("4B0 内轮侧 BALL MATCH N.G")]
        NG_4B0 = 2,

        [Description("610 HUB侧 BALL CHECK N.G")]
        NG_610 = 3,

        [Description("620 HUB侧 GREASE 注入 N.G")]
        NG_620 = 4,

        [Description("640 OPEN SEAL 压入 N.G")]
        NG_640 = 5,

        [Description("650 OPEN SEAL 扁平度 N.G")]
        NG_650 = 6,

        [Description("660 OPEN SEAL GREASE 涂抹 N.G")]
        NG_660 = 7,

        [Description("680 COVER RING 压入 N.G")]
        NG_680 = 8,

        [Description("690 COVER RING 扁平度 N.G")]
        NG_690 = 9,

        [Description("720 内轮侧 BALL CHECK N.G")]
        NG_720 = 10,

        [Description("730 内轮侧 GREASE N.G")]
        NG_730 = 11,

        [Description("810 INNER RING 压入 N.G")]
        NG_810 = 12,

        [Description("内法兰扫码 N.G")]
        NG_内法兰_扫码 = 13,

        [Description("外法兰扫码 N.G")]
        NG_外法兰_扫码 = 14,

        [Description("作业者 N.G")]
        NG_Artificial = 99,
    }
}
