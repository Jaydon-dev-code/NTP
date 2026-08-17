using SL.MLineDataPrecisionTracking.Models.Domain.Mqtt;

namespace SL.MLineDataPrecisionTracking.Models.Dtos
{
    /// <summary>
    /// 注册设备采集参数
    /// </summary>
    public class DeviceRegisterDto
    {
        public string EquipmentId { get; set; }
        public MqttPublishConfig MqttConfig { get; set; }
    }

    /// <summary>
    /// 启用/禁用参数
    /// </summary>
    public class DeviceEnabledDto
    {
        public string EquipmentId { get; set; }
        public bool IsEnabled { get; set; }
    }

    /// <summary>
    /// 心跳参数
    /// </summary>
    public class HeartbeatDto
    {
        public int? IntervalSeconds { get; set; }
    }
}
