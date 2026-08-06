using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_3AssemblyLine;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage.Factory6Workshop6_3AssemblyLine
{
    public class Tb_Factory6Workshop6_3Line_EnergyRangePointRepository : BaseRepository<Tb_Factory6Workshop6_3Line_EnergyRangePoint>
    {
        public Tb_Factory6Workshop6_3Line_EnergyRangePointRepository(ISqlSugarClient db) : base(db)
        {
        }

        public async Task<List<Tb_Factory6Workshop6_3Line_EnergyRangePoint>> QueryableAsync(
         Expression<Func<Tb_Factory6Workshop6_3Line_EnergyRangePoint, bool>> expression
     )
        {
            return await _db.Queryable<Tb_Factory6Workshop6_3Line_EnergyRangePoint>()
                .Where(expression)
                .ToListAsync();
        }

        public async Task<(
            List<Tb_Factory6Workshop6_3Line_EnergyRangePoint> List,
            int TotalCount,
            int TotalPage
        )> QueryableAsync(
            Expression<Func<Tb_Factory6Workshop6_3Line_EnergyRangePoint, bool>> expression,
            int pageNumber,
            int pageSize
        )
        {
            RefAsync<int> totalCountRef = 0;
            RefAsync<int> totalPageRef = 0;

            var list = await _db.Queryable<Tb_Factory6Workshop6_3Line_EnergyRangePoint>()
                .Where(expression)
                .OrderBy(x => x.Time)
                .ToPageListAsync(pageNumber, pageSize, totalCountRef, totalPageRef);

            return (list, totalCountRef.Value, totalPageRef.Value);
        }
        public async Task<int> InsertableAsync(List<Tb_Factory6Workshop6_3Line_EnergyRangePoint> device)
        {
            return await _db.Insertable<Tb_Factory6Workshop6_3Line_EnergyRangePoint>(device).ExecuteCommandAsync();
        }
    }
}
