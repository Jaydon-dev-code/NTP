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
    /// <summary>
    /// PLC点位仓储
    /// </summary>
    public class Tb_PlcPointRepository : BaseRepository<Tb_PlcPoint>
    {
        public Tb_PlcPointRepository(ISqlSugarClient db) : base(db)
        {
        }
        public async Task<int> DeleteableAsync(Expression<Func<Tb_PlcPoint, bool>> expression)
        {
            return await _db.Deleteable<Tb_PlcPoint>().Where(expression).ExecuteCommandAsync();

        }
        public async Task<int> InsertableAsync(Tb_PlcPoint device)
        {
            return await _db.Insertable<Tb_PlcPoint>(device).ExecuteCommandAsync();
        }

        public async Task<int> InsertableAsync(List<Tb_PlcPoint> devices)
        {
            return await _db.Insertable<Tb_PlcPoint>(devices).ExecuteCommandAsync();
        }



        public async Task<Tb_PlcPoint> QueryableFirstAsync(Expression<Func<Tb_PlcPoint, bool>> expression)
        {
            return await _db.Queryable<Tb_PlcPoint>().FirstAsync(expression);
        }
    }
}
