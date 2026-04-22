using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SL.RclMLineDataPrecisionTracking.Models.Entities;
using SqlSugar;

namespace SL.RclMLineDataPrecisionTracking.Infrastructure.Storage
{
    public class Tb_HeatTreatmentDataRepository : BaseRepository<Tb_HeatTreatmentData>
    {
        public Tb_HeatTreatmentDataRepository(ISqlSugarClient db)
            : base(db) { }

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
