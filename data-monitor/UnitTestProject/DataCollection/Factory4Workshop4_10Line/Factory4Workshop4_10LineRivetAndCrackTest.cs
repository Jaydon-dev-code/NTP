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

using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SqlSugar.Extensions;

namespace UnitTestProject
{
    [TestClass]
    public class Factory4Workshop4_10LineRivetAndCrackTest
    {
        private IContainer Container { get; set; }
        private MelsecMcServer _server;

        public Factory4Workshop4_10LineRivetAndCrackTest()
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
            _server.ServerStart(7001);
        }

        [TestMethod]
        public async Task A_DataCollectionDebugging()
        {
            var service = (Factory4Workshop4_10Line_RivetAndCrack)
                Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_RivetAndCrack));

            service.Start();
            await Task.Delay(200 * 1000);
        }

        [TestMethod]
        public async Task A_DataCollectionTest()
        {
            await Task.Delay(1000);

            var mcp = Container.Resolve<McpCommunication>();
            var service = (Factory4Workshop4_10Line_RivetAndCrack)
                Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .First(x => x.GetType().Name == nameof(Factory4Workshop4_10Line_RivetAndCrack));

            service.Start();

            var lineInfo = await InitPlcAddre("四分厂4-10-铆接和裂纹");

            var rivetingOK = lineInfo.First(x => x.PointName == "铆接OK");
            var rivetingNG = lineInfo.First(x => x.PointName == "铆接NG");
            var rivetingSN = lineInfo.First(x => x.PointName == "铆接SN");
            var rivetingInspection1Height = lineInfo.First(x => x.PointName == "铆接检测1高度");
            var RivetingInspectionFormingHeight = lineInfo.First(x => x.PointName == "铆接成型高度");
            var rivetingInspection2Height = lineInfo.First(x => x.PointName == "铆接检测2高度");

            var spinRivetingOK = lineInfo.First(x => x.PointName == "旋铆检测OK");
            var spinRivetingNG = lineInfo.First(x => x.PointName == "旋铆检测NG");
            var spinRivetingSN = lineInfo.First(x => x.PointName == "旋铆检测SN");
        

            int num = 2000;

            for (int i = 0; i < 5; i++)
            {
                string sn = $"TEST_{i}";
                _server.Write(rivetingSN.Prefix + rivetingSN.Address, sn);
                _server.Write(rivetingInspection1Height.Prefix + rivetingInspection1Height.Address, num++);
                _server.Write(RivetingInspectionFormingHeight.Prefix + RivetingInspectionFormingHeight.Address, num++);
                _server.Write(rivetingInspection2Height.Prefix + rivetingInspection2Height.Address, num++);
                _server.Write(rivetingOK.Prefix + rivetingOK.Address, true);
                _server.Write(rivetingNG.Prefix + rivetingNG.Address, false);

                await Task.Delay(3 * 1000);

                _server.Write(rivetingOK.Prefix + rivetingOK.Address, false);
                _server.Write(rivetingNG.Prefix + rivetingNG.Address, false);
                await Task.Delay(3 * 1000);
            }

            for (int i = 0; i < 5; i++)
            {
                string sn = $"TEST_{i}";
                _server.Write(spinRivetingSN.Prefix + spinRivetingSN.Address, sn);
               

                _server.Write(spinRivetingOK.Prefix + spinRivetingOK.Address, true);
                _server.Write(spinRivetingNG.Prefix + spinRivetingNG.Address, false);

                await Task.Delay(3 * 1000);

                _server.Write(spinRivetingOK.Prefix + spinRivetingOK.Address, false);
                _server.Write(spinRivetingNG.Prefix + spinRivetingNG.Address, false);
                await Task.Delay(3 * 1000);
            }

            await Task.Delay(5 * 1000);
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
