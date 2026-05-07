using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    public class Tb_LineBRepository : BaseRepository<Tb_LineB>
    {
        public Tb_LineBRepository(ISqlSugarClient db) : base(db)
        {
        }
        public async Task<int> InsertableAsync(Tb_LineB device)
        {
            return await _db.Insertable<Tb_LineB>(device).ExecuteCommandAsync();
        }
        public async Task<int> InsertableReturnIdentityAsync(Tb_LineB device)
        {
            return await _db.Insertable<Tb_LineB>(device).ExecuteReturnIdentityAsync();
        }
        public async Task<int> UpdateableAsync(Tb_LineB device)
        {
            return await _db.Updateable<Tb_LineB>(device).ExecuteCommandAsync();
        }
    }
}
