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
using SL.MLineDataPrecisionTracking.Models.Entities.Mes;

namespace UnitTestProject.Mes
{
    /// <summary>
    /// MES 三方库真实连接集成测试。
    /// 依赖：App.config 中 ThirdPartyConnection 指向可达的 SQL Server（当前配置 10.210.192.185）。
    /// 三方库主机可达后，取消各 [TestMethod] 的注释即可运行。
    /// </summary>
    [TestClass]
    public class MesThirdPartyIntegrationTest
    {
        private IContainer Container { get; set; }

        public MesThirdPartyIntegrationTest()
        {
            var builder = new ContainerBuilder();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();
            Container = builder.Build();
        }

        /// <summary>
        /// 三方库连通性：PingAsync 应返回 true
        /// </summary>
        //[TestMethod]
        public async Task ThirdParty_Db_Ping_Should_Succeed()
        {
            var repo = Container.Resolve<Tb_MesMeasureSourceRepository>();
            var ok = await repo.PingAsync();
            Assert.IsTrue(ok, "三方库（ThirdPartyConnection）应可连通");
        }

        /// <summary>
        /// 三方库真实查询：ABS 测量记录用一个较早的时间点应能查到数据，
        /// 且每行包含内码/测量时间列，out 游标最大时间应大于查询起点。
        /// </summary>
        //[TestMethod]
        public async Task ThirdParty_ABS_Query_Should_Return_Rows()
        {
            var repo = Container.Resolve<Tb_MesMeasureSourceRepository>();

            // 用一个足够早的时间点，保证能查到历史数据（最多1000条）
            var start = new DateTime(2020, 1, 1);
            var rows = await repo.GetNewDataAsync(
                tableName: "ABS测量记录",
                timeCol: "测量时间",
                secondCol: "ID",
                secondIsInt: false,
                timeNullable: true,
                lastMeasureTime: start,
                lastSecondValue: string.Empty,
                take: 1000
            );

            Assert.IsTrue(rows.Count > 0, "ABS 测量记录应能查到历史数据");
            Assert.IsTrue(
                rows.All(r => r.ContainsKey("测量时间")),
                "每行应包含 测量时间 列"
            );

            // 验证最大测量时间可从查询结果推导（与服务内游标推进逻辑一致）
            var maxTime = rows.Max(r => Convert.ToDateTime(r["测量时间"]));
            Assert.IsTrue(maxTime > start, "本批最大测量时间应大于查询起点");
        }

        /// <summary>
        /// 三方库真实查询：跳动测量记录（含端跳组+深度组的合并列清单）
        /// </summary>
        //[TestMethod]
        public async Task ThirdParty_Runout_Query_Should_Return_Rows()
        {
            var repo = Container.Resolve<Tb_MesMeasureSourceRepository>();
            var rows = await repo.GetNewDataAsync(
                tableName: "跳动测量记录",
                timeCol: "测量时间",
                secondCol: "工位ID",
                secondIsInt: true,
                timeNullable: false,
                lastMeasureTime: new DateTime(2020, 1, 1),
                lastSecondValue: string.Empty,
                take: 10
            );

            Assert.IsTrue(rows.Count > 0, "跳动测量记录应能查到历史数据");
            Assert.IsTrue(
                rows.All(r => r.ContainsKey("端跳值1") && r.ContainsKey("压装深度")),
                "每行应包含 端跳值1/压装深度 列（对应端跳组+深度组合并查询）"
            );
        }

        /// <summary>
        /// 全链路：服务真实启动 → 三方库有数据时游标应推进。
        /// 运行前建议把 MesPushCursor 中 ABS 记录的游标改为较早时间以便观察推进；
        /// MES 推送地址不可达时推送失败但游标仍会推进（当前实现查询后即推进）。
        /// </summary>
        //[TestMethod]
        public async Task ThirdParty_Service_Start_Cursor_Should_Advance()
        {
            var cursorRepo = Container.Resolve<Tb_MesPushCursorRepository>();
            const string absTable = "ABS测量记录";

            // 将 ABS 游标拨回 2026-01-01，便于观察推进
            var early = new DateTime(2026, 1, 1);
            await cursorRepo.UpsertAsync(absTable, early);

            var service = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .OfType<Factory6Line2MeasurePushService>()
                .First();
            service.Start();

            // 等待至少一个轮询周期（间隔10s + 查询耗时）
            await Task.Delay(20 * 1000);

            var cursor = await cursorRepo.GetByKeyAsync(absTable);
            Assert.IsNotNull(cursor, "游标记录应存在");
            Assert.IsTrue(
                cursor.LastMeasureTime > early,
                $"三方库有新数据时游标应从 {early:yyyy-MM-dd} 推进，实际为 {cursor.LastMeasureTime:yyyy-MM-dd HH:mm:ss}"
            );

            service.Stop();
        }
        [TestMethod]
        public async Task cc()
        {
            var service = Container
             .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
             .OfType<Factory6Line2MeasurePushService>()
             .First();
            service.Start();
            while (true)
            {
               await Task.Delay(2000);
            }
        }

        
        /// <summary>
        /// InitAsync 真实初始化：配置了可达三方库后应成功，且 7 张表游标全部落库。
        /// </summary>
        [TestMethod]
        public async Task ThirdParty_Init_Should_Succeed_And_Create_Cursors()
        {
            var service = Container
                .Resolve<IEnumerable<DataCollectionServiceAbstract>>()
                .OfType<Factory6Line2MeasurePushService>()
                .First();

            // InitAsync 是 protected，经反射调用
            var method = typeof(Factory6Line2MeasurePushService).GetMethod(
                "InitAsync",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            var task = (Task<Result>)method.Invoke(service, null);
            var result = await task;

            Assert.IsTrue(result.IsSuccess, $"初始化应成功，实际：{result.Message}");

            // 7 张表游标均应已落库
            var cursorRepo = Container.Resolve<Tb_MesPushCursorRepository>();
            var expectedTables = new[]
            {
                "ABS测量记录",
                "跳动测量记录",
                "压装测量记录",
                "游隙测量记录",
                "注脂测量记录",
                "法兰盘分选测量记录",
                "合套测量记录",
            };
            foreach (var table in expectedTables)
            {
                var cursor = await cursorRepo.GetByKeyAsync(table);
                Assert.IsNotNull(cursor, $"表 [{table}] 的游标应已初始化落库");
            }
        }
    }
}
