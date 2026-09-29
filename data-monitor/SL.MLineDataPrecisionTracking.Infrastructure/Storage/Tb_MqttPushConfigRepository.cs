using System;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    /// <summary>
    /// 产线数据 MQTT 推送配置仓储
    /// </summary>
    public class Tb_MqttPushConfigRepository : BaseRepository<Tb_MqttPushConfig>
    {
        public Tb_MqttPushConfigRepository(ISqlSugarClient dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// 按配置键获取配置，不存在时返回 null
        /// </summary>
        public async Task<Tb_MqttPushConfig> GetByKeyAsync(string configKey)
        {
            if (string.IsNullOrWhiteSpace(configKey))
                return null;

            return await _db
                .Queryable<Tb_MqttPushConfig>()
                .FirstAsync(x => x.ConfigKey == configKey);
        }

        /// <summary>
        /// 保存配置（存在则更新，不存在则插入）
        /// </summary>
        public async Task<bool> SaveAsync(Tb_MqttPushConfig config)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.ConfigKey))
                return false;

            config.UpdateTime = DateTime.Now;

            var exists = await GetByKeyAsync(config.ConfigKey);
            if (exists == null)
            {
                return await _db.Insertable(config).ExecuteCommandAsync() > 0;
            }

            return await _db
                .Updateable(config)
                .WhereColumns(x => new { x.ConfigKey })
                .ExecuteCommandAsync() > 0;
        }
    }
}
