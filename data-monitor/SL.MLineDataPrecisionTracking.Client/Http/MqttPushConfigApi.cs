using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Client.Http
{
    public class MqttPushConfigApi : BaseHttp
    {
        protected override string _controllerName => "MqttPushConfig";

        /// <summary>
        /// 获取推送配置（服务端在不存在时返回默认配置）
        /// </summary>
        public async Task<ApiResult<Tb_MqttPushConfig>> GetAsync(string configKey)
        {
            return await PostAsync<Tb_MqttPushConfig>("Get", configKey);
        }

        /// <summary>
        /// 保存推送配置
        /// </summary>
        public async Task<ApiResult> SaveAsync(Tb_MqttPushConfig config)
        {
            return await PostAsync("Save", config);
        }
    }
}
