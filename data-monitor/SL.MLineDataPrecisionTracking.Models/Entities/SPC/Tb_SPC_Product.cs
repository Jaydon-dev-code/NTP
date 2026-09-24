using System;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities.SPC
{
    /// <summary>
    /// SPC 产品模型
    /// </summary>
    [SugarTable("SPC_Product")]
    public class Tb_SPC_Product
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>产品型号</summary>
        [SugarColumn(ColumnDescription = "产品型号")]
        public string ModelNo { get; set; }

        /// <summary>产品名称</summary>
        [SugarColumn(ColumnDescription = "产品名称")]
        public string Name { get; set; }

        /// <summary>备注</summary>
        [SugarColumn(ColumnDescription = "备注", IsNullable = true)]
        public string Remark { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
