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
    /// <summary>
    /// 铆接和裂纹
    /// </summary>
    [SugarTable("Factory4Workshop4_10Line_RivetAndCrack")]
    public class Tb_Factory4Workshop4_10Line_RivetAndCrack : Factory4Workshop4_10LineBase
    {
        [SugarColumn(
            ColumnDescription = "检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert)
        )]
        public ResultEnum RivetAndCrackResult { get; set; }
    }
}
