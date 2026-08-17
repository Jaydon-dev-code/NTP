using SqlSugar;
using System;
using System.Collections.Generic;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    /// <summary>
    /// 厂（一个厂包含多条产线）
    /// </summary>
    [SugarTable("Factory")]
    public class Tb_Factory
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>厂编码（标识字段）</summary>
        [SugarColumn(ColumnDescription = "厂编码", IsNullable = false)]
        public string FactoryCode { get; set; }

        /// <summary>厂名称</summary>
        [SugarColumn(ColumnDescription = "厂名称", IsNullable = false)]
        public string FactoryName { get; set; }

        /// <summary>MQTT服务器IP</summary>
        [SugarColumn(IsNullable = true)]
        public string MqttHost { get; set; }

        /// <summary>MQTT端口</summary>
        [SugarColumn(IsNullable = true)]
        public int? MqttPort { get; set; }

        /// <summary>MQTT用户名</summary>
        [SugarColumn(IsNullable = true)]
        public string MqttUsername { get; set; }

        /// <summary>MQTT密码</summary>
        [SugarColumn(IsNullable = true)]
        public string MqttPassword { get; set; }

        /// <summary>备注</summary>
        public string Remark { get; set; } = "";

        public DateTime CreateTime { get; set; } = DateTime.Now;

        // 导航：一个厂 → 多条产线
        [Navigate(NavigateType.OneToMany, nameof(Tb_ProductionLine.FactoryId))]
        public List<Tb_ProductionLine> ProductionLines { get; set; }
    }
}
