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
    /// 设备仓储
    /// </summary>
    public class Tb_EquipmentRepository : BaseRepository<Tb_Equipment>
    {
        public Tb_EquipmentRepository(ISqlSugarClient db)
            : base(db) { }

        /// <summary>
        /// 查询设备 + 所有PLC + 所有点位（连表）
        /// </summary>
        public async Task<List<Tb_Equipment>> GetEquipmentAllAsync()
        {
            return await _db.Queryable<Tb_Equipment>()
                .Includes(e => e.PlcConnections) // 加载PLC
                .Includes(e => e.PlcConnections.First().Points) // 加载点位
                .ToListAsync();
        }

        public async Task<Tb_Equipment> GetEquipmentAllAsync(
            Expression<Func<Tb_Equipment, bool>> expression
        )
        {
            return await _db.Queryable<Tb_Equipment>()
                .Includes(e => e.PlcConnections) // 加载PLC
                .Includes(e => e.PlcConnections.First().Points)
                .FirstAsync(
                    expression
                ) // 加载点位
            ;
        }

        public async Task<int> InsertableAsync(Tb_Equipment device)
        {
            return await _db.Insertable<Tb_Equipment>(device).ExecuteCommandAsync();
        }

        public async Task<Tb_Equipment> QueryableFirstAsync(
            Expression<Func<Tb_Equipment, bool>> expression
        )
        {
            return await _db.Queryable<Tb_Equipment>().FirstAsync(expression);
        }
    }
}
