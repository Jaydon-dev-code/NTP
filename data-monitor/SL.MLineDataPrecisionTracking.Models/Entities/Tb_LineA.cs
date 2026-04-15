using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public string TrayNo { get; set; }

        /// <summary>
        /// 屏蔽工位
        /// </summary>
        public string ShieldStation { get; set; }

        /// <summary>
        /// NG代码
        /// </summary>
        public string NgCode { get; set; }

        /// <summary>
        /// 小内圈分选数据
        /// </summary>
        public string SmallInnerRingSortingData { get; set; }

        /// <summary>
        /// 外法兰分选数据
        /// </summary>
        public string OuterFlangeSortingData { get; set; }

        /// <summary>
        /// 内法兰分选数据
        /// </summary>
        public string InnerFlangeSortingData { get; set; }

        /// <summary>
        /// A面钢球组差
        /// </summary>
        public decimal? ASideSteelBallGroupDiff { get; set; }

        /// <summary>
        /// B面钢球组差
        /// </summary>
        public decimal? BSideSteelBallGroupDiff { get; set; }

        /// <summary>
        /// B面钢球注脂量
        /// </summary>
        public decimal? BSideSteelBallGreaseVolume { get; set; }

        /// <summary>
        /// 密封圈注脂量
        /// </summary>
        public decimal? SealRingGreaseVolume { get; set; }

        /// <summary>
        /// A面钢球注脂量
        /// </summary>
        public decimal? ASideSteelBallGreaseVolume { get; set; }

        /// <summary>
        /// 密封圈压装力
        /// </summary>
        public decimal? SealRingPressForce { get; set; }

        /// <summary>
        /// 密封圈压装位移
        /// </summary>
        public decimal? SealRingPressDisplacement { get; set; }

        /// <summary>
        /// 挡水环压装力
        /// </summary>
        public decimal? WaterBafflePressForce { get; set; }

        /// <summary>
        /// 挡水环压装位移
        /// </summary>
        public decimal? WaterBafflePressDisplacement { get; set; }

        /// <summary>
        /// 小内圈合套压力
        /// </summary>
        public decimal? SmallInnerRingAssemblePressure { get; set; }

        /// <summary>
        /// 小内圈合套位移
        /// </summary>
        public decimal? SmallInnerRingAssembleDisplacement { get; set; }

        /// <summary>
        /// 密封圈平行差
        /// </summary>
        public decimal? SealRingParallelDiff { get; set; }

        /// <summary>
        /// 挡水环平行差
        /// </summary>
        public decimal? WaterBaffleParallelDiff { get; set; }

    }
}
