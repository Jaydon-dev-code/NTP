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
    public class Tb_LineSummaryRepository : BaseRepository<Tb_LineSummary>
    {
        public Tb_LineSummaryRepository(ISqlSugarClient db)
            : base(db) { }

        public async Task<int> InsertableAsync(Tb_LineSummary device)
        {
            return await _db.Insertable<Tb_LineSummary>(device).ExecuteCommandAsync();
        }

        public async Task<(
            List<Tb_LineSummary> List,
            int TotalCount,
            int TotalPage
        )> QueryableAsync(
            Expression<Func<Tb_LineSummary, bool>> expression,
            Expression<Func<Tb_LineSummary, object>> orderby,
            int pageNumber,
            int pageSize
        )
        {
            RefAsync<int> totalCountRef = 0;
            RefAsync<int> totalPageRef = 0;

            var list = await _db.Queryable<Tb_LineSummary>()
                .Where(expression)
                .OrderByDescending(orderby)
                .ToPageListAsync(pageNumber, pageSize, totalCountRef, totalPageRef);

            return (list, totalCountRef.Value, totalPageRef.Value);
        }
    }
}
