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
    public class Tb_LineARepository : BaseRepository<Tb_LineA>
    {
        public Tb_LineARepository(ISqlSugarClient db) : base(db)
        {
        }
        public async Task<int> InsertableAsync(Tb_LineA device)
        {
            return await _db.Insertable<Tb_LineA>(device).ExecuteCommandAsync();
        }
        public async Task<Tb_LineA> QueryableFirstAsync(Expression<Func<Tb_LineA, bool>> expression, Expression<Func<Tb_LineA, object>> oderby)
        {
            return await _db.Queryable<Tb_LineA>().OrderByDescending(oderby).FirstAsync(expression);
        }
    }
}
