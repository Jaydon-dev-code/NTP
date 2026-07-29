using System;
using System.Collections.Generic;
using System.Linq;
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
using SqlSugar.Extensions;

namespace UnitTestProject
{
    [TestClass]
    public class Factory4Workshop4_10LineABSTest
    {
        private IContainer Container { get; set; }
        private MelsecMcServer _server;

        public Factory4Workshop4_10LineABSTest()
        {
            var builder = new ContainerBuilder();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();
            Container = builder.Build();

            _server = new MelsecMcServer
            {
                IsBinary = true,
                AnalysisLogMessage = true,
                ActiveTimeSpan = TimeSpan.Parse("01:00:00"),
                EnableIPv6 = false,
            };
            _server.ServerStart(3005);
        }

        [TestMethod]
        public async Task A_DataCollectionDebugging()
        {
            var service = (Factory4Workshop4_10Line_ABS)
                Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_ABS));

            service.Start();
            await Task.Delay(200 * 1000);
        }

        [TestMethod]
        public async Task A_DataCollectionTest()
        {
            await Task.Delay(1000);

            var mcp = Container.Resolve<McpCommunication>();
            var service = (Factory4Workshop4_10Line_ABS)
                Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_ABS));

            service.Start();

            var lineInfo = await InitPlcAddre("四分厂4-10-ABS");

            var pressDownOK = lineInfo.First(x => x.PointName == "ABS压紧OK");
            var pressDownNG = lineInfo.First(x => x.PointName == "ABS压紧NG");
            var pressDownSN = lineInfo.First(x => x.PointName == "ABS压紧SN");

            var checkOK = lineInfo.First(x => x.PointName == "ABS检测OK");
            var checkNG = lineInfo.First(x => x.PointName == "ABS检测NG");
            var checkSN = lineInfo.First(x => x.PointName == "ABS检测SN");

            for (int i = 0; i < 5; i++)
            {
                string sn = $"TEST_{i}";
                _server.Write(pressDownSN.Prefix + pressDownSN.Address, sn);

                _server.Write(pressDownOK.Prefix + pressDownOK.Address, true);
                _server.Write(pressDownNG.Prefix + pressDownNG.Address, false);

                await Task.Delay(3 * 1000);

                _server.Write(pressDownOK.Prefix + pressDownOK.Address, false);
                _server.Write(pressDownNG.Prefix + pressDownNG.Address, false);
                await Task.Delay(3 * 1000);
            }

            for (int i = 0; i < 5; i++)
            {
                string sn = $"TEST_{i}";
                _server.Write(checkSN.Prefix + checkSN.Address, sn);

                _server.Write(checkOK.Prefix + checkOK.Address, true);
                _server.Write(checkNG.Prefix + checkNG.Address, false);

                await Task.Delay(3 * 1000);

                _server.Write(checkOK.Prefix + checkOK.Address, false);
                _server.Write(checkNG.Prefix + checkNG.Address, false);
                await Task.Delay(3 * 1000);
            }

            await Task.Delay(10 * 100000);
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
