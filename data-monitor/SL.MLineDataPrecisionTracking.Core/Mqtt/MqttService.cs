using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace SL.MLineDataPrecisionTracking.Core.Mqtt
{
    /// <summary>
    /// MQTT 消息服务，封装基于 MQTTnet 的连接与发布。
    /// 配置项(App.config / Web.config 的 appSettings)：
    ///   MqttHost / MqttPort / MqttUsername / MqttPassword / MqttClientId
    /// </summary>
    public class MqttService
    {
        private static readonly Lazy<MqttService> _instance = new Lazy<MqttService>(() =>
            new MqttService()
        );

        private readonly SemaphoreSlim _connectLock = new SemaphoreSlim(1, 1);
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _clientId;

        private IMqttClient _client;

        /// <summary>单例实例。</summary>
        public static MqttService Instance => _instance.Value;

        /// <summary>当前是否已连接。</summary>
        public bool IsConnected => _client != null && _client.IsConnected;

        public MqttService()
            : this(
                ConfigurationManager.AppSettings["MqttHost"],
                int.TryParse(ConfigurationManager.AppSettings["MqttPort"], out var port)
                    ? port
                    : 1883,
                ConfigurationManager.AppSettings["MqttUsername"],
                ConfigurationManager.AppSettings["MqttPassword"],
                ConfigurationManager.AppSettings["MqttClientId"]
            ) { }

        /// <summary>以显式参数创建实例，便于测试或运行时动态指定。</summary>
        public MqttService(
            string host,
            int port,
            string username = null,
            string password = null,
            string clientId = null
        )
        {
            _host = string.IsNullOrWhiteSpace(host) ? "127.0.0.1" : host;
            _port = port;
            _username = username;
            _password = password;
            _clientId = string.IsNullOrWhiteSpace(clientId)
                ? $"MqttClient_{Guid.NewGuid():N}"
                : clientId;
        }

        /// <summary>建立连接；若已连接则直接返回。重复调用安全。</summary>
        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;

            await _connectLock.WaitAsync();
            try
            {
                if (IsConnected)
                    return;

                _client?.Dispose();
                _client = new MqttFactory().CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(_host, _port)
                    .WithClientId(_clientId)
                    .WithCleanSession()
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(30));

                if (!string.IsNullOrEmpty(_username))
                    options.WithCredentials(_username, _password);

                var client = _client;
                client.ConnectedAsync += e =>
                {
                    Console.WriteLine($"[MQTT] 已连接 {_host}:{_port}");
                    return Task.CompletedTask;
                };
                client.DisconnectedAsync += e =>
                {
                    Console.WriteLine($"[MQTT] 连接断开：{e.Reason}");
                    return Task.CompletedTask;
                };

                await client.ConnectAsync(options.Build(), CancellationToken.None);
            }
            finally
            {
                _connectLock.Release();
            }
        }

        /// <summary>断开连接。</summary>
        public async Task DisconnectAsync()
        {
            if (_client == null)
                return;
            if (_client.IsConnected)
                await _client.DisconnectAsync();
            _client?.Dispose();
            _client = null;
        }

        /// <summary>发布字符串消息。</summary>
        public Task PublishAsync(
            string topic,
            string payload,
            bool retain = false,
            MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce
        )
        {
            return PublishAsync(
                topic,
                payload == null ? null : Encoding.UTF8.GetBytes(payload),
                retain,
                qos
            );
        }

        /// <summary>发布对象，自动序列化为 JSON 字符串。</summary>
        public Task PublishAsync(
            string topic,
            object payload,
            bool retain = false,
            MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce
        )
        {
            return PublishAsync(
                topic,
                payload == null ? null : JsonConvert.SerializeObject(payload),
                retain,
                qos
            );
        }

        /// <summary>发布字节消息。</summary>
        public async Task PublishAsync(
            string topic,
            byte[] payload,
            bool retain = false,
            MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce
        )
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("topic 不能为空", nameof(topic));

            await ConnectAsync();

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag(retain)
                .WithQualityOfServiceLevel(qos)
                .Build();

            await _client.PublishAsync(message, CancellationToken.None);
        }

        /// <summary>订阅主题。</summary>
        public Task SubscribeAsync(string topic, Func<string, Task> onMessage, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("topic 不能为空", nameof(topic));

            return SubscribeAsync(new[] { topic }, (t, payload) => onMessage(payload), qos);
        }

        /// <summary>订阅多个主题。</summary>
        public async Task SubscribeAsync(string[] topics, Func<string, string, Task> onMessage, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce)
        {
            if (topics == null || topics.Length == 0)
                throw new ArgumentException("topics 不能为空", nameof(topics));

            await ConnectAsync();

            foreach (var topic in topics)
            {
                var filter = new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel(qos)
                    .Build();

                _client.ApplicationMessageReceivedAsync += async e =>
                {
                    if (e.ApplicationMessage.Topic != topic)
                        return;
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment.Array, e.ApplicationMessage.PayloadSegment.Offset, e.ApplicationMessage.PayloadSegment.Count);
                    await onMessage(topic, payload);
                };

                await _client.SubscribeAsync(filter, CancellationToken.None);
            }
        }

        /// <summary>释放资源。</summary>
        public void Dispose()
        {
            _client?.Dispose();
            _client = null;
        }
    }
}
