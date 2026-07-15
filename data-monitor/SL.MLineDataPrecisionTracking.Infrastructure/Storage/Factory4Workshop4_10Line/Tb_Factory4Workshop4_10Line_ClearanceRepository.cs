using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    public class Tb_Factory4Workshop4_10Line_ClearanceRepository : BaseRepository<Tb_Factory4Workshop4_10Line_Clearance>
    {
        public Tb_Factory4Workshop4_10Line_ClearanceRepository(ISqlSugarClient db) : base(db)
        {
        }
        public async Task<int> InsertableAsync(Tb_Factory4Workshop4_10Line_Clearance device)
        {
            return await _db.Insertable<Tb_Factory4Workshop4_10Line_Clearance>(device).ExecuteCommandAsync();
        }
        public async Task<int> UpdateableAsync(Tb_Factory4Workshop4_10Line_Clearance device)
        {
            return await _db.Updateable<Tb_Factory4Workshop4_10Line_Clearance>(device).ExecuteCommandAsync();
        }
        public async Task<Tb_Factory4Workshop4_10Line_Clearance> QueryableFirstAsync(Expression<Func<Tb_Factory4Workshop4_10Line_Clearance, bool>> expression, Expression<Func<Tb_Factory4Workshop4_10Line_Clearance, object>> oderby)
        {
            return await _db.Queryable<Tb_Factory4Workshop4_10Line_Clearance>().OrderByDescending(oderby).FirstAsync(expression);
        }

        public async Task<Tb_Factory4Workshop4_10Line_Clearance> QueryableFirstAsync(Expression<Func<Tb_Factory4Workshop4_10Line_Clearance, bool>> expression)
        {
            return await _db.Queryable<Tb_Factory4Workshop4_10Line_Clearance>().FirstAsync(expression);
        }
    }
}
