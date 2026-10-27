namespace SL.MLineDataPrecisionTracking.Models.Enum
{
    /// <summary>
    /// MES 型号异常记录状态
    /// </summary>
    public enum MesModelAbnormalStatusEnum
    {
        /// <summary>
        /// 待前台记录（已推送告警，等待用户点击开始）
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 记录中（用户已点开始，等待点结束）
        /// </summary>
        Recording = 1,

        /// <summary>
        /// 已结束（记录了结束时间，进入冷却）
        /// </summary>
        Finished = 2,
    }
}
