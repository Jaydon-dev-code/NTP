using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    public class Tb_LineSummaryRepository : BaseRepository<Tb_LineSummary>
    {
        public Tb_LineSummaryRepository(ISqlSugarClient db) : base(db)
        {
        }
        public async Task<int> InsertableAsync(Tb_LineSummary  device)
        {
            return await _db.Insertable<Tb_LineSummary>(device).ExecuteCommandAsync();
        }
    }
}
