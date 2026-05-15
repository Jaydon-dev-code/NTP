using Autofac;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Core.Hubs;
using SL.MLineDataPrecisionTracking.Models.Domain;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Configuration;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace UnitTestProject
{
    [TestClass]
    public class HubTest
    {
        private static IDisposable _webApp;
        private HubClien _hubClien;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            var baseUrl = ConfigurationManager.AppSettings["ServiceUri"];
            _webApp = WebApp.Start<SignalRTestStartup>(baseUrl);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _webApp?.Dispose();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _hubClien?.Stop();
        }

        private static async Task<T> WaitWithTimeout<T>(Task<T> task, int millisecondsTimeout)
        {
            var timeout = Task.Delay(millisecondsTimeout);
            var completed = await Task.WhenAny(task, timeout);
            if (completed == timeout)
                throw new TimeoutException($"操作超时 ({millisecondsTimeout}ms)");
            return await task;
        }

        [TestMethod]
        public async Task HubSendClient_ShouldReceiveData()
        {
            var tcs = new TaskCompletionSource<ScanRecord>();
            _hubClien = new HubClien();

            _hubClien.Start<ScanRecord>("ScanRecord", data =>
            {
                tcs.TrySetResult(data);
            });

            var ctx = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            var expected = new ScanRecord { IsHave = true, MarkingNo = "ABC123" };
            ctx.Clients.All.ScanRecord(expected);

            var actual = await WaitWithTimeout(tcs.Task, 5000);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.MarkingNo, actual.MarkingNo);
            Assert.AreEqual(expected.IsHave, actual.IsHave);
        }

        [TestMethod]
        public async Task HubSendClient_ShouldReceiveMultipleMessages()
        {
            var count = 0;
            var total = 3;
            var tcs = new TaskCompletionSource<bool>();

            _hubClien = new HubClien();
            _hubClien.Start<ScanRecord>("ScanRecord", data =>
            {
                if (Interlocked.Increment(ref count) == total)
                    tcs.TrySetResult(true);
            });

            var ctx = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            for (int i = 0; i < total; i++)
            {
                ctx.Clients.All.ScanRecord(new ScanRecord { MarkingNo = $"MSG-{i}", IsHave = i % 2 == 0 });
                await Task.Delay(100);
            }

            var received = await WaitWithTimeout(tcs.Task, 5000);
            Assert.IsTrue(received, $"应收到 {total} 条，实际收到 {count} 条");
            Assert.AreEqual(total, count);
        }

        [TestMethod]
        public async Task HubSendClient_ShouldReceiveDifferentModels()
        {
            var tcs = new TaskCompletionSource<ScanRecord>();

            _hubClien = new HubClien();
            _hubClien.Start<ScanRecord>("ScanRecord", data =>
            {
                tcs.TrySetResult(data);
            });

            var ctx = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            var expected = new ScanRecord { IsHave = false, MarkingNo = "XYZ-999" };
            ctx.Clients.All.ScanRecord(expected);

            var actual = await WaitWithTimeout(tcs.Task, 5000);

            Assert.IsNotNull(actual);
            Assert.AreEqual("XYZ-999", actual.MarkingNo);
            Assert.IsFalse(actual.IsHave);
        }

        private class SignalRTestStartup
        {
            public void Configuration(IAppBuilder app)
            {
                app.MapSignalR();
            }
        }
    }

    [TestClass]
    public class HubIocTest
    {
        private static IContainer _container;
        private static IDisposable _webApp;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            var baseUrl = ConfigurationManager.AppSettings["ServiceUri"];
            _webApp = WebApp.Start<SignalRTestStartup>(baseUrl);

            var builder = new ContainerBuilder();
            builder.AddCoreMiddleware();
            _container = builder.Build();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _container?.Dispose();
            _webApp?.Dispose();
        }

        [TestMethod]
        public void 容器应能解析出IHubContext()
        {
            var hubContext = _container.Resolve<IHubContext>();
            Assert.IsNotNull(hubContext);
        }

        [TestMethod]
        public void 两次解析应返回同一实例()
        {
            var first = _container.Resolve<IHubContext>();
            var second = _container.Resolve<IHubContext>();
            Assert.AreSame(first, second);
        }

        [TestMethod]
        public async Task 从容器取出HubContext发布_客户端应收到()
        {
            var hubContext = _container.Resolve<IHubContext>();
            var tcs = new TaskCompletionSource<ScanRecord>();
            var client = new HubClien();

            client.Start<ScanRecord>("ScanRecord", data =>
            {
                tcs.TrySetResult(data);
            });

            var expected = new ScanRecord { IsHave = true, MarkingNo = "IOC-PUB" };
            hubContext.Clients.All.ScanRecord(expected);

            var timeout = Task.Delay(5000);
            var completed = await Task.WhenAny(tcs.Task, timeout);
            Assert.IsTrue(completed == tcs.Task, "超时：客户端未收到消息");

            var actual = await tcs.Task;
            Assert.AreEqual(expected.MarkingNo, actual.MarkingNo);
            Assert.AreEqual(expected.IsHave, actual.IsHave);

            client.Stop();
        }

        private class SignalRTestStartup
        {
            public void Configuration(IAppBuilder app)
            {
                app.MapSignalR();
            }
        }
    }
}
