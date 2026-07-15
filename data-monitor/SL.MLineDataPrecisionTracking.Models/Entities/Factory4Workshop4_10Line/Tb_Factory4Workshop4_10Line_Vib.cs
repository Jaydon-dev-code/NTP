using SL.MLineDataPrecisionTracking.Models.Entities.Base;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line
{
    /// <summary>
    /// 震动
    /// </summary>
    [SugarIndex("idx_Factory4Workshop4_10Line_Vib_SN_Asc", nameof(Tb_Factory4Workshop4_10Line_Vib.SN), OrderByType.Asc)]
    [SugarTable("Factory4Workshop4_10Line_Vib")]
    public class Tb_Factory4Workshop4_10Line_Vib: Factory4Workshop4_10LineBase
    {
    }
}
