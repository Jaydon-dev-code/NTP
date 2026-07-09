using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    [SugarTable("LineA")]
    public class Tb_LineA
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public DateTime RecordTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 型号
        /// </summary>
        [SugarColumn(ColumnDescription = "型号A")]
        public string ModelNoA { get; set; }

        /// <summary>
        /// 托盘号
        /// </summary>
        [SugarColumn(ColumnDescription = "托盘号A")]
        public string TrayNoA { get; set; }

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
        /// 小内圈分选数据
        /// </summary>
        [SugarColumn(ColumnDescription = "小内圈分选数据")]
        public string SmallInnerRingSortingData { get; set; }

        /// <summary>
        /// 外法兰分选数据
        /// </summary>
        [SugarColumn(ColumnDescription = "外法兰分选数据")]
        public string OuterFlangeSortingData { get; set; }

        /// <summary>
        /// 内法兰分选数据
        /// </summary>
        [SugarColumn(ColumnDescription = "内法兰分选数据")]
        public string InnerFlangeSortingData { get; set; }

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
        /// B面钢球注脂量
        /// </summary>
        [SugarColumn(ColumnDescription = "B面钢球注脂量")]
        public string BSideSteelBallGreaseVolume { get; set; }

        /// <summary>
        /// 密封圈注脂量
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈注脂量")]
        public string SealRingGreaseVolume { get; set; }

        /// <summary>
        /// A面钢球注脂量
        /// </summary>
        [SugarColumn(ColumnDescription = "A面钢球注脂量")]
        public string ASideSteelBallGreaseVolume { get; set; }

        /// <summary>
        /// 密封圈压装力
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装力")]
        public string SealRingPressForce { get; set; }

        /// <summary>
        /// 密封圈压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装位移A")]
        public string SealRingPressDisplacementA { get; set; }

        /// <summary>
        /// 挡水环压装力
        /// </summary>
        [SugarColumn(ColumnDescription = "挡水环压装力")]
        public string WaterBafflePressForce { get; set; }

        /// <summary>
        /// 挡水环压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "挡水环压装位移")]
        public string WaterBafflePressDisplacement { get; set; }

        /// <summary>
        /// 小内圈合套压力
        /// </summary>
        [SugarColumn(ColumnDescription = "小内圈合套压力")]
        public string SmallInnerRingAssemblePressure { get; set; }

        /// <summary>
        /// 小内圈合套位移
        /// </summary>
        [SugarColumn(ColumnDescription = "小内圈合套位移")]
        public string SmallInnerRingAssembleDisplacement { get; set; }

        /// <summary>
        /// 密封圈平行差
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈平行差")]
        public string SealRingParallelDiff { get; set; }

        /// <summary>
        /// 挡水环平行差
        /// </summary>
        [SugarColumn(ColumnDescription = "挡水环平行差")]
        public string WaterBaffleParallelDiff { get; set; }

        /// <summary>
        /// 外法兰半成品码
        /// </summary>
        [SugarColumn(ColumnDescription = "外法兰半成品码", DefaultValue = " ")]
        public string OuterFlangeSemiFinishedCode { get; set; }

        /// <summary>
        /// 内法兰半成品码
        /// </summary>
        [SugarColumn(ColumnDescription = "内法兰半成品码", DefaultValue = " ")]
        public string InnerFlangeSemiFinishedCode { get; set; }

        /// <summary>
        /// 螺栓压装1
        /// </summary>
        [SugarColumn(ColumnDescription = "螺栓压装1", DefaultValue = "0")]
        public string BoltPressAssemble1 { get; set; }

        /// <summary>
        /// 螺栓压装2
        /// </summary>
        [SugarColumn(ColumnDescription = "螺栓压装2", DefaultValue = "0")]
        public string BoltPressAssemble2 { get; set; }

        /// <summary>
        /// 螺栓压装3
        /// </summary>
        [SugarColumn(ColumnDescription = "螺栓压装3", DefaultValue = "0")]
        public string BoltPressAssemble3 { get; set; }

        /// <summary>
        /// 螺栓压装4
        /// </summary>
        [SugarColumn(ColumnDescription = "螺栓压装4", DefaultValue = "0")]
        public string BoltPressAssemble4 { get; set; }

        /// <summary>
        /// 螺栓压装5
        /// </summary>
        [SugarColumn(ColumnDescription = "螺栓压装5", DefaultValue = "0")]
        public string BoltPressAssemble5 { get; set; }

        /// <summary>
        /// 螺栓压装AVG
        /// </summary>
        [SugarColumn(ColumnDescription = "螺栓压装AVG", DefaultValue = "0")]
        public string BoltPressAssembleAVG { get; set; }
    }
}
