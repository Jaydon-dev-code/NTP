using NPOI.SS.Formula.Functions;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    public class Tb_EnergyRangeRepository : BaseRepository<Tb_EnergyRange>
    {
        public Tb_EnergyRangeRepository(ISqlSugarClient db)
            : base(db) { }

        public async Task<List<Tb_EnergyRange>> GetListAsync()
        {
            return await _db.Queryable<Tb_EnergyRange>().ToListAsync();
        }

        public async Task<List<Tb_EnergyRange>> GetAllWithDetailsAsync()
        {
            return await _db.Queryable<Tb_EnergyRange>().Includes(x => x.Details).ToListAsync();
        }

        public async Task<Tb_EnergyRange> GetFirstAsync(
            Expression<Func<Tb_EnergyRange, bool>> expression
        )
        {
            return await _db.Queryable<Tb_EnergyRange>().FirstAsync(expression);
        }

        public async Task<int> InsertAsync(Tb_EnergyRange entity)
        {
            return await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateAsync(Tb_EnergyRange entity)
        {
            return await _db.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(Expression<Func<Tb_EnergyRange, bool>> expression)
        {
            return await _db.Deleteable<Tb_EnergyRange>().Where(expression).ExecuteCommandAsync();
        }

        public async Task<bool> DeleteNavAsync(Expression<Func<Tb_EnergyRange, bool>> whereExpression)
        {
            return await _db.DeleteNav<Tb_EnergyRange> (whereExpression)
                .IncludesAllFirstLayer()
                .ExecuteCommandAsync();
        }
    }
}
