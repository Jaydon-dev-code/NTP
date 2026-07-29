using Autofac;
using HslCommunication.Profinet.Melsec;
using ICSharpCode.SharpZipLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScottPlot.Plottables;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory4Workshop4_10;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestProject.DataCollection
{
    [TestClass]
    public class Factory6Workshop4_10Test
    {
        private IContainer Container { get; set; }
        private List<MelsecMcServer> _servers;
        MelsecA1EServer _melsecA1EServer;
        public Factory6Workshop4_10Test()
        {
            var builder = new ContainerBuilder();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();
            Container = builder.Build();
            _servers = new List<MelsecMcServer>();
            foreach (var port in new int[] { 3005, 7001, 9000 })
            {
                var server = new MelsecMcServer
                {
                    IsBinary = true,
                    AnalysisLogMessage = true,
                    ActiveTimeSpan = TimeSpan.Parse("01:00:00"),
                    EnableIPv6 = false,
                };
                server.ServerStart(port);
                _servers.Add(server);
            }
            _melsecA1EServer = new MelsecA1EServer()
            {
                IsBinary = true,
                AnalysisLogMessage = true,
                ActiveTimeSpan = TimeSpan.Parse("01:00:00"),
                EnableIPv6 = false,
            };
            _melsecA1EServer.ServerStart(6000);
        }

        #region Debug
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

        [TestMethod]
        public async Task Vib_Debugging()
        {
            var service = (Factory4Workshop4_10Line_Vib)
                Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_Vib));
            service.Start();
            await Task.Delay(200 * 1000);
        }

        [TestMethod]
        public async Task RivetAndCrack_Debugging()
        {
            var service = (Factory4Workshop4_10Line_RivetAndCrack)
                Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_RivetAndCrack));
            service.Start();
            await Task.Delay(200 * 1000);
        }

        [TestMethod]
        public async Task Clearance_Debugging()
        {
            var service = (Factory4Workshop4_10Line_Clearance)
                Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_Clearance));
            service.Start();
            await Task.Delay(200 * 1000);
        }

        [TestMethod]
        public async Task ABS_Debugging()
        {
            var service = (Factory4Workshop4_10Line_ABS)
                Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_ABS));
            service.Start();
            await Task.Delay(200 * 1000);
        }
        #endregion


        #region Test
        [TestMethod]
        public async void ClearanceTest()
        {
            var service = (Factory4Workshop4_10Line_Clearance)
             Container
                 .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                 .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_Vib));
            service.Start();

        }

        #endregion
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
