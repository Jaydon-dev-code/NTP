using System.Collections.Generic;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities.SPC;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage.SPC
{
    public class Tb_SPC_CollectionSourceRepository : BaseRepository<Tb_SPC_CollectionSource>
    {
        public Tb_SPC_CollectionSourceRepository(ISqlSugarClient db) : base(db) { }

        public async Task<List<Tb_SPC_CollectionSource>> GetListAsync()
        {
            return await _db.Queryable<Tb_SPC_CollectionSource>()
                .Includes(x => x.Characteristic)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<Tb_SPC_CollectionSource> GetByCharacteristicIdAsync(int characteristicId)
        {
            return await _db.Queryable<Tb_SPC_CollectionSource>()
                .Where(x => x.CharacteristicId == characteristicId)
                .FirstAsync();
        }

        public async Task<int> InsertAsync(Tb_SPC_CollectionSource entity)
        {
            return await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        }

        public async Task<bool> UpdateAsync(Tb_SPC_CollectionSource entity)
        {
            return await _db.Updateable(entity).ExecuteCommandHasChangeAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _db.Deleteable<Tb_SPC_CollectionSource>().In(id).ExecuteCommandHasChangeAsync();
        }

        public async Task<bool> DeleteByCharacteristicIdAsync(int characteristicId)
        {
            return await _db.Deleteable<Tb_SPC_CollectionSource>()
                .Where(x => x.CharacteristicId == characteristicId)
                .ExecuteCommandHasChangeAsync();
        }
    }
}
