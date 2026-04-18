using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    [SugarTable("LineSummary")]
    public class Tb_LineSummary
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public DateTime RecordTime { get; set; } = DateTime.Now;

        [SugarColumn(ColumnDescription = "型号名称")]
        public string ModelName { get; set; }

        /// <summary>
        /// 托盘号
        /// </summary>
        [SugarColumn(ColumnDescription = "托盘号A")]
        public int TrayNoA { get; set; }

        /// <summary>
        /// 托盘号
        /// </summary>
        [SugarColumn(ColumnDescription = "托盘号B")]
        public int TrayNoB { get; set; }

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
        public int PositiveClearanceValue { get; set; }

        /// <summary>
        /// 位移量
        /// </summary>
        [SugarColumn(ColumnDescription = "位移量")]
        public int DisplacementValue { get; set; }

        /// <summary>
        /// 负游隙检测值
        /// </summary>
        [SugarColumn(ColumnDescription = "负游隙检测值")]
        public int NegativeClearanceValue { get; set; }

        /// <summary>
        /// 铆接前成型高度
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接前成型高度")]
        public int PreRivetingHeight { get; set; }

        /// <summary>
        /// 铆接成型力
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接成型力")]
        public int RivetingForce { get; set; }

        /// <summary>
        /// 铆接成型位移
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接成型位移")]
        public int RivetingDisplacement { get; set; }

        /// <summary>
        /// 铆接后成型高度
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接后成型高度")]
        public int PostRivetingHeight { get; set; }

        /// <summary>
        /// 铆接后成型形状
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接后成型形状")]
        public int PostRivetingShape { get; set; }

        /// <summary>
        /// 铆接后成型外径
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接后成型外径")]
        public int PostRivetingOuterDiameter { get; set; }

        /// <summary>
        /// 二维码打标内容
        /// </summary>
        [SugarColumn(ColumnDescription = "二维码打标内容")]
        public string MarkingNo { get; set; }

        /// <summary>
        /// 径跳值
        /// </summary>
        [SugarColumn(ColumnDescription = "径跳值")]
        public int RadialRunout { get; set; }

        /// <summary>
        /// 端跳值
        /// </summary>
        [SugarColumn(ColumnDescription = "端跳值")]
        public int EndRunout { get; set; }

        /// <summary>
        /// 扭矩检测值
        /// </summary>
        [SugarColumn(ColumnDescription = "扭矩检测值")]
        public int TorqueValue { get; set; }

        /// <summary>
        /// ABS检测峰值
        /// </summary>
        [SugarColumn(ColumnDescription = "ABS检测峰值")]
        public int AbsPeakValue { get; set; }

        /// <summary>
        /// ABS检测谷值
        /// </summary>
        [SugarColumn(ColumnDescription = "ABS检测谷值")]
        public int AbsValleyValue { get; set; }

        /// <summary>
        /// ABS检测齿数
        /// </summary>
        [SugarColumn(ColumnDescription = "ABS检测齿数")]
        public int? AbsToothCount { get; set; }

        /// <summary>
        /// 密封圈压装压力
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装压力")]
        public int SealRingPressPressure { get; set; }

        /// <summary>
        /// 密封圈压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装位移B")]
        public int SealRingPressDisplacementB { get; set; }

        /// <summary>
        /// 齿圈压装压力
        /// </summary>
        [SugarColumn(ColumnDescription = "齿圈压装压力")]
        public int GearRingPressPressure { get; set; }

        /// <summary>
        /// 齿圈压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "齿圈压装位移")]
        public int GearRingPressDisplacement { get; set; }

        /// <summary>
        /// 防尘盖压装压力
        /// </summary>
        [SugarColumn(ColumnDescription = "防尘盖压装压力")]
        public int DustCoverPressPressure { get; set; }

        /// <summary>
        /// 防尘盖压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "防尘盖压装位移")]
        public int DustCoverPressDisplacement { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器1
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差检测传感器1")]
        public int MagneticRingParallelSensor1 { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器2
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差检测传感器2")]
        public int MagneticRingParallelSensor2 { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器3
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差检测传感器3")]
        public int MagneticRingParallelSensor3 { get; set; }

        /// <summary>
        /// 磁性圈平行差
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差")]
        public int MagneticRingParallelDiff { get; set; }

        /// <summary>
        /// 振动下LOAD
        /// </summary>
        [SugarColumn(ColumnDescription = "振动下LOAD")]
        public int VibrationLowerLOAD { get; set; }

        /// <summary>
        /// 振动下LH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动下LH")]
        public int VibrationLowerLH { get; set; }

        /// <summary>
        /// 振动下RH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动下RH")]
        public int VibrationLowerRH { get; set; }

        /// <summary>
        /// 振动上LOAD
        /// </summary>
        [SugarColumn(ColumnDescription = "振动上LOAD")]
        public int VibrationUpperLOAD { get; set; }

        /// <summary>
        /// 振动上LH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动上LH")]
        public int VibrationUpperLH { get; set; }

        /// <summary>
        /// 振动上RH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动上RH")]
        public int VibrationUpperRH { get; set; }
    }
}
