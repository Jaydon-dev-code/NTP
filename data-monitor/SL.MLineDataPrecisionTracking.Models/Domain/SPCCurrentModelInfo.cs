using System;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    /// <summary>
    /// 当前生产型号内存快照（无本地缓存表；由 CurrentModelSyncService 持有）。
    /// 供 Spc/GetCurrentModel 接口与 UI 展示。
    /// </summary>
    public class SPCCurrentModelInfo
    {
        /// <summary>当前生产型号（内存；失败时保留上次成功值）</summary>
        public string ModelNo { get; set; }

        /// <summary>同步状态：Ok / Error / Unknown（尚未查过）</summary>
        public string SyncStatus { get; set; } = "Unknown";

        /// <summary>最近一次失败原因</summary>
        public string LastError { get; set; }

        /// <summary>最近一次查询三方库的时间</summary>
        public DateTime? LastSyncTime { get; set; }
    }
}
