using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    /// <summary>
    /// PLC连接仓储
    /// </summary>
    public class Tb_PlcConnectionRepository : BaseRepository<Tb_PlcConnection>
    {
        public Tb_PlcConnectionRepository(ISqlSugarClient db) : base(db)
        {
        }

        public async Task<int> InsertableAsync(Tb_PlcConnection device)
        {
            return await _db.Insertable<Tb_PlcConnection>(device).ExecuteCommandAsync();
        }

        public async Task<int> ExecuteReturnIdentityAsync(Tb_PlcConnection device)
        {
            return await _db.Insertable<Tb_PlcConnection>(device).ExecuteReturnIdentityAsync();
        }


        
        public async Task<Tb_PlcConnection> QueryableFirstAsync(Expression<Func<Tb_PlcConnection, bool>> expression)
        {
            return await _db.Queryable<Tb_PlcConnection>().FirstAsync(expression);
        }
      
    }
}
