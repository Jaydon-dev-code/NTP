using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using HslCommunication.Profinet.Melsec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection;
using SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection.DataCollection;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage.SPC;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.SPC;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;

namespace UnitTestProject.SPC
{
    /// <summary>
    /// SPC 采集测试 — 覆盖：
    ///   1) 数据库采集器 DBMeasureResult 初始化/空执行
    ///   2) PLC 采集器 PlcCollectStatus 初始化/读点/上升沿
    ///   3) 采集管理器 AddOrUpdate / 启停 / GetAll / SetContext 换型
    ///   4) 解析上下文注入采集器
    /// </summary>
    [TestClass]
    public class SPCCollectionTest
    {
        private IContainer _container;
        private MelsecMcServer _plcServer;
        private const int PlcPort = 6010;

        public SPCCollectionTest()
        {
            var builder = new ContainerBuilder();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();
            _container = builder.Build();

            // 本地软 PLC，供 PlcCollectStatus 读点
            _plcServer = new MelsecMcServer
            {
                IsBinary = true,
                AnalysisLogMessage = true,
                ActiveTimeSpan = TimeSpan.Parse("01:00:00"),
                EnableIPv6 = false,
            };
            _plcServer.ServerStart(PlcPort);
        }

        #region 数据库采集

        /// <summary>
        /// DBMeasureResult：DataSource / Description 元数据正确。
        /// </summary>
        [TestMethod]
        public void DBMeasureResult_Metadata()
        {
            var collector = new DBMeasureResult();
            Assert.AreEqual(SPCDataSource.Database, collector.DataSource);
            Assert.AreEqual("数据库测量结果采集", collector.Description);
        }

        /// <summary>
        /// DBMeasureResult：Init 设置 FeatureName；无 source 时 Execute 不抛异常。
        /// </summary>
        [TestMethod]
        public void DBMeasureResult_InitAndExecute_NoSource_DoesNotThrow()
        {
            var collector = new DBMeasureResult();
            collector.Init("测试-数据库采集");

            Assert.AreEqual("测试-数据库采集", collector.FeatureName);

            // source 为 null：Execute 应直接 return
            collector.Execute();
        }

        /// <summary>
        /// DBMeasureResult：带无效连接串 Init 后 Execute 仅记日志不抛出。
        /// （真正拉数依赖 SQL Server，本机可能没有——只验证不崩溃）
        /// </summary>
        [TestMethod]
        public void DBMeasureResult_Execute_InvalidConnection_DoesNotThrow()
        {
            var collector = new DBMeasureResult();
            collector.Init("测试-无效连接", new Tb_SPC_CollectionSource
            {
                CharacteristicId = 0,
                ConnectionString = "Server=127.0.0.1;Database=no_such_db;User Id=sa;Password=x;Timeout=2;",
                TableName = "Measure",
                MeasureValueField = "Value",
                MeasureResultField = "Result",
                TimeFieldName = "CreateTime",
                LastCollectionTime = null,
            });

            // Execute 内部 try/catch，不应向外抛
            collector.Execute();
        }

