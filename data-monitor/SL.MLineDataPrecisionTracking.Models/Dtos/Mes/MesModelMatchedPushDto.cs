using System;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Mes
{
    /// <summary>
    /// 型号恢复一致推送报文（SignalR 推前台，用于关闭告警窗）
    /// </summary>
    public class MesModelMatchedPushDto
    {
        /// <summary>
        /// 恢复一致的型号
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// 恢复一致的时间
        /// </summary>
        public DateTime MatchTime { get; set; }
    }
}
