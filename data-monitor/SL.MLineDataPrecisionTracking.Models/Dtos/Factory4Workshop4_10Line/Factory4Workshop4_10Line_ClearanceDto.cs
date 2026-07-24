using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;
using SqlSugar.DbConvert;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line
{
    public class Factory4Workshop4_10Line_ClearanceDto
    {
        public DateTime RecordTime { get; set; }
        public string SN { get; set; }

        [Description("游隙检测结果")]
        public ResultEnum ClearanceResult { get; set; }

        [Description("正间隙")]
        public string PositiveGap { get; set; }

        [Description("下负荷")]
        public string LowerLoad { get; set; }

        [Description("上负荷")]
        public string UpperLoad { get; set; }

        [Description("偏移量")]
        public string Offset { get; set; }

        [Description("压入前")]
        public string BeforePressIn { get; set; }

        [Description("压入后")]
        public string AfterPressIn { get; set; }

        [Description("间隙")]
        public string Gap { get; set; }
    }
}
