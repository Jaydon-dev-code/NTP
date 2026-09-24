using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities.SPC;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage.SPC
{
    public class Tb_SPC_PlcPointRepository : BaseRepository<Tb_SPC_PlcPoint>
    {
        private readonly ISqlSugarClient _db;

        public Tb_SPC_PlcPointRepository(ISqlSugarClient db) : base(db)
        {
            _db = db;
        }

        public async Task<List<Tb_SPC_PlcPoint>> GetListAsync()
        {
            return await _db.Queryable<Tb_SPC_PlcPoint>()
                .Includes(x => x.Characteristic)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<Tb_SPC_PlcPoint> GetByCharacteristicIdAsync(int characteristicId)
        {
            return await _db.Queryable<Tb_SPC_PlcPoint>()
                .Where(x => x.CharacteristicId == characteristicId)
                .FirstAsync();
        }

        /// <summary>查该特性下所有 PLC 点位</summary>
        public async Task<List<Tb_SPC_PlcPoint>> GetAllByCharacteristicIdAsync(int characteristicId)
        {
            return await _db.Queryable<Tb_SPC_PlcPoint>()
                .Where(x => x.CharacteristicId == characteristicId)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        /// <summary>批量插入</summary>
        public async Task<int> InsertBatchAsync(List<Tb_SPC_PlcPoint> list)
        {
            if (list == null || list.Count == 0) return 0;
            foreach (var item in list) item.CreateTime = DateTime.Now;
            return await _db.Insertable(list).ExecuteCommandAsync();
        }

        public async Task<int> InsertAsync(Tb_SPC_PlcPoint entity)
        {
            return await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        }

        public async Task<bool> UpdateAsync(Tb_SPC_PlcPoint entity)
        {
            return await _db.Updateable(entity).ExecuteCommandHasChangeAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _db.Deleteable<Tb_SPC_PlcPoint>().In(id).ExecuteCommandHasChangeAsync();
        }

        public async Task<bool> DeleteByCharacteristicIdAsync(int characteristicId)
        {
            return await _db.Deleteable<Tb_SPC_PlcPoint>()
                .Where(x => x.CharacteristicId == characteristicId)
                .ExecuteCommandHasChangeAsync();
        }
    }
}
