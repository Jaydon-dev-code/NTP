using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SL.MLineDataPrecisionTracking.Core.Mqtt;
using System;
using System.Threading.Tasks;

namespace UnitTestProject.OtherPackage
{
    [TestClass]
    public class MqttTest
    {
        private const string BrokerHost = "127.0.0.1";
        private const int BrokerPort = 1883;

        private static readonly string _topicPrefix = $"unit/{Guid.NewGuid():N}";

        private static MqttService CreateService() => new MqttService(BrokerHost, BrokerPort);

        [TestMethod]
        public async Task PublishString_订阅端能收到()
        {
            var topic = $"{_topicPrefix}/string";
            var expected = "hello mqtt " + Guid.NewGuid().ToString("N");
            var service = CreateService();
            var tcs = new TaskCompletionSource<string>();

            await service.SubscribeAsync(topic, payload =>
            {
                tcs.TrySetResult(payload);
                return Task.CompletedTask;
            });

            await service.PublishAsync(topic, expected);

            var actual = await WaitWithTimeout(tcs.Task, 10000);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public async Task PublishObject_自动序列化为JSON()
        {
            // var topic = $"{_topicPrefix}/object";
             var topic = $"A";

            var expected = new { MarkingNo = "ABC123", Value = 1.23 };
            var service = CreateService();
            var tcs = new TaskCompletionSource<string>();

            await service.SubscribeAsync(topic, payload =>
            {
                tcs.TrySetResult(payload);
                return Task.CompletedTask;
            });

            await service.PublishAsync(topic, expected);

            var json = await WaitWithTimeout(tcs.Task, 10000);
            dynamic actual = JsonConvert.DeserializeObject(json);

            Assert.AreEqual(expected.MarkingNo, (string)actual.MarkingNo);
            Assert.AreEqual(expected.Value, (double)actual.Value);
        }

        [TestMethod]
        public async Task Connect_应成功建立连接()
        {
            var service = CreateService();

            await service.ConnectAsync();

            Assert.IsTrue(service.IsConnected);
        }

        private static async Task<T> WaitWithTimeout<T>(Task<T> task, int millisecondsTimeout)
        {
            var timeout = Task.Delay(millisecondsTimeout);
            var completed = await Task.WhenAny(task, timeout);
            if (completed == timeout)
                throw new TimeoutException($"操作超时 ({millisecondsTimeout}ms)");
            return await task;
        }
    }
}
