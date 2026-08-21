using Autofac;
using HslCommunication.Profinet.Melsec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MQTTnet.Server;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection;
using SL.MLineDataPrecisionTracking.Infrastructure.Expand;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestProject.DataCollection
{
    [TestClass]
    public class MqttDataColltionTest
    {
        private IContainer Container { get; set; }
        private Dictionary<string, MelsecMcServer> _servers;
        private readonly Random _random = new Random();
        private  HslCommunication.MQTT.MqttServer _mqttServer = new ();
        public MqttDataColltionTest()
        {
            var builder = new ContainerBuilder();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();
            Container = builder.Build();
            //_mqttServer.ServerStart(1833);
            _servers = new Dictionary<string, MelsecMcServer>();
            foreach (var port in new int[] { 6000, 8000, 9000 })
            {
                var server = new MelsecMcServer
                {
                    IsBinary = true,
                    AnalysisLogMessage = true,
                    ActiveTimeSpan = TimeSpan.Parse("01:00:00"),
                    EnableIPv6 = false,
                };
                server.ServerStart(port);
                _servers[$"127.0.0.1:{port}"] = server;
            }
        }

        [TestMethod]
        public void MqttTest()
        {
        }

        /// <summary>
        /// 测试StationCollectionManager：获取工位信息，然后异步给点位写数据供服务采集
        /// 点位写入规则：
        /// - 设备状态 1~7 int16 随机写入
        /// - 生产节拍 19~23 int16 随机写入
        /// - 生产总数、生产OK数、生产NG数 int16 累加写入
        /// - 警报点位(bool) 随机 true/false
        /// 每个工位20s执行一次数据变更
        /// </summary>
        [TestMethod]
        public async Task StationCollectionManager_DataWriteTest()
        {
            var equipmentRepo = Container.Resolve<Tb_EquipmentRepository>();
            var deviceCollectionRepo = Container.Resolve<Tb_DeviceCollectionRepository>();
            var equipmentManagementService = Container.Resolve<EquipmentManagementService>();
            var plcPointRepo = Container.Resolve<Tb_PlcPointRepository>();
            
            var mcp = Container.Resolve<McpCommunication>();

            var manager = new StationCollectionManager(
                equipmentRepo,
                equipmentManagementService,
                deviceCollectionRepo,
                mcp
            );

            var equipments = await equipmentRepo.GetEquipmentAllAsync();
            Assert.IsNotNull(equipments, "未查询到设备工位");
            Assert.IsTrue(equipments.Count > 0, "数据库中没有设备工位");

            Console.WriteLine($"共获取到 {equipments.Count} 个工位");

            //foreach (var equipment in equipments)
            //{
            //    if (equipment.LineId == null)
            //    {
            //        Console.WriteLine($"设备 {equipment.DeviceName} 未归属产线，跳过注册");
            //        continue;
            //    }

            //    var result = await manager.RegisterDeviceAsync(equipment.EquipmentId);
            //    if (result.IsSuccess)
            //    {
            //        Console.WriteLine($"工位 {equipment.DeviceName}({equipment.EquipmentId}) 注册成功");
            //        await manager.StartAsync(equipment.EquipmentId);
            //    }
            //    else
            //    {
            //        Console.WriteLine($"工位 {equipment.DeviceName}({equipment.EquipmentId}) 注册失败: {result.Message}");
            //    }
            //}

            manager.RestoreAsync();

            var writeTasks = new List<Task>();
            foreach (var equipment in equipments)
            {
                if (equipment.LineId == null || equipment.PlcConnections == null)
                    continue;
             
                
                    foreach (Tb_PlcConnection item in equipment.PlcConnections)
                    {
                        item.Points= await plcPointRepo.QueryableAsync(x => x.PlcConnectionId == item.Id);
                    }
              
                var task = Task.Run(async () =>
                {
                    await WriteDataForStation(equipment);
                });
                writeTasks.Add(task);
            }

            Console.WriteLine($"已启动 {writeTasks.Count} 个工位的数据写入任务");

            await Task.Delay(TimeSpan.FromMinutes(5));

            await manager.StopAllAsync();
            manager.StopWorkers();

            Console.WriteLine("测试完成，已停止所有工位");
        }

        /// <summary>
        /// 为单个工位写入模拟数据（20s周期），使用 HslCommunication MelsecMcServer 写入
        /// 按 plc.IpAddress:plc.Port 从 _servers 字典查找对应的 server
        /// </summary>
        private async Task WriteDataForStation(Tb_Equipment equipment)
        {
            int producedTotal = 0;
            int producedOk = 0;
            int producedNg = 0;

            // 按 ip:port 分组点位，每组对应一个 server
            var plcGroups = equipment.PlcConnections
                .Where(plc => plc.Points != null && plc.Points.Count > 0)
                .Select(plc => new
                {
                    ServerKey = $"{plc.IpAddress}:{plc.Port}",
                    Points = plc.Points.Select(point => new DevPlcPointDto(
                        equipment.DeviceName,
                        point.PointName,
                        plc.IpAddress,
                        plc.Port,
                        point.Area,
                        point.DataType.ToTypeCode(),
                        point.Address,
                        point.Length,
                        point.ReadFormula,
                        point.WriteFormula
                    )).ToList()
                })
                .ToList();

            if (plcGroups.Count == 0)
            {
                Console.WriteLine($"工位 {equipment.DeviceName} 无点位信息");
                return;
            }

            Console.WriteLine($"工位 {equipment.DeviceName} 共 {plcGroups.Sum(g => g.Points.Count)} 个点位，开始数据写入");

            while (true)
            {
                try
                {
                    foreach (var group in plcGroups)
                    {
                        if (!_servers.TryGetValue(group.ServerKey, out var server))
                        {
                            Console.WriteLine($"工位 {equipment.DeviceName} 未找到 server: {group.ServerKey}");
                            continue;
                        }

                        foreach (var point in group.Points)
                        {
                            string address = point.Prefix + point.Address;

                            switch (point.PointName)
                            {
                                case "设备状态":
                                    server.Write(address, (short)_random.Next(1, 8));
                                    break;

                                case "生产节拍":
                                    server.Write(address, (short)_random.Next(19, 24));
                                    break;

                                case "生产总数":
                                    producedTotal++;
                                    server.Write(address, (short)producedTotal);
                                    break;

                                case "生产OK数":
                                    producedOk++;
                                    server.Write(address, (short)producedOk);
                                    break;

                                case "生产NG数":
                                    producedNg++;
                                    server.Write(address, (short)producedNg);
                                    break;

                                default:
                                    if (point.DataType == TypeCode.Boolean || point.DataType == TypeCode.Object)
                                    {
                                        server.Write(address, (bool)(_random.Next(2) == 1));
                                    }
                                    else
                                    {
                                        server.Write(address, (short)_random.Next(0, 100));
                                    }
                                    break;
                            }
                        }
                    }

                    Console.WriteLine($"工位 {equipment.DeviceName} 数据写入完成 - 总数:{producedTotal}, OK:{producedOk}, NG:{producedNg}");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"工位 {equipment.DeviceName} 数据写入异常: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

        /// <summary>
        /// 测试单个工位的数据采集和推送
        /// </summary>
        [TestMethod]
        public async Task SingleStation_CollectionAndPushTest()
        {
            var equipmentRepo = Container.Resolve<Tb_EquipmentRepository>();
            var deviceCollectionRepo = Container.Resolve<Tb_DeviceCollectionRepository>();
            var equipmentManagementService = Container.Resolve<EquipmentManagementService>();
            var mcp = Container.Resolve<McpCommunication>();

            var equipments = await equipmentRepo.GetEquipmentAllAsync();
            var targetEquipment = equipments?.FirstOrDefault(e => e.LineId != null && e.PlcConnections?.Count > 0);

            if (targetEquipment == null)
            {
                Assert.Inconclusive("没有找到合适的测试设备（需要归属产线且有PLC连接）");
                return;
            }

            Console.WriteLine($"测试设备: {targetEquipment.DeviceName}({targetEquipment.EquipmentId})");

            var manager = new StationCollectionManager(
                equipmentRepo,
                equipmentManagementService,
                deviceCollectionRepo,
                mcp
            );

            var registerResult = await manager.RegisterDeviceAsync(targetEquipment.EquipmentId);
            Console.WriteLine($"注册结果: {registerResult.IsSuccess} - {registerResult.Message}");
            Assert.IsTrue(registerResult.IsSuccess, "工位注册失败");

            await manager.StartAsync(targetEquipment.EquipmentId);
            manager.StartWorkers();

            var writeTask = Task.Run(async () =>
            {
                await WriteDataForStation(targetEquipment);
            });

            await Task.Delay(TimeSpan.FromSeconds(60));

            var stationInfo = await manager.GetRegisteredAsync(targetEquipment.EquipmentId);
            Console.WriteLine($"工位状态: {stationInfo.Data?.Status}");
            Console.WriteLine($"工位描述: {stationInfo.Data?.Description}");

            await manager.StopAsync(targetEquipment.EquipmentId);
            manager.StopWorkers();

            Console.WriteLine("单工位测试完成");
        }
    }
}

