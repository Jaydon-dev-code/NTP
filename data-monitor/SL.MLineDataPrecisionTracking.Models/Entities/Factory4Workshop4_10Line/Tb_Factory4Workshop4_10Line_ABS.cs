using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities.Base;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;
using SqlSugar.DbConvert;

namespace SL.MLineDataPrecisionTracking.Models.Entitss.Factory4Workshop4_10Line
{
    /// <summary>
    /// abs
    /// </summary>
    [SugarTable("Factory4Workshop4_10Line_ABS")]
    public class Tb_Factory4Workshop4_10Line_ABS : Factory4Workshop4_10LineBase
    {
        /// <summary>
        /// 压紧结果
        /// </summary>
        [SugarColumn(
            ColumnDescription = "ABS压紧结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert)
        )]
        public ResultEnum? ABSPressDownResult { get; set; }

        [SugarColumn(ColumnDescription = "ABS压紧时间")]
        public DateTime? ABSPressDownTime { get; set; }

        [SugarColumn(
            ColumnDescription = "ABS检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert)
        )]
        public ResultEnum? ABSCheckResult { get; set; }

        [SugarColumn(ColumnDescription = "ABS检测时间")]
        public DateTime? ABSCheckTime { get; set; }
    }
}
