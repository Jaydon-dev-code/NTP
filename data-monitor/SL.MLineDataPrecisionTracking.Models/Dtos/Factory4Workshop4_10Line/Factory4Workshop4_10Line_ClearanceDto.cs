using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line
{
    public class Factory4Workshop4_10Line_ClearanceDto
    {
        public DateTime RecordTime { get; set; }
        public string SN { get; set; }

        public ResultEnum ClearanceResult { get; set; }

        public string PositiveGap { get; set; }

        public string LowerLoad { get; set; }

        public string UpperLoad { get; set; }

        public string Offset { get; set; }

        public string BeforePressIn { get; set; }

        public string AfterPressIn { get; set; }

        public string Gap { get; set; }
    }
}
