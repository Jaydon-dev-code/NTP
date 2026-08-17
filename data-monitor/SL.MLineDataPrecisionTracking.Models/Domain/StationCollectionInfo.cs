using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    /// <summary>
    /// 对外返回的工位信息 （前端展示用）。
    /// 字段来自：工位对象本身 + 设备所属厂/产线（连表查询）+ 持久化记录。
    /// </summary>
    public class StationCollectionInfo
    {
        public string EquipmentId { get; set; } // 设备编号（string，工位Id）
        public string DeviceName { get; set; } // 设备名
        public int? FactoryId { get; set; } // 所属厂Id
        public string FactoryCode { get; set; } // 厂标识
        public string FactoryName { get; set; } // 厂名
        public int? LineId { get; set; } // 所属产线Id
        public string LineCode { get; set; } // 产线标识
        public string LineName { get; set; } // 产线名
        public string Topic { get; set; } // MQTT 发布 Topic
        public bool IsRunning { get; set; } // 是否在采集队列中运行
        public bool IsEnabled { get; set; } // 是否启用（持久化）
        public string Status { get; set; } // "Running" / "Stopped"
        public string Description { get; set; } // 工位最新执行描述
        public Dictionary<string, object> LatestSnapshot { get; set; } // 最新点位快照
    }
}
