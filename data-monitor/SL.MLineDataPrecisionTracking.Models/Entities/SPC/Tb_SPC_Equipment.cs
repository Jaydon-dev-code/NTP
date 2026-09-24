using System;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities.SPC
{
    /// <summary>
    /// SPC 设备模型
    /// </summary>
    [SugarTable("SPC_Equipment")]
    public class Tb_SPC_Equipment
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>设备名称</summary>
        [SugarColumn(ColumnDescription = "设备名称")]
        public string Name { get; set; }

        /// <summary>所在产线</summary>
        [SugarColumn(ColumnDescription = "所在产线", IsNullable = true)]
        public string ProductionLine { get; set; }

        /// <summary>工序编号</summary>
        [SugarColumn(ColumnDescription = "工序编号", IsNullable = true)]
        public string ProcessNo { get; set; }

        /// <summary>工序名称</summary>
        [SugarColumn(ColumnDescription = "工序名称", IsNullable = true)]
        public string ProcessName { get; set; }

        /// <summary>备注</summary>
        [SugarColumn(ColumnDescription = "备注", IsNullable = true)]
        public string Remark { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
