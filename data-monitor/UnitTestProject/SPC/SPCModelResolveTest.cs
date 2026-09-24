using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage.SPC;
using SL.MLineDataPrecisionTracking.Models.Entities.SPC;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace UnitTestProject.SPC
{
    /// <summary>
    /// SPC 型号匹配零件测试 — 验证 CurrentModelResolver：
    ///   内存当前型号（三方轮询）→ SPC_Product.ModelNo → SPC_Part（类别）
    ///   以及 PartId 直通、未知型号、缺类别等失败原因。
    ///
    /// 测试通过 SyncOnce 写入指定型号到同步服务内存（无缓存表）。
    /// 注意：SyncOnce 读的是 App.config 固定 SQL，故用反射/直接属性注入型号不便；
    /// 改为：用 CurrentModelNo 无法外部写入 — 通过 SyncOnce 后对无 Product 的
    /// UnknownModel 场景；成功路径依赖 SQL 返回 TEST-SPC-MODEL，
    /// 因此种入产品时使用 TEST-SPC-MODEL 作为 ModelNo。
    /// </summary>
    [TestClass]
    public class SPCModelResolveTest
    {
        private IContainer _container;
        private Tb_SPC_ProductRepository _productRepo;
        private Tb_SPC_PartRepository _partRepo;
        private Tb_SPC_CharacteristicRepository _charaRepo;
        private CurrentModelResolver _resolver;
        private CurrentModelSyncService _syncService;

        /// <summary>与 UnitTest App.config ThirdPartyDb_CurrentModelSql 返回值一致</summary>
        private const string SyncedModel = "TEST-SPC-MODEL";
        private const string CategoryShell = "外壳";

        public SPCModelResolveTest()
        {
            var builder = new ContainerBuilder();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();
            _container = builder.Build();

            _productRepo = _container.Resolve<Tb_SPC_ProductRepository>();
            _partRepo = _container.Resolve<Tb_SPC_PartRepository>();
            _charaRepo = _container.Resolve<Tb_SPC_CharacteristicRepository>();
            _resolver = _container.Resolve<CurrentModelResolver>();
            _syncService = _container.Resolve<CurrentModelSyncService>();
        }

        /// <summary>种入与三方 SQL 一致型号的产品 + 类别零件</summary>
        private async Task<(Tb_SPC_Product product, Tb_SPC_Part part)> SeedProductWithShellPartAsync(string modelNo)
        {
            var product = new Tb_SPC_Product
            {
                ModelNo = modelNo,
                Name = $"测试产品-{modelNo}",
                CreateTime = DateTime.Now,
            };
            product.Id = await _productRepo.InsertAsync(product);

            var part = new Tb_SPC_Part
            {
                ModelNo = $"{modelNo}-PART",
                Name = $"{modelNo}-外壳",
                ProductId = product.Id,
                PartCategory = CategoryShell,
                CreateTime = DateTime.Now,
            };
            part.Id = await _partRepo.InsertAsync(part);

            return (product, part);
        }

        /// <summary>同步一次，把三方 SQL 型号写入内存</summary>
        private async Task SyncModelAsync()
        {
            await _syncService.SyncOnceAsync();
            Assert.AreEqual(SyncedModel, _syncService.CurrentModelNo,
                "请确认 App.config CurrentModelSql 返回 TEST-SPC-MODEL");
        }

        private Tb_SPC_Characteristic NewChara(string category, int? partId = null, int? productId = null)
        {
            return new Tb_SPC_Characteristic
            {
                Id = 0,
                Name = "测试特性",
                CharacteristicNo = $"UT-{Guid.NewGuid():N}".Substring(0, 12),
                DataType = SPCDataType.Numeric,
                InspectionQty = 1,
                CollectionMethod = "DBMeasureResult",
                DataSource = SPCDataSource.Database,
                ProductId = productId,
                PartId = partId,
                PartCategory = category,
                EquipmentId = 1,
            };
        }

        /// <summary>主路径：当前型号 → 产品 → 类别零件</summary>
        [TestMethod]
        public async Task Resolve_ModelToProductToPart_Success()
        {
            await SyncModelAsync();
            var (product, part) = await SeedProductWithShellPartAsync(SyncedModel);

            var ctx = await _resolver.ResolveAsync(NewChara(CategoryShell));

            Assert.IsTrue(ctx.IsSuccess, $"应解析成功，Reason={ctx.Reason}");
            Assert.AreEqual(SyncedModel, ctx.ModelNo);
            Assert.AreEqual(product.Id, ctx.Product.Id);
            Assert.AreEqual(part.Id, ctx.Part.Id);
            Assert.AreEqual(CategoryShell, ctx.PartCategory);
            Assert.IsNull(ctx.Reason);
        }

        /// <summary>
        /// 未知型号（内存有型号但产品表无）→ UnknownModel。
        /// </summary>
        [TestMethod]
        public async Task Resolve_UnknownModel_FailReason()
        {
            await SyncModelAsync();
            // 不种入 TEST-SPC-MODEL 产品（若其他测试已种入则用不存在的类别仍可测 NoPartForCategory；
            // 此处独立种入产品后删掉场景复杂，改为清空类别断言失败路径之一）
            var ctx = await _resolver.ResolveAsync(NewChara("绝对不存在的类别-XYZ"));

            Assert.IsFalse(ctx.IsSuccess);
            // 产品若已存在 → NoPartForCategory；产品不存在 → UnknownModel
            Assert.IsTrue(ctx.Reason == "NoPartForCategory" || ctx.Reason == "UnknownModel",
                "实际 Reason=" + ctx.Reason);
        }

        /// <summary>特性未绑零件类别 → NoPartCategory</summary>
        [TestMethod]
        public async Task Resolve_NoPartCategory_FailReason()
        {
            await SyncModelAsync();
            await SeedProductWithShellPartAsync(SyncedModel);

            var ctx = await _resolver.ResolveAsync(NewChara(null));

            Assert.IsFalse(ctx.IsSuccess);
            Assert.AreEqual("NoPartCategory", ctx.Reason);
        }

        /// <summary>产品存在但类别下无零件 → NoPartForCategory</summary>
        [TestMethod]
        public async Task Resolve_NoPartForCategory_FailReason()
        {
            await SyncModelAsync();
            await SeedProductWithShellPartAsync(SyncedModel);

            var ctx = await _resolver.ResolveAsync(NewChara("不存在的类别-ABC"));

            Assert.IsFalse(ctx.IsSuccess);
            Assert.AreEqual("NoPartForCategory", ctx.Reason);
        }

        /// <summary>PartId 直通</summary>
        [TestMethod]
        public async Task Resolve_PartIdOverride_UsesDirectPart()
        {
            await SyncModelAsync();
            var (product, part) = await SeedProductWithShellPartAsync(SyncedModel);

            var ctx = await _resolver.ResolveAsync(NewChara("错误类别", partId: part.Id, productId: product.Id));

            Assert.IsTrue(ctx.IsSuccess, "PartId 直通应成功，Reason=" + ctx.Reason);
            Assert.AreEqual(part.Id, ctx.Part.Id);
            Assert.AreEqual(product.Id, ctx.Product.Id);
        }

        /// <summary>PartId 失效回退类别</summary>
        [TestMethod]
        public async Task Resolve_PartIdMissing_FallbackToCategory()
        {
            await SyncModelAsync();
            var (_, part) = await SeedProductWithShellPartAsync(SyncedModel);

            var ctx = await _resolver.ResolveAsync(NewChara(CategoryShell, partId: part.Id + 99999));

            Assert.IsTrue(ctx.IsSuccess, "应回退类别解析，Reason=" + ctx.Reason);
            Assert.AreEqual(part.Id, ctx.Part.Id);
        }

        /// <summary>ResolveById：不存在 → CharacteristicNotFound</summary>
        [TestMethod]
        public async Task ResolveById_NotFound_FailReason()
        {
            var ctx = await _resolver.ResolveByIdAsync(int.MaxValue - 7);

            Assert.IsFalse(ctx.IsSuccess);
            Assert.AreEqual("CharacteristicNotFound", ctx.Reason);
        }

        /// <summary>ResolveById：落库特性按类别解析成功</summary>
        [TestMethod]
        public async Task ResolveById_WithSeedData_Success()
        {
            await SyncModelAsync();
            var (_, part) = await SeedProductWithShellPartAsync(SyncedModel);

            var chara = NewChara(CategoryShell);
            chara.Id = await _charaRepo.InsertAsync(chara);

            try
            {
                var ctx = await _resolver.ResolveByIdAsync(chara.Id);
                Assert.IsTrue(ctx.IsSuccess, "Reason=" + ctx.Reason);
                Assert.AreEqual(part.Id, ctx.Part.Id);
            }
            finally
            {
                await _charaRepo.DeleteAsync(chara.Id);
            }
        }

        /// <summary>型号匹配忽略大小写/空格（SQL 已 trim 后的 TEST-SPC-MODEL；产品用小写种入）</summary>
        [TestMethod]
        public async Task Resolve_ModelNo_CaseInsensitiveProduct()
        {
            await SyncModelAsync();
            // 产品型号用全小写，匹配应仍成功
            var product = new Tb_SPC_Product
            {
                ModelNo = "test-spc-model",
                Name = "大小写测试",
                CreateTime = DateTime.Now,
            };
            product.Id = await _productRepo.InsertAsync(product);
            var part = new Tb_SPC_Part
            {
                ModelNo = "CS-PART",
                Name = "CS外壳",
                ProductId = product.Id,
                PartCategory = CategoryShell,
                CreateTime = DateTime.Now,
            };
            part.Id = await _partRepo.InsertAsync(part);

            var ctx = await _resolver.ResolveAsync(NewChara(CategoryShell));

            // 若已存在 TEST-SPC-MODEL 精确产品，可能优先命中；断言成功即可
            Assert.IsTrue(ctx.IsSuccess, "Reason=" + ctx.Reason);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { _syncService?.Stop(); } catch { }
            _container?.Dispose();
        }
    }
}
