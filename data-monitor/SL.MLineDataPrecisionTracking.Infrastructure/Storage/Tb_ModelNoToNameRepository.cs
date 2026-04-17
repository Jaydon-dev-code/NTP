using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{

    public class Tb_ModelNoToNameRepository : BaseRepository<Tb_ModelNoToName>
    {
        public Tb_ModelNoToNameRepository(ISqlSugarClient db) : base(db)
        {
        }
        public async Task<int> InsertableAsync(Tb_ModelNoToName device)
        {
            return await _db.Insertable<Tb_ModelNoToName>(device).ExecuteCommandAsync();
        }
        public async Task<List<Tb_ModelNoToName>> QueryabletAsync(Expression<Func<Tb_ModelNoToName, bool>> expression)
        {
            return await _db.Queryable<Tb_ModelNoToName>().Where(expression).ToListAsync();
        }
    }
}
