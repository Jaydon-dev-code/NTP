using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;
using System;

namespace SL.MLineDataPrecisionTracking.Models.Entities.Mes
{
    /// <summary>
    /// MES 生产工单（接口一 /order/release）
    /// </summary>
    [SugarTable("Mes_Order")]
    public class Tb_MesOrder
    {
        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 工单号
        /// </summary>
        [SugarColumn(ColumnDescription = "工单号")]
        public string WorkOrderNo { get; set; }

        /// <summary>
        /// 成品料号
        /// </summary>
        [SugarColumn(ColumnDescription = "成品料号", IsNullable = true)]
        public string MaterialCode { get; set; }

        /// <summary>
        /// 成品物料描述
        /// </summary>
        [SugarColumn(ColumnDescription = "成品物料描述", IsNullable = true)]
        public string MaterialName { get; set; }

        /// <summary>
        /// 计划数量
        /// </summary>
        [SugarColumn(ColumnDescription = "计划数量", IsNullable = true)]
        public double PlanQty { get; set; }

        /// <summary>
        /// 计划生产日期
        /// </summary>
        [SugarColumn(ColumnDescription = "计划生产日期", IsNullable = true)]
        public DateTime? PlanDate { get; set; }

        /// <summary>
        /// 订单状态：0待执行 1执行中（MesOrderStatusEnum）
        /// </summary>
        [SugarColumn(ColumnDescription = "订单状态")]
        public MesOrderStatusEnum Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnDescription = "创建时间")]
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
