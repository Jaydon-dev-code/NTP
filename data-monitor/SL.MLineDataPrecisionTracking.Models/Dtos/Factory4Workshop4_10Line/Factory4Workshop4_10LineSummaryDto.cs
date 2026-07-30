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
    public class Factory4Workshop4_10LineSummaryDto
    {
        [Description("时间")]
        public DateTime RecordTime { get; set; }

        [Description("序列码")]
        public string? SN { get; set; }

        [Description("游隙1工位检测结果")]
        public ResultEnum? Clearance1Result { get; set; }

        [Description("游隙2工位检测结果")]
        public ResultEnum? Clearance2Result { get; set; }

        [Description("游隙1工位采集时间")]
        public DateTime? Clearance1Time { get; set; }

        [Description("游隙2工位采集时间")]
        public DateTime? Clearance2Time { get; set; }

        [Description("正间隙")]
        public string? PositiveGap { get; set; }

        [Description("下负荷")]
        public string? LowerLoad { get; set; }

        [Description("上负荷")]
        public string? UpperLoad { get; set; }

        [Description("偏移量")]
        public string? Offset { get; set; }

        [Description("压入前")]
        public string? BeforePressIn { get; set; }

        [Description("压入后")]
        public string? AfterPressIn { get; set; }

        [Description("间隙")]
        public string? Gap { get; set; }

        [Description("铆接检测结果")]
        public ResultEnum? RivetingResult { get; set; }

        [Description("铆接时间")]
        public DateTime? RivetingTime { get; set; }

        [Description("铆接检测1高度")]
        public string? RivetingInspection1Height { get; set; }

        [Description("铆接检测2高度")]
        public string? RivetingInspection2Height { get; set; }

        [Description("铆接成型高度")]
        public string? RivetingInspectionFormingHeight { get; set; }

        [SugarColumn(ColumnDescription = "旋铆检测结果")]
        public ResultEnum? SpinRivetingResult { get; set; }

        [Description("旋铆检测时间")]
        public DateTime? SpinRivetingTime { get; set; }

        [Description("检测结果")]
        public ResultEnum? VibCrackResult { get; set; }

        [Description("震动时间")]
        public DateTime? VibCrackTime { get; set; }

        [Description("ABS压紧结果")]
        public ResultEnum? ABSPressDownResult { get; set; }

        [Description("ABS压紧时间")]
        public DateTime? ABSPressDownTime { get; set; }

        [Description("ABS检测结果")]
        public ResultEnum? ABSCheckResult { get; set; }

        [Description("ABS检测时间")]
        public DateTime? ABSCheckTime { get; set; }
    }
}
