using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    public class Tb_EnergyRangeDetailRepository : BaseRepository<Tb_EnergyRangeDetail>
    {
        public Tb_EnergyRangeDetailRepository(ISqlSugarClient db) : base(db) { }

        public async Task<List<Tb_EnergyRangeDetail>> GetListAsync(Expression<Func<Tb_EnergyRangeDetail, bool>> expression)
        {
            return await _db.Queryable<Tb_EnergyRangeDetail>().Where(expression).ToListAsync();
        }

        public async Task<int> InsertAsync(List<Tb_EnergyRangeDetail> entities)
        {
            return await _db.Insertable(entities).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(Expression<Func<Tb_EnergyRangeDetail, bool>> expression)
        {
            return await _db.Deleteable<Tb_EnergyRangeDetail>().Where(expression).ExecuteCommandAsync();
        }

        public async Task<Tb_EnergyRangeDetail> GetFirstAsync(Expression<Func<Tb_EnergyRangeDetail, bool>> expression)
        {
            return await _db.Queryable<Tb_EnergyRangeDetail>().FirstAsync(expression);
        }
    }
}
