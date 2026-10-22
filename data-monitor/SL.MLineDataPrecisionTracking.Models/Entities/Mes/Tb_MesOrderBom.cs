using SqlSugar;
using System;

namespace SL.MLineDataPrecisionTracking.Models.Entities.Mes
{
    /// <summary>
    /// MES 生产工单 BOM 组件（接口一 orderBoms）
    /// </summary>
    [SugarTable("MesOrderBom")]
    public class Tb_MesOrderBom
    {
        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 所属工单ID
        /// </summary>
        [SugarColumn(ColumnDescription = "所属工单ID")]
        public int MesOrderId { get; set; }

        /// <summary>
        /// 组件料号
        /// </summary>
        [SugarColumn(ColumnDescription = "组件料号")]
        public string ComponentCode { get; set; }

        /// <summary>
        /// 组件名称
        /// </summary>
        [SugarColumn(ColumnDescription = "组件名称", IsNullable = true)]
        public string ComponentName { get; set; }

        /// <summary>
        /// 组件数量
        /// </summary>
        [SugarColumn(ColumnDescription = "组件数量", IsNullable = true)]
        public double Qty { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnDescription = "创建时间")]
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
