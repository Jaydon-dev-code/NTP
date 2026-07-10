using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_1AssemblyLine;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    public class Tb_Factory6Workshop6_1AssemblyLineARepository : BaseRepository<Tb_Factory6Workshop6_1AssemblyLineA>
    {
        public Tb_Factory6Workshop6_1AssemblyLineARepository(ISqlSugarClient db) : base(db)
        {
        }
        public async Task<int> InsertableAsync(Tb_Factory6Workshop6_1AssemblyLineA device)
        {
            return await _db.Insertable<Tb_Factory6Workshop6_1AssemblyLineA>(device).ExecuteCommandAsync();
        }
        public async Task<int> UpdateableAsync(Tb_Factory6Workshop6_1AssemblyLineA device)
        {
            return await _db.Updateable<Tb_Factory6Workshop6_1AssemblyLineA>(device).ExecuteCommandAsync();
        }
        public async Task<Tb_Factory6Workshop6_1AssemblyLineA> QueryableFirstAsync(Expression<Func<Tb_Factory6Workshop6_1AssemblyLineA, bool>> expression, Expression<Func<Tb_Factory6Workshop6_1AssemblyLineA, object>> oderby)
        {
            return await _db.Queryable<Tb_Factory6Workshop6_1AssemblyLineA>().OrderByDescending(oderby).FirstAsync(expression);
        }
    }
}
