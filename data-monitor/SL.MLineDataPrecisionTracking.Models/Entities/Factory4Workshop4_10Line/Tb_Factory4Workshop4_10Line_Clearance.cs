using SL.MLineDataPrecisionTracking.Models.Entities.Base;
using SL.MLineDataPrecisionTracking.Models.Entitss.Factory4Workshop4_10Line;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line
{
    /// <summary>
    /// 游隙
    /// </summary>
    [SugarIndex("idx_Factory4Workshop4_10Line_Clearancek_SN_Asc", nameof(Tb_Factory4Workshop4_10Line_Clearance.SN), OrderByType.Asc)]
    [SugarTable("Factory4Workshop4_10Line_Clearance")]
    public class Tb_Factory4Workshop4_10Line_Clearance: Factory4Workshop4_10LineBase
    {
    }
}
