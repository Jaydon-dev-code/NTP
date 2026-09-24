using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage.SPC;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities.SPC;

namespace SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection
{
    /// <summary>
    /// SPC 运行时零件解析器。
    ///
    /// 解析链路：
    ///   当前型号（内存 CurrentModelSyncService.CurrentModelNo，由三方库不间断轮询维护）
    ///     → SPC_Product.ModelNo 匹配产品
    ///     → SPC_Part（ProductId + PartCategory）得到具体零件
    ///
    /// 兼容：特性 PartId 非空时优先直通；失败回退类别路径。
    /// 约束：解析本身不直连三方库（只读内存型号 + 本地主数据）；
    ///      按 (characteristicId, modelNo, ModelVersion) 缓存，换型自动失效。
    /// </summary>
    public class CurrentModelResolver
    {
        private readonly Tb_SPC_ProductRepository _productRepo;
        private readonly Tb_SPC_PartRepository _partRepo;
        private readonly Tb_SPC_CharacteristicRepository _characteristicRepo;
        private readonly CurrentModelSyncService _syncService;

        /// <summary>特性 ID → 解析结果缓存；版本或型号变化后条目过期</summary>
        private readonly ConcurrentDictionary<int, CacheEntry> _cache =
            new ConcurrentDictionary<int, CacheEntry>();

        public CurrentModelResolver(
            Tb_SPC_ProductRepository productRepo,
            Tb_SPC_PartRepository partRepo,
            Tb_SPC_CharacteristicRepository characteristicRepo,
            CurrentModelSyncService syncService)
        {
            _productRepo = productRepo;
            _partRepo = partRepo;
            _characteristicRepo = characteristicRepo;
            _syncService = syncService;
        }

        /// <summary>缓存条目：用 Version + ModelNo 判断是否仍有效</summary>
        private class CacheEntry
        {
            public long Version;
            public string ModelNo;
            public SPCResolvedContext Context;
        }

        /// <summary>
        /// 按特性解析出 当前型号 + 产品 + 零件 上下文。
        /// 型号来自同步服务内存（三方轮询结果），不读本地缓存表。
        /// </summary>
        public async Task<SPCResolvedContext> ResolveAsync(Tb_SPC_Characteristic characteristic)
        {
            if (characteristic == null)
                return new SPCResolvedContext { Reason = "NullCharacteristic" };

            var version = _syncService.ModelVersion;
            var modelNo = _syncService.CurrentModelNo?.Trim();

            // 无当前型号：同步未跑 / 三方空 / 尚未查过
            if (string.IsNullOrEmpty(modelNo))
            {
                return Fail(characteristic.Id, version, null, "NoCurrentModel",
                    "无当前生产型号（未同步或三方库为空）");
            }

            // 缓存命中：同一特性 + 同一型号 + 同一版本
            if (_cache.TryGetValue(characteristic.Id, out var hit)
                && hit.Version == version
                && string.Equals(hit.ModelNo, modelNo, StringComparison.Ordinal))
            {
                return hit.Context;
            }

            var ctx = await ResolveCoreAsync(characteristic, modelNo);
            _cache[characteristic.Id] = new CacheEntry
            {
                Version = version,
                ModelNo = modelNo,
                Context = ctx,
            };
            return ctx;
        }

