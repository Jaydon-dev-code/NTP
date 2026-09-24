namespace SL.MLineDataPrecisionTracking.Models.Enum
{
    /// <summary>
    /// 服务类别 — 区分客户端服务和 Web 端服务
    /// </summary>
    public enum ServiceCategory
    {
        /// <summary>客户端服务（WPF，SignalR + 数据库）</summary>
        Client = 0,

        /// <summary>Web 端服务</summary>
        Web = 1,
    }
}
