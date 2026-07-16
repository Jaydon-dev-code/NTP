using SL.MLineDataPrecisionTracking.Models.Entities.Base;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line
{
    [SugarIndex("idx_Factory4Workshop4_10LineSummary_SN_Asc", nameof(Tb_Factory4Workshop4_10LineSummary.SN), OrderByType.Asc)]
    [SugarTable("Factory4Workshop4_10LineSummary")]
    public class Tb_Factory4Workshop4_10LineSummary: Factory4Workshop4_10LineBase
    {
    }
}
