using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection
{
    /// <summary>
    /// 工位采集接口 — 以工位为对象的采集单元。
    /// 由管理服务统筹调度：注册时调用 InitAsync 初始化，成功后才进入采集队列；
    /// 队列中的工位由管理服务的 Worker 并行调用 DataCollectionAsync。
    /// </summary>
    public interface IStationCollection
    {
        /// <summary>工位Id（设备编号，string）</summary>
        string StationId { get; }

        /// <summary>工位名称（设备名）</summary>
        string StationName { get; }

        /// <summary>是否正在运行（由管理服务设置）</summary>
        bool IsRunning { get; set; }

        /// <summary>当前执行状态描述</summary>
        string Description { get; }

        /// <summary>采集发布 Topic</summary>
        string Topic { get; }

        /// <summary>采集间隔（上次采集距今超过该间隔才执行）</summary>
        System.TimeSpan Interval { get; }

        /// <summary>
        /// 初始化：注册时由管理服务调用，传入工位所需数据。
        /// 返回失败则不加入采集队列。
        /// </summary>
        Task<Result> InitAsync(object initData);

        /// <summary>
        /// 采集：由管理服务的 Worker 并行调用。内部应使用超时隔离，避免阻塞 Worker。
        /// </summary>
        Task<Result> DataCollectionAsync();

        /// <summary>
        /// 停止/释放资源（注销时调用）。
        /// </summary>
        Task StopAsync();
    }
}
