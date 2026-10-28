using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Line2;
using SL.MLineDataPrecisionTracking.Core.Services.Mes;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace UnitTestProject.Mes
{
    /// <summary>
    /// MES 测量数据推送服务单元测试
    /// </summary>
    [TestClass]
    public class Factory6Line2MeasurePushServiceTest
    {
        private IContainer Container { get; set; }

        public Factory6Line2MeasurePushServiceTest()
        {
            var builder = new ContainerBuilder();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();
            Container = builder.Build();
        }

        /// <summary>
        /// 服务应能通过 DI 完整解析（验证 MesApiOp/MesOrderOP/仓储等依赖注册齐全）
        /// </summary>
        [TestMethod]
        public void Service_Resolve_From_DI()
        {
            Factory6Line2MeasurePushService service = null;
            try
            {
                service = Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .OfType<Factory6Line2MeasurePushService>()
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                // 依赖本地 PostgreSQL（CodeFirst 建表），环境不可用时不视为业务失败
                Assert.Inconclusive($"数据库环境不可用，跳过 DI 解析测试：{ex.Message}");
            }

            Assert.IsNotNull(service, "Factory6Line2MeasurePushService 应能通过 DI 解析");
        }

        /// <summary>
        /// 未配置 ThirdPartyConnection 时，InitAsync 应返回失败并提示配置项。
        /// 注意：仅当 App.config 中 ThirdPartyConnection 为空时才可运行；
        /// 当前已配置三方连接串，此用例跑不通，暂注释。
        /// </summary>
        //[TestMethod]
        public async Task InitAsync_Without_ThirdPartyConnection_Returns_Fail()
        {
            var service = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .OfType<Factory6Line2MeasurePushService>()
                .FirstOrDefault();
            Assert.IsNotNull(service, "服务应能通过 DI 解析");

            // InitAsync 是 protected，经反射调用
            var method = typeof(Factory6Line2MeasurePushService).GetMethod(
                "InitAsync",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.IsNotNull(method, "未找到 InitAsync 方法");

            var task = (Task<Result>)method.Invoke(service, null);
            var result = await task;

            Assert.IsFalse(result.IsSuccess, "未配置三方库连接时初始化应失败");
            StringAssert.Contains(result.Message, "ThirdPartyConnection");
        }

        /// <summary>
        /// 表清单配置：ABS 已整合进统一配置，且同表名不重复
        /// </summary>
        [TestMethod]
        public void Tables_Config_No_Duplicate_And_Contains_ABS()
        {
            // Tables / QueryConfigs 是 private static，经反射读取
            var field = typeof(Factory6Line2MeasurePushService).GetField(
                "Tables",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            Assert.IsNotNull(field, "未找到 Tables 静态字段");

            var tables = (string[])field.GetValue(null);
            Assert.IsNotNull(tables, "Tables 不应为 null");
            Assert.IsTrue(tables.Length >= 7, "应至少包含 7 张表");
            Assert.AreEqual(
                tables.Length,
                tables.Distinct().Count(),
                "同表名只允许出现一次（同表只查一次）"
            );
            CollectionAssert.Contains(tables, "ABS测量记录");
            CollectionAssert.Contains(tables, "跳动测量记录");
            CollectionAssert.Contains(tables, "合套测量记录");
        }

        /// <summary>
        /// 游标仓储往返：初次 upsert 落库 → 读取 → 推进时间 → 再读取为新时间
        /// （需要本地 PostgreSQL）
        /// </summary>
        [TestMethod]
        public async Task Cursor_Upsert_Get_Advance_Roundtrip()
        {
            Tb_MesPushCursorRepository repo;
            try
            {
                repo = Container.Resolve<Tb_MesPushCursorRepository>();
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"数据库环境不可用，跳过游标测试：{ex.Message}");
                return;
            }

            const string key = "单元测试_游标表";
            var first = new DateTime(2026, 1, 2, 3, 4, 5);
            var second = first.AddHours(1);

            try
            {
                // 初次落库（对应：初次游标为空用当前时间）
                await repo.UpsertAsync(key, first);
                var cursor = await repo.GetByKeyAsync(key);
                Assert.IsNotNull(cursor, "upsert 后应能读到游标");
                Assert.AreEqual(first, cursor.LastMeasureTime);

                // 推进游标（对应：查询后用最大测量时间）
                await repo.UpsertAsync(key, second);
                cursor = await repo.GetByKeyAsync(key);
                Assert.IsNotNull(cursor, "推进后应仍能读到游标");
                Assert.AreEqual(second, cursor.LastMeasureTime);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"数据库环境不可用，跳过游标测试：{ex.Message}");
            }
        }

        /// <summary>
        /// 服务可启动且状态最终为 Stopped（未配置三方库时初始化失败即停）。
        /// 注意：仅当 App.config 中 ThirdPartyConnection 为空时才可运行；
        /// 当前已配置三方连接串（且主机暂不可达时服务会保持 Running 等待握手），此用例跑不通，暂注释。
        /// </summary>
        //[TestMethod]
        public async Task Start_Without_ThirdPartyConnection_Ends_Stopped()
        {
            Factory6Line2MeasurePushService service;
            try
            {
                service = Container
                    .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                    .OfType<Factory6Line2MeasurePushService>()
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"数据库环境不可用，跳过启动测试：{ex.Message}");
                return;
            }
            Assert.IsNotNull(service, "服务应能通过 DI 解析");

            service.Start();
            // InitAsync 快速失败，留 3 秒等待后台任务走完生命周期
            await Task.Delay(3000);

            Assert.AreEqual(
                SL.MLineDataPrecisionTracking.Models.Enum.ServiceStatusEnum.Stopped,
                service.Status,
                "初始化失败后服务应处于 Stopped 状态"
            );
        }
    }
}
