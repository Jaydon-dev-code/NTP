namespace SL.MLineDataPrecisionTracking.Models.Dtos.Mes
{
    /// <summary>
    /// 型号告警状态查询结果（前台启动/重连时调用 modelCheck/status，补显告警窗）
    /// </summary>
    public class MesModelAlertStatusDto
    {
        /// <summary>
        /// 校验状态：0正常 1告警中（全自动记录，无手动操作）
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 最近一次告警报文（无告警为 null）
        /// </summary>
        public MesModelMismatchPushDto Alert { get; set; }
    }
}
