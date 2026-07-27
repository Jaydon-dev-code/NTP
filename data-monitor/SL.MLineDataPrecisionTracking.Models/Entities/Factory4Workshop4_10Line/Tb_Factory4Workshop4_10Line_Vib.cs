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
    [SugarTable("Factory4Workshop4_10Line_Vib")]
    public class Tb_Factory4Workshop4_10Line_Vib : Factory4Workshop4_10LineBase
    {
        [SugarColumn(
            ColumnDescription = "检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert),
            IsNullable = true
        )]
        public ResultEnum? VibCrackResult { get; set; }

        [SugarColumn(ColumnDescription = "震动时间", IsNullable = true)]
        public DateTime? VibCrackTime { get; set; }
    }
}
