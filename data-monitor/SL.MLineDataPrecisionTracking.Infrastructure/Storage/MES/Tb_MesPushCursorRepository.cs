using SL.MLineDataPrecisionTracking.Models.Entities.Mes;
using SqlSugar;
using System;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    /// <summary>
    /// MES 推送游标仓储（游标存本库）
    /// </summary>
    public class Tb_MesPushCursorRepository : BaseRepository<Tb_MesPushCursor>
    {
        public Tb_MesPushCursorRepository(ISqlSugarClient db)
            : base(db) { }

        /// <summary>
        /// 按游标键（表名）查询
        /// </summary>
        public async Task<Tb_MesPushCursor> GetByKeyAsync(string cursorKey)
        {
            return await _db
                .Queryable<Tb_MesPushCursor>()
                .Where(x => x.CursorKey == cursorKey)
                .FirstAsync();
        }

        /// <summary>
        /// 新增或更新游标
        /// </summary>
        public async Task<int> UpsertAsync(string cursorKey, DateTime lastMeasureTime)
        {
            var existing = await GetByKeyAsync(cursorKey);
            if (existing == null)
            {
                return await _db
                    .Insertable(
                        new Tb_MesPushCursor
                        {
                            CursorKey = cursorKey,
                            LastMeasureTime = lastMeasureTime,
                            UpdateTime = DateTime.Now,
                        }
                    )
                    .ExecuteReturnIdentityAsync();
            }

            existing.LastMeasureTime = lastMeasureTime;
            existing.UpdateTime = DateTime.Now;
            return await _db.Updateable(existing).ExecuteCommandAsync();
        }
    }
}
