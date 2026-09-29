using System;
using System.Threading.Tasks;
using System.Web.Http;
using SL.MLineDataPrecisionTracking.Core.Mqtt;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    /// <summary>
    /// 产线数据 MQTT 推送配置
    /// </summary>
    public class MqttPushConfigController : ApiController
    {
        Tb_MqttPushConfigRepository _mqttPushConfigRepository;

        public MqttPushConfigController(Tb_MqttPushConfigRepository mqttPushConfigRepository)
        {
            _mqttPushConfigRepository = mqttPushConfigRepository;
        }

        /// <summary>
        /// 获取指定配置键的推送配置（不存在时返回默认配置）
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<Tb_MqttPushConfig>> Get([FromBody] string configKey)
        {
            try
            {
                var config = await _mqttPushConfigRepository.GetByKeyAsync(configKey);
                if (config == null)
                {
                    config = new Tb_MqttPushConfig
                    {
                        ConfigKey = configKey,
                        IsEnabled = false,
                        Port = 1883,
                        Topic = configKey,
                    };
                }
                return ApiResult<Tb_MqttPushConfig>.Success(config);
            }
            catch (Exception ex)
            {
                return ApiResult<Tb_MqttPushConfig>.Fail(
                    $"读取MQTT推送配置失败：{ex.Message}（若提示表不存在，请将 Service 配置 IsCreateTable=true 后重启服务建表）"
                );
            }
        }

        /// <summary>
        /// 保存推送配置
        /// </summary>
        [HttpPost]
        public async Task<ApiResult> Save([FromBody] Tb_MqttPushConfig config)
        {
            try
            {
                if (config == null || string.IsNullOrWhiteSpace(config.ConfigKey))
                {
                    return ApiResult.Fail("配置键不能为空");
                }

                if (config.IsEnabled && string.IsNullOrWhiteSpace(config.Host))
                {
                    return ApiResult.Fail("开启推送时必须填写 MQTT 服务器地址");
                }

                if (config.Port <= 0 || config.Port > 65535)
                {
                    config.Port = 1883;
                }

                var ok = await _mqttPushConfigRepository.SaveAsync(config);
                if (!ok)
                {
                    return ApiResult.Fail("保存失败");
                }

                MqttPushService.InvalidateConfigCache(config.ConfigKey);
                return ApiResult.Success("保存成功，推送配置将在数秒内生效");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(
                    $"保存MQTT推送配置失败：{ex.Message}（若提示表不存在，请将 Service 配置 IsCreateTable=true 后重启服务建表）"
                );
            }
        }
    }
}
