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
        OrderByType.Desc,true
    )]
    [SugarTable("Factory4Workshop4_10LineSummary")]
    public class Tb_Factory4Workshop4_10LineSummary : Factory4Workshop4_10LineBase
    {
        [SugarColumn(
            ColumnDescription = "游隙1工位检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert),
            IsNullable = true
        )]
        public ResultEnum? Clearance1Result { get; set; }

        [SugarColumn(
            ColumnDescription = "游隙2工位检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert),
            IsNullable = true
        )]
        public ResultEnum? Clearance2Result { get; set; }

        [SugarColumn(ColumnDescription = "游隙1工位采集时间", IsNullable = true)]
        public DateTime? Clearance1Time { get; set; }

        [SugarColumn(ColumnDescription = "游隙2工位采集时间", IsNullable = true)]
        public DateTime? Clearance2Time { get; set; }

        [SugarColumn(ColumnDescription = "正间隙", IsNullable = true)]
        public string? PositiveGap { get; set; }

        [SugarColumn(ColumnDescription = "下负荷", IsNullable = true)]
        public string? LowerLoad { get; set; }

        [SugarColumn(ColumnDescription = "上负荷", IsNullable = true)]
        public string? UpperLoad { get; set; }

        [SugarColumn(ColumnDescription = "偏移量", IsNullable = true)]
        public string? Offset { get; set; }

        [SugarColumn(ColumnDescription = "压入前", IsNullable = true)]
        public string? BeforePressIn { get; set; }

        [SugarColumn(ColumnDescription = "压入后", IsNullable = true)]
        public string? AfterPressIn { get; set; }

        [SugarColumn(ColumnDescription = "间隙", IsNullable = true)]
        public string? Gap { get; set; }

        [SugarColumn(
            ColumnDescription = "铆接检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert),
            IsNullable = true
        )]
        public ResultEnum? RivetingResult { get; set; }

        [SugarColumn(ColumnDescription = "铆接时间", IsNullable = true)]
        public DateTime? RivetingTime { get; set; }

        [SugarColumn(
            ColumnDescription = "旋铆检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert),
            IsNullable = true
        )]
        public ResultEnum? SpinRivetingResult { get; set; }

        [SugarColumn(ColumnDescription = "旋铆检测时间", IsNullable = true)]
        public DateTime? SpinRivetingTime { get; set; }

        [SugarColumn(ColumnDescription = "铆接检测1高度", IsNullable = true)]
        public string? RivetingInspection1Height { get; set; }

        [SugarColumn(ColumnDescription = "铆接检测2高度", IsNullable = true)]
        public string? RivetingInspection2Height { get; set; }

        [SugarColumn(ColumnDescription = "铆接成型高度", IsNullable = true)]
        public string? RivetingInspectionFormingHeight { get; set; }

        [SugarColumn(
            ColumnDescription = "检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert),
            IsNullable = true
        )]
        public ResultEnum? VibCrackResult { get; set; }

        [SugarColumn(ColumnDescription = "震动时间", IsNullable = true)]
        public DateTime? VibCrackTime { get; set; }

        [SugarColumn(
            ColumnDescription = "ABS压紧结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert),
            IsNullable = true
        )]
        public ResultEnum? ABSPressDownResult { get; set; }

        [SugarColumn(ColumnDescription = "ABS压紧时间", IsNullable = true)]
        public DateTime? ABSPressDownTime { get; set; }

        [SugarColumn(
            ColumnDescription = "ABS检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert),
            IsNullable = true
        )]
        public ResultEnum? ABSCheckResult { get; set; }

        [SugarColumn(ColumnDescription = "ABS检测时间", IsNullable = true)]
        public DateTime? ABSCheckTime { get; set; }
    }
}
