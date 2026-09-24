using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection;

namespace UnitTestProject.SPC
{
    /// <summary>
    /// SPC 型号同步测试 — 验证「从数据库获取当前型号」链路（无本地缓存表）：
    ///   App.config ThirdPartyDb_* → CurrentModelSyncService.SyncOnceAsync
    ///   → 仅更新内存 ModelNo / SyncStatus / ModelVersion。
    ///
    /// 前置：Unit Test App.config 已配置
    ///   ThirdPartyDb_Enabled=true
    ///   ThirdPartyDb_CurrentModelSql=SELECT 'TEST-SPC-MODEL'
    /// </summary>
    [TestClass]
    public class SPCModelSyncTest
    {
        private IContainer _container;

        public SPCModelSyncTest()
        {
            var builder = new ContainerBuilder();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();
            _container = builder.Build();
        }

        [TestMethod]
        public void ThirdPartyDbConfig_IsConfigured_WhenAppConfigHasKeys()
        {
            Assert.IsTrue(ThirdPartyDbConfig.Enabled, "ThirdPartyDb_Enabled 应为 true");
            Assert.IsTrue(ThirdPartyDbConfig.IsConfigured, "连接串与 CurrentModelSql 均应已配置");
            Assert.IsFalse(string.IsNullOrWhiteSpace(ThirdPartyDbConfig.ConnectionString));
            Assert.IsFalse(string.IsNullOrWhiteSpace(ThirdPartyDbConfig.CurrentModelSql));
            Assert.IsTrue(ThirdPartyDbConfig.SyncIntervalSeconds > 0);
            Assert.AreEqual(SqlSugar.DbType.PostgreSQL, ThirdPartyDbConfig.GetDbType());
        }

        /// <summary>
        /// 核心：执行一次同步后，内存中应有 SQL 返回的型号（不写任何本地表）。
        /// </summary>
        [TestMethod]
        public async Task SyncOnce_ReadModelFromDatabase_StoresInMemory()
        {
            var sync = _container.Resolve<CurrentModelSyncService>();

            await sync.SyncOnceAsync();

            var info = sync.GetCurrentModel();
            Assert.AreEqual("G3-2B219", info.ModelNo, "型号应等于 ThirdParty SQL 返回值");
            Assert.AreEqual("Ok", info.SyncStatus);
            Assert.IsTrue(info.LastSyncTime.HasValue);
            Assert.AreEqual("G3-2B219", sync.CurrentModelNo);
            Assert.IsTrue(sync.ModelVersion >= 1, "首次写入型号后 ModelVersion 应至少为 1");
        }

        /// <summary>
        /// 相同型号重复同步：ModelVersion 不再增加。
        /// </summary>
        [TestMethod]
        public async Task SyncOnce_SameModel_DoesNotBumpVersion()
        {
            var sync = _container.Resolve<CurrentModelSyncService>();

            await sync.SyncOnceAsync();
            var v1 = sync.ModelVersion;

            await sync.SyncOnceAsync();
            var v2 = sync.ModelVersion;

            Assert.AreEqual(v1, v2, "相同型号重复同步不应递增 ModelVersion");
        }

        /// <summary>
        /// GetCurrentModel 返回内存快照。
        /// </summary>
        [TestMethod]
        public async Task GetCurrentModel_ReturnsInMemorySnapshot()
        {
            var sync = _container.Resolve<CurrentModelSyncService>();

            await sync.SyncOnceAsync();
            var info = sync.GetCurrentModel();

            Assert.AreEqual("G3-2B219", info.ModelNo);
            Assert.AreEqual("Ok", info.SyncStatus);
        }

        /// <summary>
        /// Start 后 IsRunning=true，Stop 后 false。
        /// </summary>
        [TestMethod]
        public async Task StartStop_IsRunningFlag()
        {
            var sync = _container.Resolve<CurrentModelSyncService>();
            sync.IsEnabled = true;

            try
            {
                sync.Start();
                await Task.Delay(200);
                Assert.IsTrue(sync.IsRunning, "Start 后应处于运行状态");

                sync.Stop();
                Assert.IsFalse(sync.IsRunning, "Stop 后应停止");
            }
            finally
            {
                sync.Stop();
            }
        }

        /// <summary>
        /// 禁用服务时 Start 不应启动循环。
        /// </summary>
        [TestMethod]
        public void Start_WhenDisabled_DoesNotRun()
        {
            var sync = _container.Resolve<CurrentModelSyncService>();
            var wasEnabled = sync.IsEnabled;
            try
            {
                sync.IsEnabled = false;
                sync.Start();
                Assert.IsFalse(sync.IsRunning, "禁用后 Start 不应运行");
            }
            finally
            {
                sync.IsEnabled = wasEnabled;
                sync.Stop();
            }
        }

        /// <summary>
        /// 启动轮询后内存型号被写入（不间断查询三方库的启动即查路径）。
        /// </summary>
        [TestMethod]
        public async Task Start_PollsThirdParty_WritesMemoryModel()
        {
            var sync = _container.Resolve<CurrentModelSyncService>();
            sync.IsEnabled = true;
            try
            {
                sync.Start();
                // 等待 Loop 首次 SyncOnce 完成
                var deadline = DateTime.UtcNow.AddSeconds(5);
                while (DateTime.UtcNow < deadline
                       && sync.GetCurrentModel().SyncStatus != "Ok")
                {
                    await Task.Delay(100);
                }

                var info = sync.GetCurrentModel();
                Assert.AreEqual("Ok", info.SyncStatus, "轮询应成功，LastError=" + info.LastError);
                Assert.AreEqual("TEST-SPC-MODEL", info.ModelNo);
            }
            finally
            {
                sync.Stop();
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                var sync = _container.Resolve<CurrentModelSyncService>();
                sync.Stop();
            }
            catch { }
            _container?.Dispose();
        }
    }
}