        /// <summary>
        /// DBMeasureResult：SetContext 注入解析结果后可读（ResolvedContext 在基类）。
        /// 通过反射读 protected 属性验证注入生效。
        /// </summary>
        [TestMethod]
        public void DBMeasureResult_SetContext_StoresResolvedContext()
        {
            var collector = new DBMeasureResult();
            var ctx = new SPCResolvedContext
            {
                ModelNo = "TEST-M",
                PartCategory = "外壳",
            };
            collector.SetContext(ctx);

            var prop = typeof(SPCDataCollectionAbstract)
                .GetProperty("ResolvedContext",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(prop);
            var stored = prop.GetValue(collector) as SPCResolvedContext;
            Assert.IsNotNull(stored);
            Assert.AreEqual("TEST-M", stored.ModelNo);
            Assert.AreEqual("外壳", stored.PartCategory);
        }

        #endregion

        #region PLC 采集

        /// <summary>
        /// PlcCollectStatus：元数据 + Init 绑定点位 + Execute 不抛。
        /// </summary>
        [TestMethod]
        public void PlcCollectStatus_InitAndExecute_ReadsPlc()
        {
            // 服务端写一个测量结果非 0 值
            _plcServer.Write("D200", (short)1);

            var mcp = _container.Resolve<McpCommunication>();
            var collector = new PlcCollectStatus(mcp);

            var plcPoint = new Tb_PlcPoint
            {
                PlcConnectionId = 0,
                PointName = "测量结果",
                Area = "D",
                Address = "200",
                DataType = "Int16",
                Length = 1,
                ReadFormula = "",
                WriteFormula = "",
            };
            var conn = new Tb_PlcConnection
            {
                IpAddress = "127.0.0.1",
                Port = PlcPort,
                PlcType = "三菱",
                NetworkType = "MC",
            };

            collector.Init("测试-PLC采集", plcPoint, conn);

            Assert.AreEqual("测试-PLC采集", collector.FeatureName);
            Assert.AreEqual(SPCDataSource.PLC, collector.DataSource);
            Assert.AreEqual("PLC 状态采集", collector.Description);

            // 第一次 Execute：0→1 触发上升沿逻辑（当前为 TODO 占位），不应抛
            collector.Execute();
            // 第二次：维持非 0
            collector.Execute();
        }

        /// <summary>
        /// PlcCollectStatus：未 Init（点位 null）时 Execute 直接返回。
        /// </summary>
        [TestMethod]
        public void PlcCollectStatus_Execute_WithoutInit_DoesNotThrow()
        {
            var mcp = _container.Resolve<McpCommunication>();
            var collector = new PlcCollectStatus(mcp);
            collector.Execute();
        }

        /// <summary>
        /// PlcCollectStatus：缺 connection 时用默认 127.0.0.1:8000，Init 不抛。
        /// </summary>
        [TestMethod]
        public void PlcCollectStatus_Init_WithoutConnection_UsesDefault()
        {
            var mcp = _container.Resolve<McpCommunication>();
            var collector = new PlcCollectStatus(mcp);
            collector.Init("默认连接", new Tb_PlcPoint
            {
                PointName = "P",
                Area = "D",
                Address = "10",
                DataType = "Int16",
                Length = 1,
            });
            collector.Execute(); // 连不上 8000 也只应打日志/内部失败，不抛
        }

        #endregion

        #region 采集管理器

        /// <summary>
        /// Manager：AddOrUpdate 数据库特性（有 CollectionSource）成功入队，GetAll 可见。
        /// </summary>
        [TestMethod]
        public async Task Manager_AddOrUpdate_DbCharacteristic_GetAll()
        {
            var charaRepo = _container.Resolve<Tb_SPC_CharacteristicRepository>();
            var sourceRepo = _container.Resolve<Tb_SPC_CollectionSourceRepository>();
            var manager = _container.Resolve<SPCDataCollectionManager>();

            var chara = new Tb_SPC_Characteristic
            {
                Name = "UT-DB特性",
                CharacteristicNo = $"UTDB-{Guid.NewGuid():N}".Substring(0, 12),
                DataType = SPCDataType.Numeric,
                InspectionQty = 1,
                CollectionMethod = "DBMeasureResult",
                DataSource = SPCDataSource.Database,
                PartCategory = "外壳",
                EquipmentId = 1,
                CreateTime = DateTime.Now,
            };
            chara.Id = await charaRepo.InsertAsync(chara);
            await sourceRepo.InsertAsync(new Tb_SPC_CollectionSource
            {
                CharacteristicId = chara.Id,
                ConnectionString = "Server=127.0.0.1;Database=x;User Id=sa;Password=x;Timeout=1;",
                TableName = "T",
                MeasureValueField = "V",
                MeasureResultField = "R",
                TimeFieldName = "T",
                CreateTime = DateTime.Now,
            });

            try
            {
                var ok = await manager.AddOrUpdateAsync(chara);
                Assert.IsTrue(ok, "数据库特性应能成功入队");

                var all = manager.GetAll();
                var hit = all.FirstOrDefault(x => x.CharacteristicId == chara.Id);
                Assert.IsNotNull(hit, "GetAll 应包含该特性");
                Assert.AreEqual("UT-DB特性", hit.FeatureName);
                Assert.AreEqual(SPCDataSource.Database, hit.DataSource);
            }
            finally
            {
                manager.Stop(chara.Id);
                manager.Remove(chara.Id);
                await sourceRepo.DeleteByCharacteristicIdAsync(chara.Id);
                await charaRepo.DeleteAsync(chara.Id);
            }
        }

        /// <summary>
        /// Manager：未知 CollectionMethod → AddOrUpdate 返回 false。
        /// </summary>
        [TestMethod]
        public async Task Manager_AddOrUpdate_UnknownMethod_ReturnsFalse()
        {
            var manager = _container.Resolve<SPCDataCollectionManager>();
            var ok = await manager.AddOrUpdateAsync(new Tb_SPC_Characteristic
            {
                Id = -1,
                Name = "无方法",
                CollectionMethod = "NoSuchCollectorXYZ",
                DataSource = SPCDataSource.Database,
                EquipmentId = 1,
            });
            Assert.IsFalse(ok);
        }

        /// <summary>
        /// Manager：PLC 特性在缺少 SPC_PlcPoint 映射时 AddOrUpdate 返回 false。
        /// </summary>
        [TestMethod]
        public async Task Manager_AddOrUpdate_PlcWithoutMapping_ReturnsFalse()
        {
            var manager = _container.Resolve<SPCDataCollectionManager>();
            var ok = await manager.AddOrUpdateAsync(new Tb_SPC_Characteristic
            {
                Id = -2,
                Name = "PLC无映射",
                CollectionMethod = "PlcCollectStatus",
                DataSource = SPCDataSource.PLC,
                EquipmentId = 1,
            });
            Assert.IsFalse(ok, "无 SPC_PlcPoint 行时应返回 false");
        }

        /// <summary>
        /// Manager：PLC 特性完整映射（SPC_PlcPoint + PlcPoint + Connection）可入队。
        /// </summary>
        [TestMethod]
        public async Task Manager_AddOrUpdate_PlcWithMapping_Success()
        {
            var charaRepo = _container.Resolve<Tb_SPC_CharacteristicRepository>();
            var spcPlcRepo = _container.Resolve<Tb_SPC_PlcPointRepository>();
            var plcPointRepo = _container.Resolve<Tb_PlcPointRepository>();
            var plcConnRepo = _container.Resolve<Tb_PlcConnectionRepository>();
            var manager = _container.Resolve<SPCDataCollectionManager>();

            // ExecuteReturnIdentity 才能拿到自增 Id（InsertableAsync 只返回影响行数）
            var connId = await plcConnRepo.ExecuteReturnIdentityAsync(new Tb_PlcConnection
            {
                EquipmentId = 0,
                IpAddress = "127.0.0.1",
                Port = PlcPort,
                CreateTime = DateTime.Now,
            });

            var plcPoint = new Tb_PlcPoint
            {
                PlcConnectionId = connId,
                PointName = "UT-测量结果",
                Area = "D",
                Address = "200",
                DataType = "Int16",
                Length = 1,
            };
            var db = _container.Resolve<ISqlSugarClient>();
            plcPoint.Id = await db.Insertable(plcPoint).ExecuteReturnIdentityAsync();

            var chara = new Tb_SPC_Characteristic
            {
                Name = "UT-PLC特性",
                CharacteristicNo = $"UTPLC-{Guid.NewGuid():N}".Substring(0, 12),
                CollectionMethod = "PlcCollectStatus",
                DataSource = SPCDataSource.PLC,
                PartCategory = "外壳",
                EquipmentId = 1,
                CreateTime = DateTime.Now,
            };
            chara.Id = await charaRepo.InsertAsync(chara);
            await spcPlcRepo.InsertAsync(new Tb_SPC_PlcPoint
            {
                CharacteristicId = chara.Id,
                PlcPointId = plcPoint.Id,
                CreateTime = DateTime.Now,
            });

            try
            {
                var ok = await manager.AddOrUpdateAsync(chara);
                Assert.IsTrue(ok, "完整 PLC 映射应入队成功");

                var hit = manager.GetAll().FirstOrDefault(x => x.CharacteristicId == chara.Id);
                Assert.IsNotNull(hit);
                Assert.AreEqual(SPCDataSource.PLC, hit.DataSource);
            }
            finally
            {
                manager.Stop(chara.Id);
                manager.Remove(chara.Id);
                await spcPlcRepo.DeleteByCharacteristicIdAsync(chara.Id);
                await charaRepo.DeleteAsync(chara.Id);
                await plcPointRepo.DeleteableAsync(x => x.Id == plcPoint.Id);
                await plcConnRepo.DeleteableAsync(x => x.Id == connId);
            }
        }

        /// <summary>
        /// Manager：Start / Stop / SetEnabled / GetAll 状态流转。
        /// </summary>
        [TestMethod]
        public async Task Manager_StartStopSetEnabled_GetAll()
        {
            var charaRepo = _container.Resolve<Tb_SPC_CharacteristicRepository>();
            var sourceRepo = _container.Resolve<Tb_SPC_CollectionSourceRepository>();
            var manager = _container.Resolve<SPCDataCollectionManager>();

            var chara = new Tb_SPC_Characteristic
            {
                Name = "UT-启停",
                CharacteristicNo = $"UTEN-{Guid.NewGuid():N}".Substring(0, 12),
                CollectionMethod = "DBMeasureResult",
                DataSource = SPCDataSource.Database,
                PartCategory = "外壳",
                EquipmentId = 1,
                CreateTime = DateTime.Now,
            };
            chara.Id = await charaRepo.InsertAsync(chara);
            await sourceRepo.InsertAsync(new Tb_SPC_CollectionSource
            {
                CharacteristicId = chara.Id,
                CreateTime = DateTime.Now,
            });

            try
            {
                Assert.IsTrue(await manager.AddOrUpdateAsync(chara));

                var startRe = manager.Start(chara.Id);
                Assert.IsTrue(startRe.IsSuccess, startRe.Message);

                var afterStart = manager.GetAll().First(x => x.CharacteristicId == chara.Id);
                Assert.IsTrue(afterStart.IsRunning);

                var stopRe = manager.Stop(chara.Id);
                Assert.IsTrue(stopRe.IsSuccess);
                var afterStop = manager.GetAll().First(x => x.CharacteristicId == chara.Id);
                Assert.IsFalse(afterStop.IsRunning);

                var disRe = manager.SetEnabled(chara.Id, false);
                Assert.IsTrue(disRe.IsSuccess);
                var afterDis = manager.GetAll().First(x => x.CharacteristicId == chara.Id);
                Assert.IsFalse(afterDis.IsEnabled);

                // 未加载特性 Start 应失败
                var miss = manager.Start(-999);
                Assert.IsFalse(miss.IsSuccess);
            }
            finally
            {
                manager.Remove(chara.Id);
                await sourceRepo.DeleteByCharacteristicIdAsync(chara.Id);
                await charaRepo.DeleteAsync(chara.Id);
            }
        }

        /// <summary>
        /// Manager：未知特性 Id 的 Stop/SetEnabled 返回失败。
        /// </summary>
        [TestMethod]
        public void Manager_Control_UnknownId_Fails()
        {
            var manager = _container.Resolve<SPCDataCollectionManager>();
            Assert.IsFalse(manager.Stop(-12345).IsSuccess);
            Assert.IsFalse(manager.SetEnabled(-12345, true).IsSuccess);
        }

        /// <summary>
        /// Manager：StartAll / StopAll 批量启停不抛异常。
        /// </summary>
        [TestMethod]
        public void Manager_StartAllStopAll_DoesNotThrow()
        {
            var manager = _container.Resolve<SPCDataCollectionManager>();
            manager.StartAll();
            manager.StopAll();
            Assert.IsTrue(manager.GetAll() is System.Collections.Generic.List<SPCDataCollectionInfo>);
        }

        /// <summary>
        /// Worker 计数与 IsEnabled 默认值。
        /// </summary>
        [TestMethod]
        public void Manager_DefaultFlags()
        {
            var manager = _container.Resolve<SPCDataCollectionManager>();
            Assert.IsTrue(manager.WorkerCount >= 1);
            Assert.IsTrue(manager.IsEnabled);
            Assert.IsFalse(manager.WorkersRunning); // 未 LoadAsync
        }

        #endregion

        #region 型号同步联动

        /// <summary>
        /// LoadAsync 应联动启动型号同步（若启用），并启动 Worker。
        /// 注意：会加载库中全部特性——测试库可能有脏数据，只断言 Worker 运行。
        /// </summary>
        [TestMethod]
        public async Task Manager_LoadAsync_StartsWorkersAndModelSync()
        {
            var manager = _container.Resolve<SPCDataCollectionManager>();
            var sync = _container.Resolve<CurrentModelSyncService>();
            var wasEnabled = sync.IsEnabled;

            try
            {
                sync.IsEnabled = true;
                await manager.LoadAsync();

                Assert.IsTrue(manager.WorkersRunning, "LoadAsync 后 Worker 应在运行");
                Assert.IsTrue(sync.IsRunning, "型号同步应随 SPC 采集联动启动");
            }
            finally
            {
                manager.StopWorkers();
                sync.Stop();
                sync.IsEnabled = wasEnabled;
            }
        }

        /// <summary>
        /// StopWorkers 后 Worker 停止，但型号同步可保持独立运行。
        /// </summary>
        [TestMethod]
        public async Task Manager_StopWorkers_ModelSyncCanStayRunning()
        {
            var manager = _container.Resolve<SPCDataCollectionManager>();
            var sync = _container.Resolve<CurrentModelSyncService>();
            var wasEnabled = sync.IsEnabled;

            try
            {
                sync.IsEnabled = true;
                sync.Start();
                await Task.Delay(150);
                Assert.IsTrue(sync.IsRunning);

                await manager.LoadAsync();
                manager.StopWorkers();

                Assert.IsFalse(manager.WorkersRunning, "SPC Worker 应已停止");
                Assert.IsTrue(sync.IsRunning, "型号同步应独立继续运行");
            }
            finally
            {
                manager.StopWorkers();
                sync.Stop();
                sync.IsEnabled = wasEnabled;
            }
        }

        #endregion

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                var manager = _container.Resolve<SPCDataCollectionManager>();
                manager.StopWorkers();
            }
            catch { }
            try
            {
                _container.Resolve<CurrentModelSyncService>().Stop();
            }
            catch { }

            _plcServer?.ServerClose();
            _container?.Dispose();
        }
    }
}
