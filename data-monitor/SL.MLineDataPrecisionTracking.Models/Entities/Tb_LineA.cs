using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    [SugarTable("LineA")]
    public class Tb_LineA
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        /// <summary>
        /// 托盘号
        /// </summary>
        [SugarColumn( ColumnDescription = "托盘号")]
        public int TrayNo { get; set; }

        /// <summary>
        /// 屏蔽工位
        /// </summary>
        [SugarColumn(ColumnDescription = "屏蔽工位")]
        public int ShieldStation { get; set; }

        /// <summary>
        /// NG代码
        /// </summary>
        [SugarColumn(ColumnDescription = "NG代码")]
        public int NgCode { get; set; }

        /// <summary>
        /// 小内圈分选数据
        /// </summary>
        [SugarColumn(ColumnDescription = "小内圈分选数据")]
        public float SmallInnerRingSortingData { get; set; }

        /// <summary>
        /// 外法兰分选数据
        /// </summary>
        public float OuterFlangeSortingData { get; set; }

        /// <summary>
        /// 内法兰分选数据
        /// </summary>
        public float InnerFlangeSortingData { get; set; }

        /// <summary>
        /// A面钢球组差
        /// </summary>
        public float ASideSteelBallGroupDiff { get; set; }

        /// <summary>
        /// B面钢球组差
        /// </summary>
        public float BSideSteelBallGroupDiff { get; set; }

        /// <summary>
        /// B面钢球注脂量
        /// </summary>
        public float BSideSteelBallGreaseVolume { get; set; }

        /// <summary>
        /// 密封圈注脂量
        /// </summary>
        public float SealRingGreaseVolume { get; set; }

        /// <summary>
        /// A面钢球注脂量
        /// </summary>
        public float ASideSteelBallGreaseVolume { get; set; }

        /// <summary>
        /// 密封圈压装力
        /// </summary>
        public float SealRingPressForce { get; set; }

        /// <summary>
        /// 密封圈压装位移
        /// </summary>
        public float SealRingPressDisplacement { get; set; }

        /// <summary>
        /// 挡水环压装力
        /// </summary>
        public float WaterBafflePressForce { get; set; }

        /// <summary>
        /// 挡水环压装位移
        /// </summary>
        public float WaterBafflePressDisplacement { get; set; }

        /// <summary>
        /// 小内圈合套压力
        /// </summary>
        public float SmallInnerRingAssemblePressure { get; set; }

        /// <summary>
        /// 小内圈合套位移
        /// </summary>
        public float SmallInnerRingAssembleDisplacement { get; set; }

        /// <summary>
        /// 密封圈平行差
        /// </summary>
        public float SealRingParallelDiff { get; set; }

        /// <summary>
        /// 挡水环平行差
        /// </summary>
        public float WaterBaffleParallelDiff { get; set; }

    }
}
