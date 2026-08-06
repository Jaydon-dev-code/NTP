using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_3AssemblyLine;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage.Factory6Workshop6_3AssemblyLine
{
    public class Tb_Factory6Workshop6_3Line_EnergyRangeRepository
        : BaseRepository<Tb_Factory6Workshop6_3Line_EnergyRange>
    {
        public Tb_Factory6Workshop6_3Line_EnergyRangeRepository(ISqlSugarClient db)
            : base(db) { }

        public async Task<List<Tb_Factory6Workshop6_3Line_EnergyRange>> QueryableAsync(
            Expression<Func<Tb_Factory6Workshop6_3Line_EnergyRange, bool>> expression
        )
        {
            return await _db.Queryable<Tb_Factory6Workshop6_3Line_EnergyRange>()
                .Where(expression)
                .ToListAsync();
        }

        public async Task<(
            List<Tb_Factory6Workshop6_3Line_EnergyRange> List,
            int TotalCount,
            int TotalPage
        )> QueryableAsync(
            Expression<Func<Tb_Factory6Workshop6_3Line_EnergyRange, bool>> expression,
            Expression<Func<Tb_Factory6Workshop6_3Line_EnergyRange, object>> orderby,
            int pageNumber,
            int pageSize
        )
        {
            RefAsync<int> totalCountRef = 0;
            RefAsync<int> totalPageRef = 0;

            var list = await _db.Queryable<Tb_Factory6Workshop6_3Line_EnergyRange>()
                .Where(expression)
                .OrderByDescending(orderby)
                .ToPageListAsync(pageNumber, pageSize, totalCountRef, totalPageRef);

            return (list, totalCountRef.Value, totalPageRef.Value);
        }

        public async Task<int> InsertableAsync(Tb_Factory6Workshop6_3Line_EnergyRange device)
        {
            return await _db.Insertable<Tb_Factory6Workshop6_3Line_EnergyRange>(device)
                .ExecuteCommandAsync();
        }
    }
}
