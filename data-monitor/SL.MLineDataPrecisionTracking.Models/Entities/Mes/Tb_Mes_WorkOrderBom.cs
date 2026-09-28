using SqlSugar;
using System;

namespace SL.MLineDataPrecisionTracking.Models.Entities.MES
{
    /// <summary>
    /// MES 生产工单 BOM 组件（接口一：随工单一同下发）
    /// </summary>
    [SugarTable("Mes_WorkOrderBom")]
    public class Tb_Mes_WorkOrderBom
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>所属工单号</summary>
        [SugarColumn(ColumnDescription = "工单号", IsNullable = false)]
        public string WorkOrderNo { get; set; }

        /// <summary>组件料号</summary>
        [SugarColumn(ColumnDescription = "组件料号", IsNullable = false)]
        public string ComponentCode { get; set; }

        /// <summary>组件名称</summary>
        [SugarColumn(ColumnDescription = "组件名称", IsNullable = true)]
        public string ComponentName { get; set; }

        /// <summary>组件数量</summary>
        [SugarColumn(ColumnDescription = "组件数量")]
        public decimal Qty { get; set; }

        /// <summary>创建时间</summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
