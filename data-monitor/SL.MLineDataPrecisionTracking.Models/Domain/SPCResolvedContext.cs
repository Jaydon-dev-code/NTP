using SL.MLineDataPrecisionTracking.Models.Entities.SPC;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    /// <summary>
    /// SPC 运行时解析结果：一次「当前型号 → 产品 → 零件」的完整上下文。
    /// 由 CurrentModelResolver 产出，经 SPCDataCollectionManager.SetContext 注入采集器。
    /// </summary>
    public class SPCResolvedContext
    {
        /// <summary>解析时使用的当前生产型号（来自同步服务内存，由三方库轮询维护）</summary>
        public string ModelNo { get; set; }

        /// <summary>按 ModelNo 匹配到的产品（SPC_Product）；失败时为 null</summary>
        public Tb_SPC_Product Product { get; set; }

        /// <summary>按 产品+零件类别（或 PartId 直通）得到的具体零件；失败时为 null</summary>
        public Tb_SPC_Part Part { get; set; }

        /// <summary>使用的零件类别：特性绑定的 PartCategory，或 PartId 直通时零件自带类别</summary>
        public string PartCategory { get; set; }

        /// <summary>
        /// 失败原因代码，成功时为 null。常见值：
        ///   NoCurrentModel / UnknownModel / NoPartCategory / NoPartForCategory / CharacteristicNotFound
        /// </summary>
        public string Reason { get; set; }

        /// <summary>是否解析成功（产品与零件均非空）</summary>
        public bool IsSuccess => Product != null && Part != null;
    }
}
