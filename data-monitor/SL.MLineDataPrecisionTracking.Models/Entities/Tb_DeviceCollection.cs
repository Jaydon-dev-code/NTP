using SqlSugar;
using System;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    /// <summary>
    /// 设备采集注册表 — 记录已注册的设备采集服务及其运行状态，重启后据此恢复注册并自动运行。
    /// </summary>
    [SugarTable("DeviceCollection")]
    public class Tb_DeviceCollection
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>设备编号（string，来自 Tb_Equipment.EquipmentId，唯一）</summary>
        [SugarColumn(ColumnDescription = "设备编号", IsNullable = false)]
        public string EquipmentId { get; set; }

        /// <summary>设备名称（冗余显示）</summary>
        public string DeviceName { get; set; }

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

        /// <summary>MQTT发布Topic（解析后的完整主题）</summary>
        [SugarColumn(IsNullable = true)]
        public string Topic { get; set; }

        /// <summary>是否启用</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>上次运行状态（重启后据此自动启动）</summary>
        public bool IsRunning { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
