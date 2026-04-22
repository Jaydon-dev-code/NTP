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
    public class Tb_HeatTreatmentDataRepository : BaseRepository<Tb_HeatTreatmentData>
    {
        public Tb_HeatTreatmentDataRepository(ISqlSugarClient db)
            : base(db) { }

        public async Task<Tb_HeatTreatmentData> QueryableFirstAsync(
            Expression<Func<Tb_HeatTreatmentData, bool>> expression
        )
        {
            return await _db.Queryable<Tb_HeatTreatmentData>().FirstAsync(expression);
        }

        public async Task<List<Tb_HeatTreatmentData>> QueryableAsync(
            Expression<Func<Tb_HeatTreatmentData, bool>> expression
        )
        {
            return await _db.Queryable<Tb_HeatTreatmentData>().Where(expression).ToListAsync();
        }

        public async Task<(
            List<Tb_HeatTreatmentData> List,
            int TotalCount,
            int TotalPage
        )> QueryableAsync(
            Expression<Func<Tb_HeatTreatmentData, bool>> expression,
            Expression<Func<Tb_HeatTreatmentData, object>> orderby,
            int pageNumber,
            int pageSize
        )
        {
            RefAsync<int> totalCountRef = 0;
            RefAsync<int> totalPageRef = 0;

            var list = await _db.Queryable<Tb_HeatTreatmentData>()
                .Where(expression)
                .OrderByDescending(orderby)
                .ToPageListAsync(pageNumber, pageSize, totalCountRef, totalPageRef);

            return (list, totalCountRef.Value, totalPageRef.Value);
        }

        public async Task<int> InsertableAsync(Tb_HeatTreatmentData device)
        {
            return await _db.Insertable<Tb_HeatTreatmentData>(device).ExecuteCommandAsync();
        }

        public async Task<int> UpdateableAsync(Tb_HeatTreatmentData device)
        {
            return await _db.Updateable<Tb_HeatTreatmentData>(device).ExecuteCommandAsync();
        }

        public async Task<Tb_HeatTreatmentData> QueryableFirstAsync(
            Expression<Func<Tb_HeatTreatmentData, bool>> expression,
            Expression<Func<Tb_HeatTreatmentData, object>> oderby
        )
        {
            return await _db.Queryable<Tb_HeatTreatmentData>()
                .OrderByDescending(oderby)
                .FirstAsync(expression);
        }
    }
}
