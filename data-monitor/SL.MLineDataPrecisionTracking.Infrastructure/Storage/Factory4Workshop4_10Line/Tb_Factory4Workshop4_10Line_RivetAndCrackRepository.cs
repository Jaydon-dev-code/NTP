using NPOI.SS.Formula.Functions;
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
    public class Tb_Factory4Workshop4_10Line_RivetAndCrackRepository : BaseRepository<Tb_Factory4Workshop4_10Line_RivetAndCrack>
    {
        public Tb_Factory4Workshop4_10Line_RivetAndCrackRepository(ISqlSugarClient db) : base(db)
        {
      
        }
        public async Task<int> InsertableAsync(Tb_Factory4Workshop4_10Line_RivetAndCrack device)
        {
            return await _db.Insertable<Tb_Factory4Workshop4_10Line_RivetAndCrack>(device).ExecuteCommandAsync();
        }
        public async Task<int> InsertableReturnIdentityAsync(Tb_Factory4Workshop4_10Line_RivetAndCrack device)
        {
            return await _db.Insertable<Tb_Factory4Workshop4_10Line_RivetAndCrack>(device).ExecuteReturnIdentityAsync();
        }

        public async Task<Tb_Factory4Workshop4_10Line_RivetAndCrack> QueryableFirstAsync(Expression<Func<Tb_Factory4Workshop4_10Line_RivetAndCrack, bool>> expression)
        {
            return await _db.Queryable<Tb_Factory4Workshop4_10Line_RivetAndCrack>().FirstAsync(expression);
        }

        public async Task<int> UpdateableAsync(Tb_Factory4Workshop4_10Line_RivetAndCrack device)
        {
            return await _db.Updateable<Tb_Factory4Workshop4_10Line_RivetAndCrack>(device).ExecuteCommandAsync();
        }
    }
}
