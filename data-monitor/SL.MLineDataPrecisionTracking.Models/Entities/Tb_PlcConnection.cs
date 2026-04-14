using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    /// <summary>
    /// PLC连接信息（一个设备多个IP/端口）
    /// </summary>
    [SugarTable("PlcConnection")]
    public class Tb_PlcConnection
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public int EquipmentId { get; set; }

        /// <summary>PLC IP</summary>
        public string IpAddress { get; set; }

        /// <summary>端口</summary>
        public int Port { get; set; } = 8000;

        /// <summary>PLC类型</summary>
        public string PlcType { get; set; } = "三菱";

        /// <summary>通讯协议</summary>
        public string NetworkType { get; set; } = "MC";

        public string Remark { get; set; } = "";
        public DateTime CreateTime { get; set; } = DateTime.Now;

        // 导航
        [SugarColumn(IsIgnore = true)]
        public List<Tb_PlcPoint> Points { get; set; }
    }
}
