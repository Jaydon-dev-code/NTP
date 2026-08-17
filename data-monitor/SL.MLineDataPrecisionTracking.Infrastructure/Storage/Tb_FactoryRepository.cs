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
    /// 厂仓储
    /// </summary>
    public class Tb_FactoryRepository : BaseRepository<Tb_Factory>
    {
        public Tb_FactoryRepository(ISqlSugarClient db)
            : base(db) { }

        public async Task<List<Tb_Factory>> GetListAsync()
        {
            return await _db.Queryable<Tb_Factory>().OrderBy(f => f.Id).ToListAsync();
        }

        public async Task<List<Tb_Factory>> GetAllWithLinesAsync()
        {
            var factories = await _db
                .Queryable<Tb_Factory>()
                .Includes(f => f.ProductionLines)
                .ToListAsync();

            // 一次性查出所有产线及其设备，再按厂归属
            var lines = await _db
                .Queryable<Tb_ProductionLine>()
                .Includes(l => l.Equipments)
                .ToListAsync();

            foreach (var factory in factories)
            {
                if (factory.ProductionLines == null)
                    factory.ProductionLines = new List<Tb_ProductionLine>();
                factory.ProductionLines = lines
                    .Where(l => l.FactoryId == factory.Id)
                    .ToList();
            }
            return factories;
        }

        public async Task<Tb_Factory> GetFirstAsync(Expression<Func<Tb_Factory, bool>> expression)
        {
            return await _db.Queryable<Tb_Factory>().FirstAsync(expression);
        }

        public async Task<bool> AnyAsync(Expression<Func<Tb_Factory, bool>> expression)
        {
            return await _db.Queryable<Tb_Factory>().AnyAsync(expression);
        }

        public async Task<int> InsertAsync(Tb_Factory entity)
        {
            return await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateAsync(Tb_Factory entity)
        {
            return await _db.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(Expression<Func<Tb_Factory, bool>> expression)
        {
            return await _db.Deleteable<Tb_Factory>().Where(expression).ExecuteCommandAsync();
        }
    }
}
