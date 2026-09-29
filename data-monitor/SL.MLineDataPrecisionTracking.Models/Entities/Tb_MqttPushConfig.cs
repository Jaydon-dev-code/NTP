using System;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    /// <summary>
    /// 产线数据 MQTT 推送配置表（按采集服务维度）
    /// </summary>
    [SugarTable("MqttPushConfig")]
    public class Tb_MqttPushConfig
    {
        /// <summary>
        /// 配置键（采集服务类名，如 Factory6Workshop6_3AssemblyLineB）
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 100, ColumnDescription = "配置键")]
        public string ConfigKey { get; set; }

        /// <summary>
        /// 是否开启推送
        /// </summary>
        [SugarColumn(ColumnDescription = "是否开启推送")]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// MQTT 服务器地址
        /// </summary>
        [SugarColumn(Length = 200, ColumnDescription = "MQTT服务器地址")]
        public string Host { get; set; }

        /// <summary>
        /// MQTT 服务器端口
        /// </summary>
        [SugarColumn(ColumnDescription = "MQTT服务器端口")]
        public int Port { get; set; } = 1883;

        /// <summary>
        /// 用户名
        /// </summary>
        [SugarColumn(Length = 100, ColumnDescription = "用户名")]
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [SugarColumn(Length = 200, ColumnDescription = "密码")]
        public string Password { get; set; }

        /// <summary>
        /// 发布 Topic（为空时使用配置键）
        /// </summary>
        [SugarColumn(Length = 300, ColumnDescription = "发布Topic")]
        public string Topic { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [SugarColumn(ColumnDescription = "最后更新时间")]
        public DateTime UpdateTime { get; set; }
    }
}
