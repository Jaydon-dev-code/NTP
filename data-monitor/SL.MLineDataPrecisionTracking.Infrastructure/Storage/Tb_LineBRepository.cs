using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    public class Tb_LineBRepository : BaseRepository<TB_LineB>
    {
        public Tb_LineBRepository(ISqlSugarClient db) : base(db)
        {
        }
        public async Task<int> InsertableAsync(TB_LineB device)
        {
            return await _db.Insertable<TB_LineB>(device).ExecuteCommandAsync();
        }
    }
}
