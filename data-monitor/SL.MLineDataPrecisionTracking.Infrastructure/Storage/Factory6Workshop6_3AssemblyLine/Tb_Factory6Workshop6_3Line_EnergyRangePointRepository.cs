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

        /// <summary>
        /// 只取分页数据，不做 count 统计（总条数由子项的 Count 字段提供，避免全表扫描）
        /// </summary>
        public async Task<List<Tb_Factory6Workshop6_3Line_EnergyRangePoint>> QueryablePageAsync(
            Expression<Func<Tb_Factory6Workshop6_3Line_EnergyRangePoint, bool>> expression,
            int pageNumber,
            int pageSize
        )
        {
            return await _db.Queryable<Tb_Factory6Workshop6_3Line_EnergyRangePoint>()
                .Where(expression)
                .OrderBy(x => x.Time)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// 按条件一次查出全部点（已知子项 Count，直接取全量，不分页）
        /// </summary>
        public async Task<List<Tb_Factory6Workshop6_3Line_EnergyRangePoint>> QueryableAllAsync(
            Expression<Func<Tb_Factory6Workshop6_3Line_EnergyRangePoint, bool>> expression,
            int count
        )
        {
            return await _db.Queryable<Tb_Factory6Workshop6_3Line_EnergyRangePoint>()
                .Where(expression)
                .OrderBy(x => x.Time)
                .Take(count)
                .ToListAsync();
        }
        public async Task<int> InsertableAsync(List<Tb_Factory6Workshop6_3Line_EnergyRangePoint> device)
        {
            return await _db.Insertable<Tb_Factory6Workshop6_3Line_EnergyRangePoint>(device).ExecuteCommandAsync();
        }
    }
}
