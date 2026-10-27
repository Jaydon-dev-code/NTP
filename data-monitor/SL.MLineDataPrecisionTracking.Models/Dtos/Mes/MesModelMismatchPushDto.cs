using System;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Mes
{
    /// <summary>
    /// 型号不一致告警推送报文（SignalR 推前台）
    /// </summary>
    public class MesModelMismatchPushDto
    {
        /// <summary>
        /// 异常记录ID（前台记录开始/结束时回传）
        /// </summary>
        public int RecordId { get; set; }

        /// <summary>
        /// 三方库最新型号
        /// </summary>
        public string ThirdPartyModel { get; set; }

        /// <summary>
        /// 执行中订单型号（成品料号）
        /// </summary>
        public string OrderModel { get; set; }

        /// <summary>
        /// 执行中工单号
        /// </summary>
        public string WorkOrderNo { get; set; }

        /// <summary>
        /// 告警推送时间
        /// </summary>
        public DateTime PushTime { get; set; }
    }
}
