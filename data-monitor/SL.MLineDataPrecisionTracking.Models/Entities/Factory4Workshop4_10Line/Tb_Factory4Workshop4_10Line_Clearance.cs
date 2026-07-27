using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities.Base;
using SL.MLineDataPrecisionTracking.Models.Entitss.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;
using SqlSugar.DbConvert;

namespace SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line
{
    /// <summary>
    /// 游隙
    /// </summary>
    [SugarTable("Factory4Workshop4_10Line_Clearance")]
    public class Tb_Factory4Workshop4_10Line_Clearance : Factory4Workshop4_10LineBase
    {
        [SugarColumn(
            ColumnDescription = "游隙1工位检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert)
        )]
        public ResultEnum? Clearance1Result { get; set; }

        [SugarColumn(
            ColumnDescription = "游隙2工位检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert)
        )]
        public ResultEnum? Clearance2Result { get; set; }

        [SugarColumn(ColumnDescription = "游隙1工位采集时间")]
        public DateTime? Clearance1Time { get; set; }

        [SugarColumn(ColumnDescription = "游隙2工位采集时间")]
        public DateTime? Clearance2Time { get; set; }

        [SugarColumn(ColumnDescription = "正间隙")]
        public string? PositiveGap { get; set; }

        [SugarColumn(ColumnDescription = "下负荷")]
        public string? LowerLoad { get; set; }

        [SugarColumn(ColumnDescription = "上负荷")]
        public string? UpperLoad { get; set; }

        [SugarColumn(ColumnDescription = "偏移量")]
        public string? Offset { get; set; }

        [SugarColumn(ColumnDescription = "压入前")]
        public string? BeforePressIn { get; set; }

        [SugarColumn(ColumnDescription = "压入后")]
        public string? AfterPressIn { get; set; }

        [SugarColumn(ColumnDescription = "间隙")]
        public string? Gap { get; set; }
    }
}
