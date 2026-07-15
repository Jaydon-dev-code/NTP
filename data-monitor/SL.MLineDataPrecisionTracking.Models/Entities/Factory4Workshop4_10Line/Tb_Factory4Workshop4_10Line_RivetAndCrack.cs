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
    /// 铆接和裂纹
    /// </summary>
    [SugarIndex("idx_Factory4Workshop4_10Line_RivetAndCrack_SN_Asc", nameof(Tb_Factory4Workshop4_10Line_Vib.SN), OrderByType.Asc)]
    [SugarTable("Factory4Workshop4_10Line_RivetAndCrack")]
    public class Tb_Factory4Workshop4_10Line_RivetAndCrack: Factory4Workshop4_10LineBase
    {
    }
}
