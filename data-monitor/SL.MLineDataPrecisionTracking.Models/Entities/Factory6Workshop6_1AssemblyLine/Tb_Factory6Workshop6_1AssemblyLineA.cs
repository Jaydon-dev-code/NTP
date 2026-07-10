using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_1AssemblyLine
{
    [SugarTable("Factory6Workshop6_1AssemblyLineA")]
    public class Tb_Factory6Workshop6_1AssemblyLineA
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public DateTime RecordTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 托盘号
        /// </summary>
        [SugarColumn(ColumnDescription = "托盘号A")]
        public string TrayNoA { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        [SugarColumn(ColumnDescription = "型号A")]
        public string ModelNoA { get; set; }

        /// <summary>
        /// 屏蔽工位
        /// </summary>
        [SugarColumn(ColumnDescription = "屏蔽工位A", ColumnDataType = "varchar(500)")]
        public string ShieldStationA { get; set; }

        /// <summary>
        /// NG代码
        /// </summary>
        [SugarColumn(ColumnDescription = "NG代码A")]
        public string NgCodeA { get; set; }

        /// <summary>
        /// A面钢球组差
        /// </summary>
        [SugarColumn(ColumnDescription = "A面钢球组差")]
        public string ASideSteelBallGroupDiff { get; set; }

        /// <summary>
        /// B面钢球组差
        /// </summary>
        [SugarColumn(ColumnDescription = "B面钢球组差")]
        public string BSideSteelBallGroupDiff { get; set; }

        /// <summary>
        /// A面钢球注脂量
        /// </summary>
        [SugarColumn(ColumnDescription = "A面钢球注脂量")]
        public string ASideSteelBallGreaseVolume { get; set; }

        /// <summary>
        /// 密封圈平行差
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈平行差")]
        public string SealRingParallelDiff { get; set; }

        /// <summary>
        /// B面钢球注脂量
        /// </summary>
        [SugarColumn(ColumnDescription = "B面钢球注脂量")]
        public string BSideSteelBallGreaseVolume { get; set; }

        /// <summary>
        /// 外法兰半成品码
        /// </summary>
        [SugarColumn(ColumnDescription = "外法兰半成品码")]
        public string OuterFlangeSemiFinishedCode { get; set; }

        /// <summary>
        /// 内法兰半成品码
        /// </summary>
        [SugarColumn(ColumnDescription = "内法兰半成品码")]
        public string InnerFlangeSemiFinishedCode { get; set; }
    }
}
