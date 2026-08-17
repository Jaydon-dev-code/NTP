using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    /// <summary>
    /// 产线仓储
    /// </summary>
    public class Tb_ProductionLineRepository : BaseRepository<Tb_ProductionLine>
    {
        public Tb_ProductionLineRepository(ISqlSugarClient db)
            : base(db) { }

        public async Task<List<Tb_ProductionLine>> GetListAsync()
        {
            return await _db
                .Queryable<Tb_ProductionLine>()
                .OrderBy(l => l.FactoryId)
                .OrderBy(l => l.Id)
                .ToListAsync();
        }

        public async Task<List<Tb_ProductionLine>> GetListAsync(
            Expression<Func<Tb_ProductionLine, bool>> expression
        )
        {
            return await _db
                .Queryable<Tb_ProductionLine>()
                .Where(expression)
                .OrderBy(l => l.Id)
                .ToListAsync();
        }

        public async Task<List<Tb_ProductionLine>> GetListWithFactoryAsync()
        {
            return await _db
                .Queryable<Tb_ProductionLine>()
                .Includes(l => l.Factory)
                .OrderBy(l => l.FactoryId)
                .OrderBy(l => l.Id)
                .ToListAsync();
        }

        public async Task<Tb_ProductionLine> GetFirstAsync(
            Expression<Func<Tb_ProductionLine, bool>> expression
        )
        {
            return await _db.Queryable<Tb_ProductionLine>().FirstAsync(expression);
        }

        public async Task<bool> AnyAsync(Expression<Func<Tb_ProductionLine, bool>> expression)
        {
            return await _db.Queryable<Tb_ProductionLine>().AnyAsync(expression);
        }

        public async Task<int> InsertAsync(Tb_ProductionLine entity)
        {
            return await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateAsync(Tb_ProductionLine entity)
        {
            return await _db.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(Expression<Func<Tb_ProductionLine, bool>> expression)
        {
            return await _db
                .Deleteable<Tb_ProductionLine>()
                .Where(expression)
                .ExecuteCommandAsync();
        }
    }
}
