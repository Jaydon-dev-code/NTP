using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Core.Mqtt
{
    /// <summary>
    /// 产线采集数据 MQTT 推送：按配置键读取 MqttPushConfig 配置，
    /// 开启时把插入的行数据以 JSON 推送到配置的 Topic。
    /// 配置有 5 秒本地缓存，页面上修改保存后最多 5 秒生效。
    /// </summary>
    public class MqttPushService
    {
        private static readonly TimeSpan _configCacheTtl = TimeSpan.FromSeconds(5);
        private static readonly ConcurrentDictionary<string, (DateTime LoadTime, Tb_MqttPushConfig Config)>
            _configCache =
                new ConcurrentDictionary<string, (DateTime, Tb_MqttPushConfig)>();

        private static readonly ConcurrentDictionary<string, MqttService> _mqttClients =
            new ConcurrentDictionary<string, MqttService>();

        private readonly Tb_MqttPushConfigRepository _mqttPushConfigRepository;

        public MqttPushService(Tb_MqttPushConfigRepository mqttPushConfigRepository)
        {
            _mqttPushConfigRepository = mqttPushConfigRepository;
        }

        /// <summary>
        /// 后台推送（不抛出异常、不阻塞采集流程）。
        /// </summary>
        public void PublishSafe(string configKey, object data)
        {
            if (string.IsNullOrWhiteSpace(configKey) || data == null)
                return;

            _ = Task.Run(async () =>
            {
                try
                {
                    await PublishAsync(configKey, data);
                }
                catch (Exception ex)
                {
                    Serilog.Log.Warning(
                        "[MQTT推送]【{ConfigKey}】推送失败：{Message}",
                        configKey,
                        ex.Message
                    );
                }
            });
        }

        private async Task PublishAsync(string configKey, object data)
        {
            var config = await GetConfigAsync(configKey);
            if (config == null || !config.IsEnabled)
                return;

            if (string.IsNullOrWhiteSpace(config.Host))
            {
                Serilog.Log.Warning("[MQTT推送]【{ConfigKey}】已开启但未配置服务器地址", configKey);
                return;
            }

            var port = config.Port <= 0 ? 1883 : config.Port;
            var topic = string.IsNullOrWhiteSpace(config.Topic) ? configKey : config.Topic;
            var cacheKey =
                $"{config.Host}:{port}:{config.Username}:{config.Password}:{configKey}";

            var mqttClient = _mqttClients.GetOrAdd(
                cacheKey,
                _ =>
                    new MqttService(
                        config.Host,
                        port,
                        config.Username,
                        config.Password,
                        $"LineDataPush_{configKey}"
                    )
            );

            await mqttClient.PublishAsync(topic, data);
            Serilog.Log.Information(
                "[MQTT推送]【{ConfigKey}】已推送数据到 {Topic}",
                configKey,
                topic
            );
        }

        /// <summary>
        /// 读取配置（带 5 秒缓存）。
        /// </summary>
        private async Task<Tb_MqttPushConfig> GetConfigAsync(string configKey)
        {
            if (
                _configCache.TryGetValue(configKey, out var cached)
                && DateTime.Now - cached.LoadTime < _configCacheTtl
            )
            {
                return cached.Config;
            }

            Tb_MqttPushConfig config = null;
            try
            {
                config = await _mqttPushConfigRepository.GetByKeyAsync(configKey);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(
                    "[MQTT推送]【{ConfigKey}】读取配置失败：{Message}",
                    configKey,
                    ex.Message
                );
            }

            _configCache[configKey] = (DateTime.Now, config);
            return config;
        }

        /// <summary>
        /// 保存后调用，立即失效缓存，让新配置马上生效。
        /// </summary>
        public static void InvalidateConfigCache(string configKey)
        {
            if (!string.IsNullOrWhiteSpace(configKey))
            {
                _configCache.TryRemove(configKey, out _);
            }
        }
    }
}
