using Autofac;
using HslCommunication.Profinet.Melsec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPOI.SS.Formula.Functions;
using S7.Net.Types;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_1;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Workshop6_3;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SqlSugar.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestProject
{
    [TestClass]
    public class Factory6Workshop6_1Test
    {
        /// <summary>
        /// 依赖注入容器
        /// </summary>
        private IContainer Container { get; set; }

        /// <summary>
        /// PLC模拟服务器列表
        /// 用于模拟PLC通信
        /// </summary>
        private List<MelsecMcServer> _servers;

        public Factory6Workshop6_1Test()
        { // 1. 配置依赖注入容器
            var builder = new ContainerBuilder();
            builder.AddSqlSugerMiddleware();
            builder.AddInfrastructureMiddleware();
            builder.AddLogMiddleware();
            builder.AddCoreMiddleware();
            Container = builder.Build();

            // 2. 启动PLC模拟服务器（监听端口：2000、6000、4990）
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

        #region 热处理
        /// <summary>
        /// 测试热处理数据采集服务
        /// 启动服务后等待20秒，观察是否能正常采集数据
        /// </summary>
        /// <remarks>
        /// 热处理服务特点：
        /// - 通过序列码进行去重
        /// - 数据独立入库，不参与A/B线汇总
        /// </remarks>
        [TestMethod]
        public async Task Rcl_DataCollectionDebugging()
        {
            // 从容器中解析热处理服务
            var rclService = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(Factory6Section6_3HeatTreatment));

            // 启动热处理数据采集
            rclService.Start();

            // 保持运行20秒以便观察采集情况
            await Task.Delay(200 * 1000);
        }

        [TestMethod]
        public async Task Rcl_DataCollectionTest()
        {
            var lineInfo = await InitPlcAddre("热处理");
            var endPoint = lineInfo.FirstOrDefault(x => x.PointName == "采集结束");
            var startPoint = lineInfo.FirstOrDefault(x => x.PointName == "采集开始");
            lineInfo.Remove(endPoint);
            lineInfo.Remove(startPoint);
            var mcp = Container.Resolve<McpCommunication>();
            var rclService = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(Factory6Section6_3HeatTreatment));
            rclService.Start();
            // 默认采集开始触发
            startPoint.Value = new List<object>() { true };
            mcp.Write(startPoint);

            int val = 1000;
            for (int i = 0; i < 100; i++)
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

                await Task.Delay(200);
            }
        }

        /// <summary>
        /// 从数据库初始化指定生产线的PLC点位信息
        /// </summary>
        /// <param name="lineName">生产线名称</param>
        /// <returns>PLC点位DTO列表</returns>
        private async Task<List<DevPlcPointDto>> InitPlcAddre(string lineName)
        {
            // 从数据库获取设备信息
            var linePoint = await Container
                .Resolve<Tb_EquipmentRepository>()
                .GetEquipmentAllAsync(x => x.DeviceName == lineName);

            // 转换为PLC点位DTO列表
            var result = new List<DevPlcPointDto>();
            foreach (var plcConnection in linePoint.PlcConnections)
            {
                foreach (var point in plcConnection.Points)
                {
                    result.Add(
                        new DevPlcPointDto(
                            linePoint.DeviceName, // 设备名称
                            point.PointName, // 点位名称
                            plcConnection.IpAddress, // IP地址
                            plcConnection.Port, // 端口
                            point.Area, // 区域前缀
                            point.DataType.ToTypeCode(), // 数据类型
                            point.Address, // 地址
                            point.Length, // 长度
                            point.ReadFormula, // 读取公式
                            point.WriteFormula // 写入公式
                        )
                    );
                }
            }
            return result;
        }

        #endregion



        /// <summary>
        /// 启动PLC采集完成信号监控
        /// 当检测到采集完成信号时，重置控制点位
        /// </summary>
        /// <param name="endPoint">采集结束点位</param>
        /// <param name="startPoint">采集开始点位</param>
        /// <param name="mcp">PLC通信组件</param>
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
                        // 重置控制点位
                        startPoint.Value = new List<object>() { false };
                        endPoint.Value = new List<object>() { false };
                        mcp.Write(startPoint);
                    }
                    finally
                    {
                        await Task.Delay(200);
                    }
                }
            });
        }
    }
}
