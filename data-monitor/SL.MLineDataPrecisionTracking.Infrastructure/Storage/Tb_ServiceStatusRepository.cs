using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    /// <summary>
    /// 服务状态仓储类
    /// </summary>
    public class Tb_ServiceStatusRepository : BaseRepository<Tb_ServiceStatus>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        public Tb_ServiceStatusRepository(ISqlSugarClient dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// 根据服务ID获取服务状态
        /// </summary>
        /// <param name="serviceId">服务ID</param>
        /// <returns>服务状态实体</returns>
        public async Task<Tb_ServiceStatus> GetByServiceIdAsync(string serviceId)
        {
            return await _db.Queryable<Tb_ServiceStatus>().FirstAsync(x => x.ServiceId == serviceId);
        }

        /// <summary>
        /// 获取所有服务状态
        /// </summary>
        /// <returns>服务状态列表</returns>
        public async Task<List<Tb_ServiceStatus>> GetAllServiceStatusAsync()
        {
            return await _db.Queryable<Tb_ServiceStatus>().Where(x=>true).ToListAsync();
        }


        /// <summary>
        /// 批量更新服务状态
        /// </summary>
        /// <param name="serviceStatuses">服务状态列表</param>
        /// <returns>是否更新成功</returns>
        public async Task<bool> UpdateAsync(List<Tb_ServiceStatus> serviceStatuses)
        {
            return await _db.Updateable<Tb_ServiceStatus>(serviceStatuses).ExecuteCommandAsync() > 0;
        }
        public async Task<bool> UpdateByServiceIdAsync(List<Tb_ServiceStatus> serviceStatuses)
        {
            return await _db.Updateable<Tb_ServiceStatus>(serviceStatuses).WhereColumns(x=>new { x.ServiceId}).ExecuteCommandAsync() > 0;
        }
        /// <summary>
        /// 批量更新服务状态
        /// </summary>
        /// <param name="serviceStatuses">服务状态列表</param>
        /// <returns>是否更新成功</returns>
        public async Task<bool> UpdateAsync(Tb_ServiceStatus serviceStatuses)
        {
            return await _db.Updateable<Tb_ServiceStatus>(serviceStatuses).ExecuteCommandAsync() > 0;
        }
        public async Task<bool> UpdateByServiceIdAsync(Tb_ServiceStatus serviceStatuses)
        {
            return await _db.Updateable<Tb_ServiceStatus>(serviceStatuses).WhereColumns(x => new { x.ServiceId }).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// 初始化服务状态记录
        /// </summary>
        /// <param name="serviceStatuses">服务状态列表</param>
        /// <returns>是否初始化成功</returns>
        public async Task<bool> InitServiceStatusAsync(List<Tb_ServiceStatus> serviceStatuses)
        {
            var existingStatuses = await GetAllServiceStatusAsync();
            var existingIds = existingStatuses.Select(x => x.ServiceId).ToHashSet();

            var newStatuses = serviceStatuses.Where(x => !existingIds.Contains(x.ServiceId)).ToList();
            if (newStatuses.Any())
            {
                return await _db.Insertable< Tb_ServiceStatus >(newStatuses).ExecuteCommandAsync()>0;
            }
            return true;
        }
    }
}