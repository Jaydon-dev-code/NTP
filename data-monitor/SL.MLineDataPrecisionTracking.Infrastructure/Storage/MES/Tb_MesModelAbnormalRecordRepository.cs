using SL.MLineDataPrecisionTracking.Models.Entities.Mes;
using SqlSugar;
using System;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    /// <summary>
    /// MES 型号异常记录仓储
    /// </summary>
    public class Tb_MesModelAbnormalRecordRepository : BaseRepository<Tb_MesModelAbnormalRecord>
    {
        public Tb_MesModelAbnormalRecordRepository(ISqlSugarClient db)
            : base(db) { }

        /// <summary>
        /// 新增异常记录，返回主键ID
        /// </summary>
        public async Task<int> InsertAsync(Tb_MesModelAbnormalRecord entity)
        {
            return await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        }

        /// <summary>
        /// 按ID查询
        /// </summary>
        public async Task<Tb_MesModelAbnormalRecord> GetByIdAsync(int id)
        {
            return await _db.Queryable<Tb_MesModelAbnormalRecord>().Where(x => x.Id == id).FirstAsync();
        }

        /// <summary>
        /// 更新开始时间（同时置状态为记录中）
        /// </summary>
        public async Task<int> UpdateStartTimeAsync(int id, DateTime startTime)
        {
            return await _db
                .Updateable<Tb_MesModelAbnormalRecord>()
                .SetColumns(x => x.StartTime == startTime)
                .SetColumns(x => x.Status == 1)
                .Where(x => x.Id == id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 更新结束时间（同时置状态为已结束）
        /// </summary>
        public async Task<int> UpdateEndTimeAsync(int id, DateTime endTime)
        {
            return await _db
                .Updateable<Tb_MesModelAbnormalRecord>()
                .SetColumns(x => x.EndTime == endTime)
                .SetColumns(x => x.Status == 2)
                .Where(x => x.Id == id)
                .ExecuteCommandAsync();
        }
    }
}
