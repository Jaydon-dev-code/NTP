using SqlSugar;
using System;
using System.Collections.Generic;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    /// <summary>
    /// 产线（一条产线归属一个厂，包含多台设备）
    /// </summary>
    [SugarTable("ProductionLine")]
    public class Tb_ProductionLine
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>所属厂</summary>
        public int FactoryId { get; set; }

        /// <summary>产线编码（标识字段）</summary>
        [SugarColumn(ColumnDescription = "产线编码", IsNullable = false)]
        public string LineCode { get; set; }

        /// <summary>产线名称</summary>
        [SugarColumn(ColumnDescription = "产线名称", IsNullable = false)]
        public string LineName { get; set; }

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

        // 导航：一个产线 → 一台厂
        [Navigate(NavigateType.ManyToOne, nameof(FactoryId))]
        public Tb_Factory Factory { get; set; }

        // 导航：一个产线 → 多台设备
        [Navigate(NavigateType.OneToMany, nameof(Tb_Equipment.LineId))]
        public List<Tb_Equipment> Equipments { get; set; }
    }
}
