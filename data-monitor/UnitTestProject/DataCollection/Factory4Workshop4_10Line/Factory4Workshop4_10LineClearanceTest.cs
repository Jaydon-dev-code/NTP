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
    public class Factory4Workshop4_10LineClearanceTest
    {
        private IContainer Container { get; set; }
        private MelsecMcServer _server;

        public Factory4Workshop4_10LineClearanceTest()
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
            _server.ServerStart(4005);
        }

        [TestMethod]
        public async Task A_DataCollectionDebugging()
        {
            var service = (Factory4Workshop4_10Line_Clearance)
                Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_Clearance));

            service.Start();
            await Task.Delay(200 * 1000);
        }

        [TestMethod]
        public async Task A_DataCollectionTest()
        {
            await Task.Delay(1000);

            var mcp = Container.Resolve<McpCommunication>();
            var service = (Factory4Workshop4_10Line_Clearance)
                Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_Clearance));

            service.Start();

            var lineInfo = await InitPlcAddre("四分厂4-10-游隙");

            var clearance1OK = lineInfo.First(x => x.PointName == "游隙1工位OK");
            var clearance1NG = lineInfo.First(x => x.PointName == "游隙1工位NG");
            var clearance1SN = lineInfo.First(x => x.PointName == "游隙1工位SN");
            var positiveGap = lineInfo.First(x => x.PointName == "正间隙");
            var lowerLoad = lineInfo.First(x => x.PointName == "下负荷");
            var upperLoad = lineInfo.First(x => x.PointName == "上负荷");

            var clearance2OK = lineInfo.First(x => x.PointName == "游隙2工位OK");
            var clearance2NG = lineInfo.First(x => x.PointName == "游隙2工位NG");
            var clearance2SN = lineInfo.First(x => x.PointName == "游隙2工位SN");
            var offset = lineInfo.First(x => x.PointName == "偏移量");
            var beforePressIn = lineInfo.First(x => x.PointName == "压入前");
            var afterPressIn = lineInfo.First(x => x.PointName == "压入后");
            var gap = lineInfo.First(x => x.PointName == "间隙");

            int num = 2000;

            for (int i = 0; i < 5; i++)
            {
                string sn = $"TEST_{i}";
                _server.Write(clearance1SN.Prefix + clearance1SN.Address, sn);
                _server.Write(positiveGap.Prefix + positiveGap.Address, num++);
                _server.Write(lowerLoad.Prefix + lowerLoad.Address, num++);
                _server.Write(upperLoad.Prefix + upperLoad.Address, num++);

                _server.Write(clearance1OK.Prefix + clearance1OK.Address, true);
                _server.Write(clearance1NG.Prefix + clearance1NG.Address, false);

                await Task.Delay(3 * 1000);

                _server.Write(clearance1OK.Prefix + clearance1OK.Address, false);
                _server.Write(clearance1NG.Prefix + clearance1NG.Address, false);
                await Task.Delay(3 * 1000);
            }



            for (int i = 0; i < 5; i++)
            {
                string sn = $"TEST_{i}";

                _server.Write(clearance2SN.Prefix + clearance2SN.Address, sn);

                _server.Write(offset.Prefix + offset.Address, num++);
                _server.Write(beforePressIn.Prefix + beforePressIn.Address, num++);
                _server.Write(afterPressIn.Prefix + afterPressIn.Address, num++);
                _server.Write(gap.Prefix + gap.Address, num++);

                _server.Write(clearance2OK.Prefix + clearance2OK.Address, true);
                _server.Write(clearance2NG.Prefix + clearance2NG.Address, false);
                await Task.Delay(3* 1000);

                _server.Write(clearance2OK.Prefix + clearance2OK.Address, false);
                _server.Write(clearance2NG.Prefix + clearance2NG.Address, false);
                await Task.Delay(3 * 1000);
            }

            await Task.Delay(5*1000);
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
