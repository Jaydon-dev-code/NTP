using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScottPlot.Plottables;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory4Workshop4_10;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_1;

namespace UnitTestProject.DataCollection
{
    [TestClass]
    public class Factory6Workshop4_10Test
    {
        private IContainer Container { get; set; }

        public Factory6Workshop4_10Test()
        {
            var builder = new ContainerBuilder();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();
            Container = builder.Build();
        }

        [TestMethod]
        public void ScanSoketDebug()
        {
            var service = (Factory4Workshop4_10Line_Vib)
                Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_Vib));
            Task.Run(() =>
            {
                service.ScanSocket();
            });
            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 模拟扫码枪发数据，断电打到 ReceiveBarcodeLoop处查看接收数据
        /// </summary>
        [TestMethod]
        public void ScanSoketTest()
        {
            var service = (Factory4Workshop4_10Line_Vib)
                Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_Vib));
            Task.Run(() =>
            {
                service.ScanSocket();
            });

            TcpClient();
        }

        void TcpClient()
        {
            string scannerIp = "127.0.0.01";
            int port = 9004;

            try
            {
                TcpClient tcp = new TcpClient();
                tcp.Connect(scannerIp, port);
                NetworkStream stream = tcp.GetStream();
                // 发送读取指令
                var soure = "abcd";
                byte[] cmd = Encoding.ASCII.GetBytes("0008" + soure);
                stream.Write(cmd, 0, cmd.Length);

                byte[] buf = new byte[1024];
                int len = stream.Read(buf, 0, buf.Length);
                string bar = Encoding.ASCII.GetString(buf, 0, len).Trim();

                Assert.AreEqual(bar, "TestMsgIs" + soure);
            }
            catch
            {
                Console.WriteLine("连接失败");
            }
        }
    }
}
