using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using Autofac;
using HslCommunication.Profinet.Melsec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar.Extensions;

namespace UnitTestProject
{
    /// <summary>
    /// 数据采集服务单元测试类
    /// 测试各生产线数据采集服务的功能
    /// </summary>
    /// <remarks>
    /// 测试前提条件：
    /// 1. 数据库中已配置A线、B线、热处理生产线的设备信息和PLC点位
    /// 2. PLC模拟服务器已启动（端口2000、6000、4990）
    /// 3. Autofac依赖注入容器配置正确
    /// </remarks>
    [TestClass]
    public class DataCollectionSTest
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

        /// <summary>
        /// 测试类构造函数
        /// 初始化依赖注入容器和PLC模拟服务器
        /// </summary>
        public DataCollectionSTest()
        {
            // 1. 配置依赖注入容器
            var builder = new ContainerBuilder();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();
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
                    EnableIPv6 = false
                };
                server.ServerStart(port);
                _servers.Add(server);
            }
        }

        #region 单线测试方法

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
        public async Task Rcl_DataCollectionTest()
        {
            // 从容器中解析热处理服务
            var rclService = Container
                .Resolve<IEnumerable<ProLineDataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(Rcl_ProLineDataCollectionService));

            // 启动热处理数据采集
            rclService.ExecuteAsync(new CancellationToken());

            // 保持运行20秒以便观察采集情况
            await Task.Delay(20000 * 1000);
        }

        /// <summary>
        /// 测试A线数据采集服务
        /// 启动服务后等待20秒，观察是否能正常采集数据
        /// </summary>
        /// <remarks>
        /// A线服务特点：
        /// - 通过托盘号A进行去重
        /// - NG产品自动生成汇总记录
        /// </remarks>
        [TestMethod]
        public async Task A_DataCollectionTest()
        {
            // 从容器中解析A线服务
            var aService = Container
                .Resolve<IEnumerable<ProLineDataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(A_ProLineDataCollectionService));

            // 启动A线数据采集
            await aService.ExecuteAsync(new CancellationToken());

            // 保持运行20秒以便观察采集情况
            await Task.Delay(20000 * 1000);
        }

        /// <summary>
        /// 测试B线数据采集服务
        /// 启动服务后等待20秒，观察是否能正常采集数据
        /// </summary>
        /// <remarks>
        /// B线服务特点：
        /// - 通过托盘号B进行去重
        /// - 关联A线数据生成汇总记录
        /// </remarks>
        [TestMethod]
        public async Task B_DataCollectionTest()
        {
            // 从容器中解析B线服务
            var bService = Container
                .Resolve<IEnumerable<ProLineDataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(B_ProLineDataCollectionService));

            // 启动B线数据采集
            await bService.ExecuteAsync(new CancellationToken());

            // 保持运行20秒以便观察采集情况
            await Task.Delay(20000 * 1000);
        }

        #endregion

        #region A/B线联动测试

        /// <summary>
        /// 测试A/B线联动数据采集
        /// 模拟A线先采集，B线后采集的完整流程
        /// </summary>
        /// <remarks>
        /// 联动测试流程：
        /// 1. A线采集9个托盘的数据
        /// 2. B线采集对应的9个托盘数据（包含A线托盘编号关联）
        /// 3. 验证A/B线数据能正确关联并生成汇总记录
        /// </remarks>
        [TestMethod]
        public async Task DataCollectionTest()
        {
            await Task.Delay(1000);

            // 解析A线和B线服务
            var aService = Container
                .Resolve<IEnumerable<ProLineDataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(A_ProLineDataCollectionService));

            var bService = Container
                .Resolve<IEnumerable<ProLineDataCollectionServiceAbstract>>()
                .First(x => x.GetType().Name == nameof(B_ProLineDataCollectionService));

            // 定义A线和B线的托盘号序列
            int[] aTrayNos = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int[] bTrayNos = new int[] { 10, 20, 30, 40, 50, 60, 70, 80, 90 };

            var mcp = Container.Resolve<McpCommunication>();

            // 启动A线和B线的数据采集任务
            Task aTask = RunAsync("A线", aService, aTrayNos);
            await Task.Delay(5000); // 等待A线先启动
            Task bTask = RunAsync(
                "B线",
                bService,
                bTrayNos,
                // B线特殊的回调函数：设置A线托盘编号关联
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

            // 等待所有任务完成
            await Task.WhenAll(aTask, bTask);
            await Task.Delay(20000 * 10);
        }

        /// <summary>
        /// 执行单条生产线的PLC数据模拟写入
        /// </summary>
        /// <param name="lineName">生产线名称</param>
        /// <param name="lineServer">数据采集服务实例</param>
        /// <param name="pallNo">托盘号序列</param>
        /// <param name="func">可选的回调函数（用于B线设置A线托盘编号关联）</param>
        private async Task RunAsync(
            string lineName,
            ProLineDataCollectionServiceAbstract lineServer,
            int[] pallNo,
            Func<List<DevPlcPointMcDto>, int, Task> func = null
        )
        {
            // 1. 启动数据采集服务
            lineServer.ExecuteAsync(new CancellationToken());

            // 2. 初始化PLC点位
            var lineInfo = await InitPlcAddre(lineName);
            var endPoint = lineInfo.FirstOrDefault(x => x.PointName == "采集结束");
            var startPoint = lineInfo.FirstOrDefault(x => x.PointName == "采集开始");
            var pallNotPoint = lineInfo.FirstOrDefault(x => x.PointName.Contains("托盘号"));
            var modelNoPoint = lineInfo.FirstOrDefault(x => x.PointName.Contains("型号"));
            var ngCodeoPoint = lineInfo.FirstOrDefault(x => x.PointName.Contains("NG代码"));

            // 3. 初始化固定值
            modelNoPoint.Value = new List<object>() { 4 };
            ngCodeoPoint.Value = new List<object>() { 0 };

            // 4. 移除控制点位，只保留数据采集点位
            lineInfo.Remove(endPoint);
            lineInfo.Remove(startPoint);
            lineInfo.Remove(pallNotPoint);
            lineInfo.Remove(ngCodeoPoint);
            lineInfo.Remove(modelNoPoint);

            var mcp = Container.Resolve<McpCommunication>();

            // 5. 写入初始值
            mcp.Write(modelNoPoint);
            mcp.Write(ngCodeoPoint);

            // 6. 启动PLC完成信号监控
            EndServer(endPoint, startPoint, mcp);

            // 7. 循环写入托盘数据
            int val = 1;
            for (int i = 0; i < pallNo.Length; i++)
            {
                // 等待采集开始信号为false（表示PLC可以接收新数据）
                if ((mcp.Read(startPoint)).Data.Value[0].ObjToBool() is false)
                {
                    // 写入托盘号和点位数据
                    val = await WriteValueAsync(lineInfo, pallNotPoint, mcp, val, pallNo[i]);

                    // 执行回调函数（如B线需要设置A线托盘编号）
                    if (func != null)
                    {
                        await func(lineInfo, i);
                    }

                    // 触发采集开始信号
                    startPoint.Value = new List<object>() { true };
                    mcp.Write(startPoint);
                }
                else
                {
                    // 如果PLC还在处理，等待200ms后重试
                    i--;
                    await Task.Delay(200);
                }
            }
        }

        /// <summary>
        /// 写入点位值到PLC
        /// </summary>
        /// <param name="lineInfo">PLC点位列表</param>
        /// <param name="pallNotPoint">托盘号点位</param>
        /// <param name="mcp">PLC通信组件</param>
        /// <param name="val">当前值计数器</param>
        /// <param name="item">托盘号</param>
        /// <returns>更新后的值计数器</returns>
        private static async Task<int> WriteValueAsync(
            List<DevPlcPointMcDto> lineInfo,
            DevPlcPointMcDto pallNotPoint,
            McpCommunication mcp,
            int val,
            int item
        )
        {
            // 写入托盘号
            pallNotPoint.Value = new List<object>() { item };
            mcp.Write(pallNotPoint);

            // 循环写入各点位数据
            for (int i = 0; i < lineInfo.Count; i++)
            {
                lineInfo[i].Value = new List<object>() { val };
                val++;
                mcp.Write(lineInfo[i]);
            }

            return val;
        }

        /// <summary>
        /// 热处理数据采集测试（替代方法）
        /// 通过RunAsync通用方法执行热处理测试
        /// </summary>
        [TestMethod]
        public async Task RclRunAsync()
        {
            var lineInfo = await InitPlcAddre("热处理");
            var endPoint = lineInfo.FirstOrDefault(x => x.PointName == "采集结束");
            var startPoint = lineInfo.FirstOrDefault(x => x.PointName == "采集开始");
            lineInfo.Remove(endPoint);
            lineInfo.Remove(startPoint);
            var mcp = Container.Resolve<McpCommunication>();

            // 启动采集完成监控
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

        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 启动PLC采集完成信号监控
        /// 当检测到采集完成信号时，重置控制点位
        /// </summary>
        /// <param name="endPoint">采集结束点位</param>
        /// <param name="startPoint">采集开始点位</param>
        /// <param name="mcp">PLC通信组件</param>
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
                            // 重置控制点位
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

        /// <summary>
        /// 热处理服务专用的PLC采集完成信号监控
        /// </summary>
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

        /// <summary>
        /// 从数据库初始化指定生产线的PLC点位信息
        /// </summary>
        /// <param name="lineName">生产线名称</param>
        /// <returns>PLC点位DTO列表</returns>
        private async Task<List<DevPlcPointMcDto>> InitPlcAddre(string lineName)
        {
            // 从数据库获取设备信息
            var linePoint = await Container
                .Resolve<Tb_EquipmentRepository>()
                .GetEquipmentAllAsync(x => x.DeviceName == lineName);

            // 转换为PLC点位DTO列表
            var result = new List<DevPlcPointMcDto>();
            foreach (var plcConnection in linePoint.PlcConnections)
            {
                foreach (var point in plcConnection.Points)
                {
                    result.Add(new DevPlcPointMcDto(
                        linePoint.DeviceName,          // 设备名称
                        point.PointName,               // 点位名称
                        plcConnection.IpAddress,       // IP地址
                        plcConnection.Port,           // 端口
                        point.Area.ToPrefix(),         // 区域前缀
                        point.DataType.ToTypeCode(),  // 数据类型
                        point.Address,                 // 地址
                        point.Length,                  // 长度
                        point.ReadFormula,             // 读取公式
                        point.WriteFormula             // 写入公式
                    ));
                }
            }
            return result;
        }

        #endregion
    }
}