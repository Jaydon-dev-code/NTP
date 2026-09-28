using SqlSugar;
using System;

namespace SL.MLineDataPrecisionTracking.Models.Entities.MES
{
    /// <summary>
    /// MES 生产工单（接口一：/order/release 接收落库）
    /// </summary>
    [SugarTable("MesWorkOrder")]
    public class Tb_MesWorkOrder
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>工单号</summary>
        [SugarColumn(ColumnDescription = "工单号", IsNullable = false)]
        public string WorkOrderNo { get; set; }

        /// <summary>成品料号</summary>
        [SugarColumn(ColumnDescription = "成品料号", IsNullable = false)]
        public string MaterialCode { get; set; }

        /// <summary>成品物料描述</summary>
        [SugarColumn(ColumnDescription = "成品物料描述", IsNullable = true)]
        public string MaterialName { get; set; }

        /// <summary>计划数量</summary>
        [SugarColumn(ColumnDescription = "计划数量")]
        public decimal PlanQty { get; set; }

        /// <summary>计划生产日期</summary>
        [SugarColumn(ColumnDescription = "计划生产日期", IsNullable = true)]
        public DateTime? PlanDate { get; set; }

        /// <summary>创建时间</summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
