using System;
using System.Linq;

namespace SL.MLineDataPrecisionTracking.Models.Domain.Mqtt
{
    /// <summary>
    /// MQTT 发布配置 — 注册设备采集服务时传入。
    /// Topic 未指定时按 厂标识 + 产线标识 拼接（如 FactoryCode/LineCode）。
    /// </summary>
    public class MqttPublishConfig
    {
        /// <summary>MQTT服务器IP</summary>
        public string Host { get; set; }

        /// <summary>MQTT端口</summary>
        public int Port { get; set; } = 1883;

        /// <summary>用户名（可空）</summary>
        public string Username { get; set; }

        /// <summary>密码（可空）</summary>
        public string Password { get; set; }

        /// <summary>Topic 前缀/完整模板；为空时按 厂标识/产线标识 自动拼接</summary>
        public string Topic { get; set; }

        /// <summary>
        /// 生成发布 Topic：优先用显式 Topic；否则拼接 厂标识/产线标识。
        /// </summary>
        public string BuildTopic(string factoryCode, string lineCode, string suffix = null)
        {
            var topic = Topic;
            if (string.IsNullOrWhiteSpace(topic))
            {
                topic = string.Join(
                    "/",
                    new[] { factoryCode, lineCode }.Where(x => !string.IsNullOrWhiteSpace(x))
                );
            }
            if (!string.IsNullOrWhiteSpace(suffix))
            {
                topic = string.IsNullOrWhiteSpace(topic)
                    ? suffix
                    : $"{topic}/{suffix}";
            }
            return topic;
        }
    }
}
