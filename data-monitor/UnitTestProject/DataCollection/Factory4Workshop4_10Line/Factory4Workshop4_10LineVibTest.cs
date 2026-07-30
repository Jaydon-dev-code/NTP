using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using HslCommunication.Profinet.Melsec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory4Workshop4_10;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SqlSugar;
using SqlSugar.Extensions;

namespace UnitTestProject
{
    [TestClass]
    public class Factory4Workshop4_10LineVibTest
    {
        private IContainer Container { get; set; }
        private HslCommunication.Profinet.Melsec.MelsecA1EServer _server;

        public Factory4Workshop4_10LineVibTest()
        {
            var builder = new ContainerBuilder();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();
            Container = builder.Build();

            _server = new MelsecA1EServer
            {
                IsBinary = true,
                AnalysisLogMessage = true,
                ActiveTimeSpan = TimeSpan.Parse("01:00:00"),
                EnableIPv6 = false,
            };
            _server.ServerStart(9000);
        }

        [TestMethod]
        public async Task A_DataCollectionDebugging()
        {
            var service = (Factory4Workshop4_10Line_Vib)
                Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_Vib));

            service.Start();
            await Task.Delay(200 * 1000);
        }

        [TestMethod]
        public async Task A_DataCollectionTest()
        {
            await Task.Delay(1000);

            var mcp = Container.Resolve<McpCommunication>();
            var service = (Factory4Workshop4_10Line_Vib)
                Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_Vib));

            service.Start();

            string scannerIp = "127.0.0.01";
            int port = 9004;

            try
            {
                TcpClient tcp = new TcpClient();
                tcp.Connect(scannerIp, port);
                NetworkStream stream = tcp.GetStream();

                var lineInfo = await InitPlcAddre("四分厂4-10-震动");

                var vibOK = lineInfo.First(x => x.PointName == "震动OK");
                var vibNG = lineInfo.First(x => x.PointName == "震动NG");
                var vibSN = lineInfo.First(x => x.PointName == "震动SN");
                var vibIussicSN = lineInfo.First(x => x.PointName == "下发震动SN");
                Task.Run(async () =>
                {
                    string lastSN = string.Empty;
                    while (true)
                    {
                        var sn = _server.ReadString(
                            vibIussicSN.Prefix + vibIussicSN.Address,
                            (ushort)vibIussicSN.Length
                        );
                        var str = sn.Content.Replace('\0', ' ').Trim();
                        if (string.IsNullOrEmpty(str))
                        {
                            await Task.Delay(1000 / 2);
                            continue;
                        }
                        if (lastSN != str)
                        {
                            _server.Write(vibSN.Prefix + vibSN.Address, str);
                            lastSN = str;
                        }
                        await Task.Delay(1000 / 2);
                    }
                });
                for (int i = 0; i < 5; i++)
                {
                    string sn = $"TEST_{i}";
                    // 发送读取指令

                    byte[] cmd = Encoding.ASCII.GetBytes("0010" + sn);
                    stream.Write(cmd, 0, cmd.Length);
                    await Task.Delay(2 * 1000);
                    //byte[] buf = new byte[1024];
                    //int len = stream.Read(buf, 0, buf.Length);
                    _server.Write(vibOK.Prefix + vibOK.Address, true);
                    _server.Write(vibNG.Prefix + vibNG.Address, false);

                    await Task.Delay(5 * 1000);

                    _server.Write(vibOK.Prefix + vibOK.Address, false);
                    _server.Write(vibNG.Prefix + vibNG.Address, false);
                    await Task.Delay(3 * 1000);
                }
            }
            catch
            {
                Console.WriteLine("连接失败");
            }

            await Task.Delay(10 * 1000);
        }

        private async Task<List<DevPlcPointDto>> InitPlcAddre(string lineName)
        {
            var linePoint = await Container
                .Resolve<Tb_EquipmentRepository>()
                .GetEquipmentAllAsync(x => x.DeviceName == lineName);

            var result = new List<DevPlcPointDto>();
            foreach (var plcConnection in linePoint.PlcConnections)
            {
                foreach (var point in plcConnection.Points)
                {
                    result.Add(
                        new DevPlcPointDto(
                            linePoint.DeviceName,
                            point.PointName,
                            plcConnection.IpAddress,
                            plcConnection.Port,
                            point.Area,
                            point.DataType.ToTypeCode(),
                            point.Address,
                            point.Length,
                            point.ReadFormula,
                            point.WriteFormula
                        )
                    );
                }
            }
            return result;
        }
    }
}
