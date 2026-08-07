using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities.Base;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_3AssemblyLine
{
    [SugarTable("Factory6Workshop6_3Line_EnergyRange")]
    public class Tb_Factory6Workshop6_3Line_EnergyRange : DataCollectionBase
    {
        [SugarColumn(DefaultValue = "0")]
        public int Count { get; set; }
    }
}
