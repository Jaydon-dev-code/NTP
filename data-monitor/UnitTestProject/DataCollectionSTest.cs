using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using HslCommunication.Profinet.Melsec;
using McpXLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Pqc.Crypto.Falcon;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using SqlSugar.Extensions;

namespace UnitTestProject
{
    [TestClass]
    public class DataCollectionSTest
    {
        IContainer Container;

        List<MelsecMcServer> _servers;

        public DataCollectionSTest()
        {
            var builder = new ContainerBuilder();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();
            Container = builder.Build();
            //-------------------------------------------------
            foreach (
                var item in new int[]
                {  2000, 6000,
                    4990,
                }
            )
            {
                MelsecMcServer server = new HslCommunication.Profinet.Melsec.MelsecMcServer();
                server.IsBinary = true;
                server.AnalysisLogMessage = true;
                server.ActiveTimeSpan = TimeSpan.Parse("01:00:00");

                server.EnableIPv6 = false;
                server.ServerStart(item);
            }
        }

        [TestMethod]
        public async Task Rcl_DataCollectionTest()
        {
            var rcl = Container.Resolve<
                ProLineDataCollectionServiceAbstract<Tb_HeatTreatmentData>
            >();
            rcl.ExecuteAsync(new System.Threading.CancellationToken());
            //await RclRunAsync("热处理");
            await Task.Delay(20000 * 1000);
        }

        [TestMethod]
        public async Task A_DataCollectionTest()
        {
            var rcl = Container.Resolve<ProLineDataCollectionServiceAbstract<Tb_LineA>>();
            await rcl.ExecuteAsync(new System.Threading.CancellationToken());

            await Task.Delay(20000 * 1000);
        }

        private async Task RclRunAsync(string lineName)
        {
            var lineInfo = await InitPlcAddre(lineName);
            var endPoint = lineInfo.FirstOrDefault(x => x.PointName == "采集结束");
            var startPoint = lineInfo.FirstOrDefault(x => x.PointName == "采集开始");
            lineInfo.Remove(endPoint);
            lineInfo.Remove(startPoint);
            var mcp = Container.Resolve<McpCommunication>();
            RclEndServer(endPoint, startPoint, mcp);
            int val = 1000;
            for (int i = 0; i < 10; i++)
            {
                if ((mcp.Read(startPoint)).Data.Value[0].ObjToBool() is false)
                {
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

        private void RclEndServer(
            DevPlcPointMcDto endPoint,
            DevPlcPointMcDto startPoint,
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

        [TestMethod]
        public async Task B_DataCollectionTest()
        {
            var b = Container.Resolve<ProLineDataCollectionServiceAbstract<Tb_LineB>>();
            await b.ExecuteAsync(new System.Threading.CancellationToken());
            await Task.Delay(20000 * 1000);
        }

        [TestMethod]
        public async Task DataCollectionTest()
        {
            await Task.Delay(1000);
            var a = Container.Resolve<ProLineDataCollectionServiceAbstract<Tb_LineA>>();
            var b = Container.Resolve<ProLineDataCollectionServiceAbstract<Tb_LineB>>();
            int[] aPallNo = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int[] bPallNo = new int[] { 10, 20, 30, 40, 50, 60, 70, 80, 90 };
            var mcp = Container.Resolve<McpCommunication>();
            Task task1 = RunAsync("A线", a, aPallNo);
            await Task.Delay(5000);
            Task task2 = RunAsync(
                "B线",
                b,
                bPallNo, // 把 Action 改成 Func<..., Task> 就能等待了
                new Func<List<DevPlcPointMcDto>, int, Task>(
                    async (soure, index) =>
                    {
                        await Task.Delay(100);
                        var aPallPoint = soure.FirstOrDefault(x => x.PointName == "A线托盘编号");

                        if (aPallPoint != null)
                        {
                            aPallPoint.Value = new List<object>() { aPallNo[index] };
                            mcp.Write(aPallPoint);
                        }
                    }
                )
            );
            await Task.WhenAll(task1, task2);
            await Task.Delay(20000 * 10);
        }

        async Task RunAsync<T>(
            string lineName,
            ProLineDataCollectionServiceAbstract<T> lineServer,
            int[] pallNo,
            Func<List<DevPlcPointMcDto>, int, Task> func = null
        )
            where T : class, new()
        {
            lineServer.ExecuteAsync(new System.Threading.CancellationToken());
            var lineInfo = await InitPlcAddre(lineName);

            //foreach (var item in lineInfo)
            //{
            //    item.IpAddress = "127.0.0.1";
            //}
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
            var mcp = Container.Resolve<McpCommunication>();
            lineInfo.Remove(modelNoPoint);

            mcp.Write(modelNoPoint);
            mcp.Write(ngCodeoPoint);
            EndServer(endPoint, startPoint, mcp);

            int val = 1;
            for (int i = 0; i < pallNo.Length; i++)
            {
                if ((mcp.Read(startPoint)).Data.Value[0].ObjToBool() is false)
                {
                    val = await WirteVale(lineInfo, pallNotPoint, mcp, val, pallNo[i]);
                    if (func != null)
                    {
                        await func(lineInfo, i);
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

        private static async Task<int> WirteVale(
            List<DevPlcPointMcDto> lineInfo,
            DevPlcPointMcDto pallNotPoint,
            McpCommunication mcp,
            int val,
            int item
        )
        {
            pallNotPoint.Value = new List<object>() { item };
            mcp.Write(pallNotPoint);
            for (int i = 0; i < lineInfo.Count; i++)
            {
                lineInfo[i].Value = new List<object>() { val };
                val++;
                mcp.Write(lineInfo[i]);
            }

            return val;
        }

        private void EndServer(
            DevPlcPointMcDto endPoint,
            DevPlcPointMcDto startPoint,
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

        protected async Task<List<DevPlcPointMcDto>> InitPlcAddre(string lineName)
        {
            var linePoint = await Container
                .Resolve<Tb_EquipmentRepository>()
                .GetEquipmentAllAsync(x => x.DeviceName == lineName);
            var re = new List<DevPlcPointMcDto>();
            foreach (var plcLinkeInfo in linePoint.PlcConnections)
            {
                foreach (var plcAddres in plcLinkeInfo.Points)
                {
                    re.Add(
                        new DevPlcPointMcDto(
                            linePoint.DeviceName,
                            plcAddres.PointName,
                            plcLinkeInfo.IpAddress,
                            plcLinkeInfo.Port,
                            plcAddres.Area.ToPrefix(),
                            plcAddres.DataType.ToTypeCode(),
                            plcAddres.Address,
                            plcAddres.Length,
                            plcAddres.ReadFormula,
                            plcAddres.WriteFormula
                        )
                    );
                }
            }
            return re;
        }
    }
}
