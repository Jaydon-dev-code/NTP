using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_1AssemblyLine;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    public class Tb_Factory6Workshop6_1AssemblyLineABSummaryRepository : BaseRepository<Tb_Factory6Workshop6_1AssemblyLineABSummary>
    {
        public Tb_Factory6Workshop6_1AssemblyLineABSummaryRepository(ISqlSugarClient db)
            : base(db) { }

        public async Task<int> InsertableAsync(Tb_Factory6Workshop6_1AssemblyLineABSummary device)
        {
            return await _db.Insertable<Tb_Factory6Workshop6_1AssemblyLineABSummary>(device).ExecuteCommandAsync();
        }

        public async Task<List<Tb_Factory6Workshop6_1AssemblyLineABSummary>> QueryableAsync(
            Expression<Func<Tb_Factory6Workshop6_1AssemblyLineABSummary, bool>> expression
        )
        {
            return await _db.Queryable<Tb_Factory6Workshop6_1AssemblyLineABSummary>().Where(expression).ToListAsync();
        }
        public async Task<Tb_Factory6Workshop6_1AssemblyLineABSummary> QueryableFirstAsync(
       Expression<Func<Tb_Factory6Workshop6_1AssemblyLineABSummary, bool>> expression
   )
        {
            return await _db.Queryable<Tb_Factory6Workshop6_1AssemblyLineABSummary>().FirstAsync(expression);
        }
        public async Task<(
            List<Tb_Factory6Workshop6_1AssemblyLineABSummary> List,
            int TotalCount,
            int TotalPage
        )> QueryableAsync(
            Expression<Func<Tb_Factory6Workshop6_1AssemblyLineABSummary, bool>> expression,
            Expression<Func<Tb_Factory6Workshop6_1AssemblyLineABSummary, object>> orderby,
            int pageNumber,
            int pageSize
        )
        {
            RefAsync<int> totalCountRef = 0;
            RefAsync<int> totalPageRef = 0;

            var list = await _db.Queryable<Tb_Factory6Workshop6_1AssemblyLineABSummary>()
                .Where(expression)
                .OrderByDescending(orderby)
                .ToPageListAsync(pageNumber, pageSize, totalCountRef, totalPageRef);

            return (list, totalCountRef.Value, totalPageRef.Value);
        }
    }
}
