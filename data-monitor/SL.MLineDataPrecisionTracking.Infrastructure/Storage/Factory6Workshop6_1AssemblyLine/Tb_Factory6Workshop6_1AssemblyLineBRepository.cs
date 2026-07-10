using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_1AssemblyLine;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    public class Tb_Factory6Workshop6_1AssemblyLineBRepository : BaseRepository<Tb_Factory6Workshop6_1AssemblyLineB>
    {
        public Tb_Factory6Workshop6_1AssemblyLineBRepository(ISqlSugarClient db) : base(db)
        {
        }
        public async Task<int> InsertableAsync(Tb_Factory6Workshop6_1AssemblyLineB device)
        {
            return await _db.Insertable<Tb_Factory6Workshop6_1AssemblyLineB>(device).ExecuteCommandAsync();
        }
        public async Task<int> InsertableReturnIdentityAsync(Tb_Factory6Workshop6_1AssemblyLineB device)
        {
            return await _db.Insertable<Tb_Factory6Workshop6_1AssemblyLineB>(device).ExecuteReturnIdentityAsync();
        }
        public async Task<int> UpdateableAsync(Tb_Factory6Workshop6_1AssemblyLineB device)
        {
            return await _db.Updateable<Tb_Factory6Workshop6_1AssemblyLineB>(device).ExecuteCommandAsync();
        }
    }
}
