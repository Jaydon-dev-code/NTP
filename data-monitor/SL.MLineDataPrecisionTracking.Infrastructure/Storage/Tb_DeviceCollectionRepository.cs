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
    /// <summary>
    /// 设备采集注册表仓储
    /// </summary>
    public class Tb_DeviceCollectionRepository : BaseRepository<Tb_DeviceCollection>
    {
        public Tb_DeviceCollectionRepository(ISqlSugarClient db)
            : base(db) { }

        public async Task<List<Tb_DeviceCollection>> GetListAsync()
        {
            return await _db.Queryable<Tb_DeviceCollection>().OrderBy(x => x.Id).ToListAsync();
        }

        public async Task<Tb_DeviceCollection> GetByEquipmentIdAsync(string equipmentId)
        {
            return await _db
                .Queryable<Tb_DeviceCollection>()
                .FirstAsync(x => x.EquipmentId == equipmentId);
        }

        public async Task<bool> AnyAsync(string equipmentId)
        {
            return await _db.Queryable<Tb_DeviceCollection>().AnyAsync(x => x.EquipmentId == equipmentId);
        }

        /// <summary>
        /// 按设备编号 upsert（存在则更新，否则插入）
        /// </summary>
        public async Task UpsertAsync(Tb_DeviceCollection entity)
        {
            var existing = await GetByEquipmentIdAsync(entity.EquipmentId);
            if (existing == null)
            {
                entity.CreateTime = DateTime.Now;
                entity.LastUpdateTime = DateTime.Now;
                await _db.Insertable(entity).ExecuteCommandAsync();
            }
            else
            {
                entity.Id = existing.Id;
                entity.CreateTime = existing.CreateTime;
                entity.LastUpdateTime = DateTime.Now;
                await _db.Updateable(entity).ExecuteCommandAsync();
            }
        }

        public async Task<int> DeleteAsync(string equipmentId)
        {
            return await _db
                .Deleteable<Tb_DeviceCollection>()
                .Where(x => x.EquipmentId == equipmentId)
                .ExecuteCommandAsync();
        }
    }
}
