using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    [SugarTable("LineSummary")]
    public class Tb_LineSummary
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        public DateTime RecordTime { get; set; } = DateTime.Now;
        public string ModelName { get; set; }

        /// <summary>
        /// 托盘号
        /// </summary>
        [SugarColumn(ColumnDescription = "托盘号")]
        public int TrayNo { get; set; }

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
        public float SmallInnerRingSortingData { get; set; }

        /// <summary>
        /// 外法兰分选数据
        /// </summary>
        [SugarColumn(ColumnDescription = "外法兰分选数据")]
        public float OuterFlangeSortingData { get; set; }

        /// <summary>
        /// 内法兰分选数据
        /// </summary>
        [SugarColumn(ColumnDescription = "内法兰分选数据")]
        public float InnerFlangeSortingData { get; set; }

        /// <summary>
        /// A面钢球组差
        /// </summary>
        [SugarColumn(ColumnDescription = "A面钢球组差")]
        public float ASideSteelBallGroupDiff { get; set; }

        /// <summary>
        /// B面钢球组差
        /// </summary>
        [SugarColumn(ColumnDescription = "B面钢球组差")]
        public float BSideSteelBallGroupDiff { get; set; }

        /// <summary>
        /// B面钢球注脂量
        /// </summary>
        [SugarColumn(ColumnDescription = "B面钢球注脂量")]
        public float BSideSteelBallGreaseVolume { get; set; }

        /// <summary>
        /// 密封圈注脂量
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈注脂量")]
        public float SealRingGreaseVolume { get; set; }

        /// <summary>
        /// A面钢球注脂量
        /// </summary>
        [SugarColumn(ColumnDescription = "A面钢球注脂量")]
        public float ASideSteelBallGreaseVolume { get; set; }

        /// <summary>
        /// 密封圈压装力
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装力")]
        public float SealRingPressForce { get; set; }

        /// <summary>
        /// 密封圈压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装位移A")]
        public float SealRingPressDisplacementA { get; set; }

        /// <summary>
        /// 挡水环压装力
        /// </summary>
        [SugarColumn(ColumnDescription = "挡水环压装力")]
        public float WaterBafflePressForce { get; set; }

        /// <summary>
        /// 挡水环压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "挡水环压装位移")]
        public float WaterBafflePressDisplacement { get; set; }

        /// <summary>
        /// 小内圈合套压力
        /// </summary>
        [SugarColumn(ColumnDescription = "小内圈合套压力")]
        public float SmallInnerRingAssemblePressure { get; set; }

        /// <summary>
        /// 小内圈合套位移
        /// </summary>
        [SugarColumn(ColumnDescription = "小内圈合套位移")]
        public float SmallInnerRingAssembleDisplacement { get; set; }

        /// <summary>
        /// 密封圈平行差
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈平行差")]
        public float SealRingParallelDiff { get; set; }

        /// <summary>
        /// 挡水环平行差
        /// </summary>
        [SugarColumn(ColumnDescription = "挡水环平行差")]
        public float WaterBaffleParallelDiff { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        [SugarColumn(ColumnDescription = "型号")]
        public int ModelNo { get; set; }

        /// <summary>
        /// 屏蔽工位
        /// </summary>
        [SugarColumn(ColumnDescription = "屏蔽工位B")]
        public int ShieldStationB { get; set; }

        /// <summary>
        /// NG代码
        /// </summary>
        [SugarColumn(ColumnDescription = "NG代码B")]
        public int NgCodeB { get; set; }

        ///// <summary>
        ///// A线托盘编号
        ///// </summary>
        //[SugarColumn(ColumnDescription = "A线托盘编号")]
        //public int LineATrayNo { get; set; }

        /// <summary>
        /// 识别代码
        /// </summary>
        [SugarColumn(ColumnDescription = "识别代码")]
        public int IdentificationCode { get; set; }

        /// <summary>
        /// 正游隙检测值
        /// </summary>
        [SugarColumn(ColumnDescription = "正游隙检测值")]
        public float PositiveClearanceValue { get; set; }

        /// <summary>
        /// 位移量
        /// </summary>
        [SugarColumn(ColumnDescription = "位移量")]
        public float DisplacementValue { get; set; }

        /// <summary>
        /// 负游隙检测值
        /// </summary>
        [SugarColumn(ColumnDescription = "负游隙检测值")]
        public float NegativeClearanceValue { get; set; }

        /// <summary>
        /// 铆接前成型高度
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接前成型高度")]
        public decimal PreRivetingHeight { get; set; }

        /// <summary>
        /// 铆接成型力
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接成型力")]
        public decimal RivetingForce { get; set; }

        /// <summary>
        /// 铆接成型位移
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接成型位移")]
        public decimal RivetingDisplacement { get; set; }

        /// <summary>
        /// 铆接后成型高度
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接后成型高度")]
        public decimal PostRivetingHeight { get; set; }

        /// <summary>
        /// 铆接后成型形状
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接后成型形状")]
        public string PostRivetingShape { get; set; }

        /// <summary>
        /// 铆接后成型外径
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接后成型外径")]
        public decimal PostRivetingOuterDiameter { get; set; }

        /// <summary>
        /// 二维码打标内容
        /// </summary>
        [SugarColumn(ColumnDescription = "二维码打标内容")]
        public string QrCodeContent { get; set; }

        /// <summary>
        /// 径跳值
        /// </summary>
        [SugarColumn(ColumnDescription = "径跳值")]
        public decimal RadialRunout { get; set; }

        /// <summary>
        /// 端跳值
        /// </summary>
        [SugarColumn(ColumnDescription = "端跳值")]
        public decimal EndRunout { get; set; }

        /// <summary>
        /// 扭矩检测值
        /// </summary>
        [SugarColumn(ColumnDescription = "扭矩检测值")]
        public decimal TorqueValue { get; set; }

        /// <summary>
        /// ABS检测峰值
        /// </summary>
        [SugarColumn(ColumnDescription = "ABS检测峰值")]
        public decimal AbsPeakValue { get; set; }

        /// <summary>
        /// ABS检测谷值
        /// </summary>
        [SugarColumn(ColumnDescription = "ABS检测谷值")]
        public decimal AbsValleyValue { get; set; }

        /// <summary>
        /// ABS检测齿数
        /// </summary>
        [SugarColumn(ColumnDescription = "ABS检测齿数")]
        public int? AbsToothCount { get; set; }

        /// <summary>
        /// 密封圈压装压力
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装压力")]
        public decimal SealRingPressPressure { get; set; }

        /// <summary>
        /// 密封圈压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装位移B")]
        public decimal SealRingPressDisplacementB { get; set; }

        /// <summary>
        /// 齿圈压装压力
        /// </summary>
        [SugarColumn(ColumnDescription = "齿圈压装压力")]
        public decimal GearRingPressPressure { get; set; }

        /// <summary>
        /// 齿圈压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "齿圈压装位移")]
        public decimal GearRingPressDisplacement { get; set; }

        /// <summary>
        /// 防尘盖压装压力
        /// </summary>
        [SugarColumn(ColumnDescription = "防尘盖压装压力")]
        public decimal DustCoverPressPressure { get; set; }

        /// <summary>
        /// 防尘盖压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "防尘盖压装位移")]
        public decimal DustCoverPressDisplacement { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器1
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差检测传感器1")]
        public decimal MagneticRingParallelSensor1 { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器2
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差检测传感器2")]
        public decimal MagneticRingParallelSensor2 { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器3
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差检测传感器3")]
        public decimal MagneticRingParallelSensor3 { get; set; }

        /// <summary>
        /// 磁性圈平行差
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差")]
        public decimal MagneticRingParallelDiff { get; set; }

        /// <summary>
        /// 振动下LOAD
        /// </summary>
        [SugarColumn(ColumnDescription = "振动下LOAD")]
        public decimal VibrationLowerLOAD { get; set; }

        /// <summary>
        /// 振动下LH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动下LH")]
        public decimal VibrationLowerLH { get; set; }

        /// <summary>
        /// 振动下RH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动下RH")]
        public decimal VibrationLowerRH { get; set; }

        /// <summary>
        /// 振动上LOAD
        /// </summary>
        [SugarColumn(ColumnDescription = "振动上LOAD")]
        public decimal VibrationUpperLOAD { get; set; }

        /// <summary>
        /// 振动上LH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动上LH")]
        public decimal VibrationUpperLH { get; set; }

        /// <summary>
        /// 振动上RH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动上RH")]
        public decimal VibrationUpperRH { get; set; }
    }
}
