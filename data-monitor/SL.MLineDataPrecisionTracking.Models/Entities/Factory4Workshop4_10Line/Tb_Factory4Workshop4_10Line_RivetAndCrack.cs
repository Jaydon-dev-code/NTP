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
    [SugarIndex("unique_Factory4Workshop4_10Line_RivetAndCrack_SN", nameof(Tb_Factory4Workshop4_10Line_RivetAndCrack.SN), OrderByType.Desc, true)]
    [SugarTable("Factory4Workshop4_10Line_RivetAndCrack")]
    public class Tb_Factory4Workshop4_10Line_RivetAndCrack : Factory4Workshop4_10LineBase
    {
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
    }
}
