using SqlSugar;
using System;

namespace SL.MLineDataPrecisionTracking.Models.Entities.Mes
{
    /// <summary>
    /// MES 型号异常记录（三方库型号与执行中订单型号不一致时产生，
    /// 前台点击记录开始/结束时间）
    /// </summary>
    [SugarTable("Mes_ModelAbnormalRecord")]
    public class Tb_MesModelAbnormalRecord
    {
        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 三方库最新型号
        /// </summary>
        [SugarColumn(ColumnDescription = "三方库型号", IsNullable = true, Length = 100)]
        public string ThirdPartyModel { get; set; }

        /// <summary>
        /// 执行中订单型号（成品料号）
        /// </summary>
        [SugarColumn(ColumnDescription = "订单型号", IsNullable = true, Length = 100)]
        public string OrderModel { get; set; }

        /// <summary>
        /// 执行中工单号
        /// </summary>
        [SugarColumn(ColumnDescription = "工单号", IsNullable = true, Length = 100)]
        public string WorkOrderNo { get; set; }

        /// <summary>
        /// 记录开始时间（前台点开始时写入）
        /// </summary>
        [SugarColumn(ColumnDescription = "开始时间", IsNullable = true)]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 记录结束时间（前台点结束时写入）
        /// </summary>
        [SugarColumn(ColumnDescription = "结束时间", IsNullable = true)]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 状态（MesModelAbnormalStatusEnum）：0待记录 1记录中 2已结束
        /// </summary>
        [SugarColumn(ColumnDescription = "状态")]
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnDescription = "创建时间")]
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
