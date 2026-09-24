using System;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities.SPC
{
    /// <summary>
    /// SPC PLC 点位 — 数据来源=PLC 时，记录点位信息。
    /// PlcPointId > 0：引用 Tb_PlcPoint（已有点位）；
    /// PlcPointId = 0：手动录入（使用下方内联字段）。
    /// </summary>
    [SugarTable("SPC_PlcPoint")]
    public class Tb_SPC_PlcPoint
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>关联特性ID</summary>
        [SugarColumn(ColumnDescription = "关联特性ID")]
        public int CharacteristicId { get; set; }

        /// <summary>引用 Tb_PlcPoint.Id；0 = 手动录入（用内联字段）</summary>
        [SugarColumn(ColumnDescription = "PLC点位ID")]
        public int PlcPointId { get; set; }

        // ── 手动录入内联字段（PlcPointId=0 时生效） ──

        [SugarColumn(ColumnDescription = "PLC IP", IsNullable = true)]
        public string IpAddress { get; set; }

        [SugarColumn(ColumnDescription = "端口", IsNullable = true)]
        public int? Port { get; set; }

        [SugarColumn(ColumnDescription = "PLC类型", IsNullable = true)]
        public string PlcType { get; set; }

        [SugarColumn(ColumnDescription = "变量名", IsNullable = true)]
        public string PointName { get; set; }

        [SugarColumn(ColumnDescription = "区块", IsNullable = true)]
        public string Area { get; set; }

        [SugarColumn(ColumnDescription = "地址", IsNullable = true)]
        public string Address { get; set; }

        [SugarColumn(ColumnDescription = "数据类型", IsNullable = true)]
        public string DataType { get; set; }

        [SugarColumn(ColumnDescription = "长度", IsNullable = true)]
        public int? Length { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>导航：关联特性</summary>
        [Navigate(NavigateType.ManyToOne, nameof(CharacteristicId))]
        public Tb_SPC_Characteristic Characteristic { get; set; }
    }
}
