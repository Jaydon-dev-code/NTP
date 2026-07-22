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
    public class Tb_Factory4Workshop4_10LineSummaryRepository
        : BaseRepository<Tb_Factory4Workshop4_10LineSummary>
    {
        public Tb_Factory4Workshop4_10LineSummaryRepository(ISqlSugarClient db)
            : base(db) { }

        public async Task<int> InsertableAsync(Tb_Factory4Workshop4_10LineSummary device)
        {
            return await _db.Insertable<Tb_Factory4Workshop4_10LineSummary>(device)
                .ExecuteCommandAsync();
        }

        public async Task<List<Tb_Factory4Workshop4_10LineSummary>> QueryableAsync(
            Expression<Func<Tb_Factory4Workshop4_10LineSummary, bool>> expression
        )
        {
            return await _db.Queryable<Tb_Factory4Workshop4_10LineSummary>()
                .Where(expression)
                .ToListAsync();
        }

        public async Task<Tb_Factory4Workshop4_10LineSummary> QueryableFirstAsync(
            Expression<Func<Tb_Factory4Workshop4_10LineSummary, bool>> expression
        )
        {
            return await _db.Queryable<Tb_Factory4Workshop4_10LineSummary>().FirstAsync(expression);
        }
        /// <summary>
        /// 不用主键更新并更新指定列
        /// </summary>
        /// <param name="lineSummary"></param>
        /// <param name="columns"></param>
        /// <param name="upColumnsName"></param>
        /// <returns></returns>
        public async Task<int> UpDataAsync(Tb_Factory4Workshop4_10LineSummary lineSummary, Expression<Func<Tb_Factory4Workshop4_10LineSummary, object>> columns, params string[] upColumnsName)
        {
            return  await _db.Updateable(lineSummary).WhereColumns(columns).UpdateColumns(upColumnsName).ExecuteCommandAsync();
            ;
        }

        public async Task<(
            List<Tb_Factory4Workshop4_10LineSummary> List,
            int TotalCount,
            int TotalPage
        )> QueryableAsync(
            Expression<Func<Tb_Factory4Workshop4_10LineSummary, bool>> expression,
            Expression<Func<Tb_Factory4Workshop4_10LineSummary, object>> orderby,
            int pageNumber,
            int pageSize
        )
        {
            RefAsync<int> totalCountRef = 0;
            RefAsync<int> totalPageRef = 0;

            var list = await _db.Queryable<Tb_Factory4Workshop4_10LineSummary>()
                .Where(expression)
                .OrderByDescending(orderby)
                .ToPageListAsync(pageNumber, pageSize, totalCountRef, totalPageRef);

            return (list, totalCountRef.Value, totalPageRef.Value);
        }
    }
}
