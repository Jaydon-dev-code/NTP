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

        /// <summary>设备编号（业务标识，string）</summary>
        [SugarColumn(ColumnDescription = "设备编号", IsNullable = false)]
        public string EquipmentId { get; set; }

        /// <summary>设备名称</summary>
        public string DeviceName { get; set; }

        /// <summary>归属产线（可为空：设备独立存在，可后加入产线）</summary>
        [SugarColumn(IsNullable = true)]
        public int? LineId { get; set; }

        /// <summary>备注</summary>
        public string Remark { get; set; } = "";

        public DateTime CreateTime { get; set; } = DateTime.Now;

        // 导航：一个设备 → 一个产线
        [Navigate(NavigateType.ManyToOne, nameof(LineId))]
        public Tb_ProductionLine ProductionLine { get; set; }

        // 导航：一个设备 → 多个PLC连接
        [Navigate(NavigateType.OneToMany, nameof(Tb_PlcConnection.EquipmentId))]
   
        public List<Tb_PlcConnection> PlcConnections { get; set; }
    }
}
