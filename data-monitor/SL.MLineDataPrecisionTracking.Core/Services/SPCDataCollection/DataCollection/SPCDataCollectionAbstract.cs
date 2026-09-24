using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection.DataCollection
{
    /// <summary>
    /// SPC 采集器基类（策略模式）。
    /// 子类：DBMeasureResult（外部测量库）、PlcCollectStatus（PLC 点位）。
    /// 由 SPCDataCollectionManager 按 characteristic.CollectionMethod 类名反射创建。
    ///
    /// 生命周期：
    ///   Init(args)        — 按数据来源初始化连接/点位
    ///   SetContext(ctx)   — Manager 解析「当前型号→产品→零件」后注入上下文（可多次，换型时刷新）
    ///   Execute()         — Worker 每 5s 调用一次采集
    /// </summary>
    public abstract class SPCDataCollectionAbstract : ISPCDataCollection
    {
        /// <summary>展示用特性名称（Init 时由 args[0] 写入）</summary>
        public string FeatureName => _featureName;

        /// <summary>子类实现的名称字段（与类名/配置的 CollectionMethod 对应）</summary>
        public abstract string _featureName { get; set; }

        /// <summary>采集类型（由子类声明）：Database / PLC</summary>
        public abstract SPCDataSource DataSource { get; }

        /// <summary>描述（由子类声明，采集方式下拉展示）</summary>
        public abstract string Description { get; }

        /// <summary>
        /// 运行时解析出的 当前型号/产品/零件 上下文。
        /// 子类在 Execute 内可通过 ResolvedContext 判断 IsSuccess / 取 Part 信息；
        /// ctx 为 null 或解析失败时应跳过需要零件信息的处理逻辑。
        /// </summary>
        protected SPCResolvedContext ResolvedContext { get; private set; }

        /// <summary>
        /// 初始化入口：objs[0] 固定为特性名称，其余由子类 InitCollectionInfo 消费。
        /// </summary>
        public void Init(params object[] objs)
        {
            _featureName = objs[0].ToString();
            InitCollectionInfo(objs);
        }

        /// <summary>
        /// 由 Manager 在「任务加载完成 / 换型刷新」时注入解析上下文。
        /// 默认只保存到 ResolvedContext；子类若有特殊注入需求可 override。
        /// </summary>
        public virtual void SetContext(SPCResolvedContext ctx)
        {
            ResolvedContext = ctx;
        }

        /// <summary>子类初始化细节（建连接、绑点位等）</summary>
        protected abstract void InitCollectionInfo(object[] objs);

        /// <summary>执行一次采集（Worker 按 Interval 调用）</summary>
        public abstract void Execute();
    }
}
