using System;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities.SPC
{
    /// <summary>
    /// SPC 数据库采集源 — 数据来源=数据库时，记录关联信息和上次采集时间。
    /// </summary>
    [SugarTable("SPC_CollectionSource")]
    public class Tb_SPC_CollectionSource
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>关联特性ID</summary>
        [SugarColumn(ColumnDescription = "关联特性ID")]
        public int CharacteristicId { get; set; }

        /// <summary>数据库连接字符串</summary>
        [SugarColumn(ColumnDescription = "连接字符串", IsNullable = true)]
        public string ConnectionString { get; set; }

        /// <summary>表名</summary>
        [SugarColumn(ColumnDescription = "表名", IsNullable = true)]
        public string TableName { get; set; }

        /// <summary>测量值字段名</summary>
        [SugarColumn(ColumnDescription = "测量值字段名", IsNullable = true)]
        public string MeasureValueField { get; set; }

        /// <summary>测量结果字段名</summary>
        [SugarColumn(ColumnDescription = "测量结果字段名", IsNullable = true)]
        public string MeasureResultField { get; set; }

        /// <summary>时间字段名（WHERE 条件）</summary>
        [SugarColumn(ColumnDescription = "时间字段名", IsNullable = true)]
        public string TimeFieldName { get; set; }

        /// <summary>上次采集时间</summary>
        [SugarColumn(ColumnDescription = "上次采集时间", IsNullable = true)]
        public DateTime? LastCollectionTime { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>导航：关联特性</summary>
        [Navigate(NavigateType.ManyToOne, nameof(CharacteristicId))]
        public Tb_SPC_Characteristic Characteristic { get; set; }
    }
}
