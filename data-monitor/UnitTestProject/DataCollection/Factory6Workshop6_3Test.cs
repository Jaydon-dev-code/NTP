using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using HslCommunication.Profinet.Melsec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_3;

using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar.Extensions;

namespace UnitTestProject.DataCollection
{
    [TestClass]
    public class Factory6Workshop6_3Test
    {
        private IContainer Container { get; set; }
        private List<MelsecMcServer> _servers;

        public Factory6Workshop6_3Test()
        {
            var builder = new ContainerBuilder();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();
            Container = builder.Build();

            _servers = new List<MelsecMcServer>();
            foreach (var port in new int[] { 2000, 6000, 4990 })
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
        }

        #region A线

        [TestMethod]
        public async Task A_DataCollectionDebugging()
        {
            var aService = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(Factory6Workshop6_3AssemblyLineA));

            aService.Start();

            await Task.Delay(200 * 1000);
        }

        [TestMethod]
        public async Task A_DataCollectionTest()
        {
            await Task.Delay(1000);

            var mcp = Container.Resolve<McpCommunication>();
            var aService = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(Factory6Workshop6_3AssemblyLineA));

            aService.Start();

            var lineInfo = await InitPlcAddre("A线");
            var endPoint = lineInfo.FirstOrDefault(x => x.PointName == "采集结束");
            var startPoint = lineInfo.FirstOrDefault(x => x.PointName == "采集开始");
            var trayNoPoint = lineInfo.FirstOrDefault(x => x.PointName == "托盘号A");
            lineInfo.Remove(endPoint);
            lineInfo.Remove(startPoint);
            lineInfo.Remove(trayNoPoint);
            startPoint.Value = new List<object>() { false };
            mcp.Write(startPoint);
            int val = 1;
            for (int i = 0; i < 100; i++)
            {
                trayNoPoint.Value = new List<object>() { val };
                mcp.Write(trayNoPoint);

                foreach (var item in lineInfo)
                {
                    item.Value = new List<object>() { val };
                    if (item.DataType == TypeCode.String)
                    {
                        item.Value = new List<object>() { val.ToString() };
                    }
                    mcp.Write(item);
                    val++;
                }

                startPoint.Value = new List<object>() { true };
                mcp.Write(startPoint);
                await Task.Delay(1000);
            }
        }

        #endregion

        #region B线

        [TestMethod]
        public async Task B_DataCollectionDebugging()
        {
            var bService = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(Factory6Workshop6_3AssemblyLineB));

            bService.Start();

            await Task.Delay(200 * 1000);
        }

        [TestMethod]
        public async Task B_DataCollectionTest()
        {
            await Task.Delay(1000);

            var mcp = Container.Resolve<McpCommunication>();
            var bService = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(Factory6Workshop6_3AssemblyLineB));

            bService.Start();

            var lineInfo = await InitPlcAddre("B线");
            var endPoint = lineInfo.FirstOrDefault(x => x.PointName == "采集结束");
            var startPoint = lineInfo.FirstOrDefault(x => x.PointName == "采集开始");
            var trayNoPoint = lineInfo.FirstOrDefault(x => x.PointName == "托盘号B");
            lineInfo.Remove(endPoint);
            lineInfo.Remove(startPoint);
            lineInfo.Remove(trayNoPoint);

            EndServer(endPoint, startPoint, mcp);

            int val = 1000;
            for (int i = 0; i < 100; i++)
            {
                if ((mcp.Read(startPoint)).Data.Value[0].ObjToBool() is false)
                {
                    trayNoPoint.Value = new List<object>() { val };
                    mcp.Write(trayNoPoint);

                    foreach (var item in lineInfo)
                    {
                        item.Value = new List<object>() { val };
                        if (item.DataType == TypeCode.String)
                        {
                            item.Value = new List<object>() { val.ToString() };
                        }
                        mcp.Write(item);
                        val++;
                    }

                    startPoint.Value = new List<object>() { true };
                    mcp.Write(startPoint);
                }
                else
                {
                    i--;
                    await Task.Delay(200);
                }
            }
        }

        #endregion

        #region 联动测试

        [TestMethod]
        public async Task DataCollectionTest()
        {
            await Task.Delay(1000);

            var aService = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(Factory6Workshop6_3AssemblyLineA));

            var bService = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(Factory6Workshop6_3AssemblyLineB));

            int[] aTrayNos = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int[] bTrayNos = new int[] { 10, 20, 30, 40, 50, 60, 70, 80, 90 };

            var mcp = Container.Resolve<McpCommunication>();

            Task aTask = RunAsync("A线", aService, aTrayNos);
            await Task.Delay(5000);
            Task bTask = RunAsync(
                "B线",
                bService,
                bTrayNos,
                async (lineInfo, index) =>
                {
                    await Task.Delay(100);
                    var aTrayNoPoint = lineInfo.FirstOrDefault(x => x.PointName == "A线托盘编号");
                    try
                    {
                        if (aTrayNoPoint != null)
                        {
                            aTrayNoPoint.Value = new List<object>() { aTrayNos[index] };
                            mcp.Write(aTrayNoPoint);
                        }
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                 
                }
            );

            await Task.WhenAll(aTask, bTask);
            await Task.Delay(20000 * 10);
        }

        [TestMethod]
        public async Task DataCollectionChkeIsRepeatTest()
        {
            await Task.Delay(1000);

            var aService = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(Factory6Workshop6_3AssemblyLineA));

            var bService = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(Factory6Workshop6_3AssemblyLineB));

            int[] aTrayNos = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            int[] bTrayNos = new int[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 };

            var mcp = Container.Resolve<McpCommunication>();

            Task aTask = RunAsync("A线", aService, aTrayNos);
            await Task.Delay(5000);
            Task bTask = RunAsync(
                "B线",
                bService,
                bTrayNos,
                async (lineInfo, index) =>
                {
                    await Task.Delay(100);
                    var aTrayNoPoint = lineInfo.FirstOrDefault(x => x.PointName == "A线托盘编号");
                    if (aTrayNoPoint != null)
                    {
                        aTrayNoPoint.Value = new List<object>() { aTrayNos[index] };
                        mcp.Write(aTrayNoPoint);
                    }
                }
            );

            await Task.WhenAll(aTask, bTask);
            await Task.Delay(20000 * 10);
        }

        [TestMethod]
        public async Task DataCollectionBindingTest()
        {
            await Task.Delay(1000);

            var aService = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(Factory6Workshop6_3AssemblyLineA));

            var bService = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(Factory6Workshop6_3AssemblyLineB));

            var bindService = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(Factory6Workshop6_3AssemblyLineABBinding));

            int[] aTrayNos = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int[] bTrayNos = new int[] { 10, 20, 30, 40, 50, 60, 70, 80, 90 };

            var mcp = Container.Resolve<McpCommunication>();

            var bLinePoints = await InitPlcAddre("B线");
            var aBindPoint = bLinePoints.FirstOrDefault(x => x.PointName == "A托盘绑定");
            var bBindPoint = bLinePoints.FirstOrDefault(x => x.PointName == "B托盘绑定");
            Assert.IsNotNull(aBindPoint, "未找到点位：A托盘绑定");
            Assert.IsNotNull(bBindPoint, "未找到点位：B托盘绑定");

            bindService.Start();

            Task aTask = RunAsync("A线", aService, aTrayNos, null);
            await Task.Delay(5000);
            var binTask = (async () =>
            {
                for (int i = 0; i < 9; i++)
                {
                    aBindPoint.Value = new List<object>() { aTrayNos[i] };
                    bBindPoint.Value = new List<object>() { bTrayNos[i] };
                    mcp.Write(aBindPoint);
                    mcp.Write(bBindPoint);
                    await Task.Delay(1000);
                }
            });
            await Task.WhenAll(aTask);
            await binTask();
            Task bTask = RunAsync(
                "B线",
                bService,
                bTrayNos,
                async (lineInfo, index) =>
                {
                    await Task.Delay(100);
                    var aTrayNoPoint = lineInfo.FirstOrDefault(x => x.PointName == "A线托盘编号");
                    if (aTrayNoPoint != null)
                    {
                        aTrayNoPoint.Value = new List<object>() { aTrayNos[index] };
                        mcp.Write(aTrayNoPoint);
                    }
                }
            );

            await Task.WhenAll(aTask, bTask);
            await Task.Delay(2000 * 10);
        }

        #endregion

        #region 辅助方法

        private async Task RunAsync(
            string lineName,
            DataCollectionServiceAbstract lineServer,
            int[] pallNo,
            Func<List<DevPlcPointDto>, int, Task> func = null
        )
        {
            lineServer.Start();

            var lineInfo = await InitPlcAddre(lineName);
            var bindPoints = lineInfo.Where(x => x.PointName.Contains("绑定")).ToList();
            foreach (var bindPoint in bindPoints)
            {
                lineInfo.Remove(bindPoint);
            }
            var endPoint = lineInfo.FirstOrDefault(x => x.PointName == "采集结束");
            var startPoint = lineInfo.FirstOrDefault(x => x.PointName == "采集开始");
            var pallNotPoint = lineInfo.FirstOrDefault(x => x.PointName.Contains("托盘号"));
            var modelNoPoint = lineInfo.FirstOrDefault(x => x.PointName.Contains("型号"));
            var ngCodeoPoint = lineInfo.FirstOrDefault(x => x.PointName.Contains("NG代码"));

            modelNoPoint.Value = new List<object>() { 4 };
            ngCodeoPoint.Value = new List<object>() { 0 };

            lineInfo.Remove(endPoint);
            lineInfo.Remove(startPoint);
            lineInfo.Remove(pallNotPoint);
            lineInfo.Remove(ngCodeoPoint);
            lineInfo.Remove(modelNoPoint);

            var mcp = Container.Resolve<McpCommunication>();

            mcp.Write(modelNoPoint);
            mcp.Write(ngCodeoPoint);

            startPoint.Value = new List<object>() { true };
            mcp.Write(startPoint);
            //EndServer(endPoint, startPoint, mcp);
            int a = 0;
            try
            {
              

                int val = 1;
                for (int i = 0; i < pallNo.Length - 1; i++)
                {
                    a = i;
                    //if ((mcp.Read(startPoint)).Data.Value[0].ObjToBool() is false)
                    //{
                    val = await WriteValueAsync(lineInfo, pallNotPoint, mcp, val, pallNo[i]);

                    if (func != null)
                    {
                        await func(lineInfo, i);
                    }
                    await Task.Delay(2 * 1000);
                }
                //else
                //{
                //    i--;

                //}
            }
            catch (Exception ex)
            {

                throw;
            }

           
        }

        private static async Task<int> WriteValueAsync(
            List<DevPlcPointDto> lineInfo,
            DevPlcPointDto pallNotPoint,
            McpCommunication mcp,
            int val,
            int item
        )
        {
            pallNotPoint.Value = new List<object>() { item };
            mcp.Write(pallNotPoint);

            for (int i = 0; i < lineInfo.Count; i++)
            {
                if (lineInfo[i].PointName.Contains("托盘编号"))
                {
                    continue;
                }
                lineInfo[i].Value = new List<object>() { val };
                val++;
                mcp.Write(lineInfo[i]);
            }

            return val;
        }

        private void EndServer(
            DevPlcPointDto endPoint,
            DevPlcPointDto startPoint,
            McpCommunication mcp
        )
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var endValue = mcp.Read(endPoint);
                        if (endValue.Data.Value[0].ObjToBool())
                        {
                            startPoint.Value = new List<object>() { false };
                            endPoint.Value = new List<object>() { false };
                            mcp.Write(startPoint);
                            mcp.Write(endPoint);
                        }
                    }
                    finally
                    {
                        await Task.Delay(200);
                    }
                }
            });
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

        #endregion
    }
}
