using System;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities.SPC
{
    /// <summary>
    /// SPC 零件模型
    /// </summary>
    [SugarTable("SPC_Part")]
    public class Tb_SPC_Part
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>零件型号</summary>
        [SugarColumn(ColumnDescription = "零件型号")]
        public string ModelNo { get; set; }

        /// <summary>零件名称</summary>
        [SugarColumn(ColumnDescription = "零件名称")]
        public string Name { get; set; }

        /// <summary>关联产品ID</summary>
        [SugarColumn(ColumnDescription = "关联产品ID", IsNullable = true)]
        public int? ProductId { get; set; }

        /// <summary>零件类别</summary>
        [SugarColumn(ColumnDescription = "零件类别", IsNullable = true)]
        public string PartCategory { get; set; }

        /// <summary>备注</summary>
        [SugarColumn(ColumnDescription = "备注", IsNullable = true)]
        public string Remark { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>导航：关联产品</summary>
        [Navigate(NavigateType.ManyToOne, nameof(ProductId))]
        public Tb_SPC_Product Product { get; set; }
    }
}
