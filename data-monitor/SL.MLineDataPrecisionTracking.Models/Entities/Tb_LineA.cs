using System;
using System.Collections.Generic;
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
        /// 托盘号
        /// </summary>
        [SugarColumn(ColumnDescription = "型号A")]
        public int ModelNoA { get; set; }

        /// <summary>
        /// 托盘号
        /// </summary>
        [SugarColumn(ColumnDescription = "托盘号A")]
        public int TrayNoA { get; set; }

        /// <summary>
        /// 屏蔽工位
        /// </summary>
        [SugarColumn(ColumnDescription = "屏蔽工位A")]
        public int ShieldStationA { get; set; }

        /// <summary>
        /// NG代码
        /// </summary>
        [SugarColumn(ColumnDescription = "NG代码A")]
        public int NgCodeA { get; set; }

        /// <summary>
        /// 小内圈分选数据
        /// </summary>
        [SugarColumn(ColumnDescription = "小内圈分选数据")]
        public int SmallInnerRingSortingData { get; set; }

        /// <summary>
        /// 外法兰分选数据
        /// </summary>
        [SugarColumn(ColumnDescription = "外法兰分选数据")]
        public int OuterFlangeSortingData { get; set; }

        /// <summary>
        /// 内法兰分选数据
        /// </summary>
        [SugarColumn(ColumnDescription = "内法兰分选数据")]
        public int InnerFlangeSortingData { get; set; }

        /// <summary>
        /// A面钢球组差
        /// </summary>
        [SugarColumn(ColumnDescription = "A面钢球组差")]
        public int ASideSteelBallGroupDiff { get; set; }

        /// <summary>
        /// B面钢球组差
        /// </summary>
        [SugarColumn(ColumnDescription = "B面钢球组差")]
        public int BSideSteelBallGroupDiff { get; set; }

        /// <summary>
        /// B面钢球注脂量
        /// </summary>
        [SugarColumn(ColumnDescription = "B面钢球注脂量")]
        public int BSideSteelBallGreaseVolume { get; set; }

        /// <summary>
        /// 密封圈注脂量
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈注脂量")]
        public int SealRingGreaseVolume { get; set; }

        /// <summary>
        /// A面钢球注脂量
        /// </summary>
        [SugarColumn(ColumnDescription = "A面钢球注脂量")]
        public int ASideSteelBallGreaseVolume { get; set; }

        /// <summary>
        /// 密封圈压装力
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装力")]
        public int SealRingPressForce { get; set; }

        /// <summary>
        /// 密封圈压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装位移A")]
        public int SealRingPressDisplacementA { get; set; }

        /// <summary>
        /// 挡水环压装力
        /// </summary>
        [SugarColumn(ColumnDescription = "挡水环压装力")]
        public int WaterBafflePressForce { get; set; }

        /// <summary>
        /// 挡水环压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "挡水环压装位移")]
        public int WaterBafflePressDisplacement { get; set; }

        /// <summary>
        /// 小内圈合套压力
        /// </summary>
        [SugarColumn(ColumnDescription = "小内圈合套压力")]
        public int SmallInnerRingAssemblePressure { get; set; }

        /// <summary>
        /// 小内圈合套位移
        /// </summary>
        [SugarColumn(ColumnDescription = "小内圈合套位移")]
        public int SmallInnerRingAssembleDisplacement { get; set; }

        /// <summary>
        /// 密封圈平行差
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈平行差")]
        public int SealRingParallelDiff { get; set; }

        /// <summary>
        /// 挡水环平行差
        /// </summary>
        [SugarColumn(ColumnDescription = "挡水环平行差")]
        public int WaterBaffleParallelDiff { get; set; }
    }
}
