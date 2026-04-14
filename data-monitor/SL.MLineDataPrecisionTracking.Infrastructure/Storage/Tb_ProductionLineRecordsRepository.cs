using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    public class Tb_ProductionLineRecordsRepository : BaseRepository<Tb_ProductionLineRecords>
    {
        public Tb_ProductionLineRecordsRepository(ISqlSugarClient db) : base(db)
        {
        }
    }
}
