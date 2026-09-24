using System.Collections.Generic;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities.SPC;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage.SPC
{
    public class Tb_SPC_EquipmentRepository : BaseRepository<Tb_SPC_Equipment>
    {
        public Tb_SPC_EquipmentRepository(ISqlSugarClient db) : base(db) { }

        public async Task<List<Tb_SPC_Equipment>> GetListAsync()
        {
            return await _db.Queryable<Tb_SPC_Equipment>().OrderBy(x => x.Id).ToListAsync();
        }

        public async Task<Tb_SPC_Equipment> GetByIdAsync(int id)
        {
            return await _db.Queryable<Tb_SPC_Equipment>().InSingleAsync(id);
        }

        public async Task<int> InsertAsync(Tb_SPC_Equipment entity)
        {
            return await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        }

        public async Task<bool> UpdateAsync(Tb_SPC_Equipment entity)
        {
            return await _db.Updateable(entity).ExecuteCommandHasChangeAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _db.Deleteable<Tb_SPC_Equipment>().In(id).ExecuteCommandHasChangeAsync();
        }
    }
}
