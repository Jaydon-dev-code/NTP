using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    using SqlSugar;

    /// <summary>
    /// 逻辑设备
    /// </summary>
    [SugarTable("Equipment")]
    public class Tb_Equipment
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>设备名称</summary>
        public string DeviceName { get; set; }

        /// <summary>备注</summary>
        public string Remark { get; set; } = "";

        public DateTime CreateTime { get; set; } = DateTime.Now;

        // 导航：一个设备 → 多个PLC连接
        [SugarColumn(IsIgnore = true)]
        public List<Tb_PlcConnection> PlcConnections { get; set; }
    }
}
