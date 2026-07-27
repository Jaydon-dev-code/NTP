using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities.Base;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;
using SqlSugar.DbConvert;

namespace SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line
{
    [SugarIndex(
        "idx_Factory4Workshop4_10LineSummary_SN_Asc",
        nameof(Tb_Factory4Workshop4_10LineSummary.SN),
        OrderByType.Asc
    )]
    [SugarTable("Factory4Workshop4_10LineSummary")]
    public class Tb_Factory4Workshop4_10LineSummary : Factory4Workshop4_10LineBase
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

        [SugarColumn(
            ColumnDescription = "铆接检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert)
        )]
        public ResultEnum? RivetingResult { get; set; }

        [SugarColumn(ColumnDescription = "铆接时间")]
        public DateTime? RivetingTime { get; set; }

        [SugarColumn(
            ColumnDescription = "旋铆检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert)
        )]
        public ResultEnum? SpinRivetingResult { get; set; }

        [SugarColumn(ColumnDescription = "旋铆检测时间")]
        public DateTime? SpinRivetingTime { get; set; }

        [SugarColumn(ColumnDescription = "旋铆检测1高度")]
        public string? RivetingInspection1Height { get; set; }

        [SugarColumn(ColumnDescription = "旋铆检测2高度")]
        public string? RivetingInspection2Height { get; set; }

        [SugarColumn(ColumnDescription = "旋铆成型高度")]
        public string? SpiralRivetingFormingHeight { get; set; }

        [SugarColumn(
            ColumnDescription = "检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert)
        )]
        public ResultEnum? VibCrackResult { get; set; }

        [SugarColumn(ColumnDescription = "震动时间")]
        public DateTime? VibCrackTime { get; set; }

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