        /// <summary>
        /// 核心解析：先尝试 PartId 直通，再走「型号→产品→类别→零件」。
        /// 失败路径统一走 Fail()（打日志 + 写缓存）。
        /// </summary>
        private async Task<SPCResolvedContext> ResolveCoreAsync(Tb_SPC_Characteristic c, string modelNo)
        {
            // ── 路径 A：兼容旧数据，特性显式绑定了 PartId ──
            if (c.PartId.HasValue && c.PartId.Value > 0)
            {
                var part = await _partRepo.GetByIdAsync(c.PartId.Value);
                if (part != null)
                {
                    Tb_SPC_Product product = null;
                    if (part.ProductId.HasValue)
                        product = await _productRepo.GetByIdAsync(part.ProductId.Value);

                    // 零件的产品与特性 ProductId 一致（或特性未绑产品）才允许直通
                    if (product != null && (!c.ProductId.HasValue || c.ProductId == product.Id))
                    {
                        return new SPCResolvedContext
                        {
                            ModelNo = modelNo,
                            Product = product,
                            Part = part,
                            PartCategory = part.PartCategory ?? c.PartCategory,
                            Reason = null,
                        };
                    }

                    // 产品对不上 → 不能盲信 PartId，回退类别路径
                    Serilog.Log.Warning("[SPC解析] 特性 {Id} PartId={PartId} 产品不匹配，回退类别解析",
                        c.Id, c.PartId);
                }
                else
                {
                    // 零件已被删 → 回退类别路径
                    Serilog.Log.Warning("[SPC解析] 特性 {Id} PartId={PartId} 不存在，回退类别解析",
                        c.Id, c.PartId);
                }
            }

            // ── 路径 B：型号 → 产品（忽略大小写、去空格） ──
            var productByModel = await _productRepo.GetByModelNoAsync(modelNo);
            if (productByModel == null)
                return Fail(c.Id, _syncService.ModelVersion, modelNo, "UnknownModel",
                    $"未找到型号为 [{modelNo}] 的产品");

            // ── 路径 B 续：特性必须绑定零件类别 ──
            if (string.IsNullOrWhiteSpace(c.PartCategory))
                return Fail(c.Id, _syncService.ModelVersion, modelNo, "NoPartCategory",
                    "特性未绑定零件类别");

            // ── 路径 B 续：产品 + 类别 → 零件（仓库按 Id 升序） ──
            var parts = await _partRepo.GetByProductAndCategoryAsync(productByModel.Id, c.PartCategory);
            if (parts == null || parts.Count == 0)
                return Fail(c.Id, _syncService.ModelVersion, modelNo, "NoPartForCategory",
                    $"产品 [{productByModel.ModelNo}] 下无类别 [{c.PartCategory}] 的零件");

            // 同类别多零件：确定性取最小 Id，并告警提示主数据应保证唯一
            if (parts.Count > 1)
                Serilog.Log.Warning("[SPC解析] 产品 {ProductId} 类别 {Category} 命中 {Count} 个零件，取最小 Id={Id}",
                    productByModel.Id, c.PartCategory, parts.Count, parts[0].Id);

            return new SPCResolvedContext
            {
                ModelNo = modelNo,
                Product = productByModel,
                Part = parts[0],
                PartCategory = c.PartCategory,
                Reason = null,
            };
        }

        /// <summary>
        /// 统一失败出口：打一次 Warning + 写入失败缓存。
        /// 失败也缓存是为了避免同一型号下每 5s Worker 重复刷日志；
        /// ModelVersion/型号变化后缓存自动失效并重试。
        /// </summary>
        private SPCResolvedContext Fail(int charId, long version, string modelNo, string reason, string message)
        {
            Serilog.Log.Warning("[SPC解析] 特性 {Id} 解析失败 {Reason}: {Message}", charId, reason, message);
            var ctx = new SPCResolvedContext
            {
                ModelNo = modelNo,
                Reason = reason,
            };
            _cache[charId] = new CacheEntry
            {
                Version = version,
                ModelNo = modelNo,
                Context = ctx,
            };
            return ctx;
        }

        /// <summary>干跑解析：按特性 ID 加载后走 ResolveAsync（Spc/ResolveCharacteristic 调试接口）</summary>
        public async Task<SPCResolvedContext> ResolveByIdAsync(int characteristicId)
        {
            var chara = await _characteristicRepo.GetByIdAsync(characteristicId);
            if (chara == null)
                return new SPCResolvedContext { Reason = "CharacteristicNotFound" };
            return await ResolveAsync(chara);
        }
    }
}
