using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    /// <summary>
    /// 服务信息
    /// </summary>
    public class ServiceInfo
    {
        /// <summary>
        /// 服务ID
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 服务类型
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>
        /// 是否运行中
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// 最后操作人
        /// </summary>
        public string LastOperator { get; set; }

        /// <summary>
        /// 操作备注
        /// </summary>
        public string Remark { get; set; }
    }
}
