using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;
using SqlSugar.DbConvert;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    [SugarTable("LineSummary")]
    public class Tb_LineSummary
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public DateTime RecordTime { get; set; } = DateTime.Now;

        [SugarColumn(
            ColumnDescription = "检测结果",
            ColumnDataType = "varchar(20)",
            SqlParameterDbType = typeof(EnumToStringConvert)
        )]
        public ResultEnum Result { get; set; }

        [SugarColumn(ColumnDescription = "型号名称")]
        public string ModelName { get; set; } = "";

        /// <summary>
        /// 托盘号
        /// </summary>
        [SugarColumn(ColumnDescription = "型号")]
        public string ModelNo { get; set; } = "";

        /// <summary>
        /// 托盘号
        /// </summary>
        [SugarColumn(ColumnDescription = "托盘号A")]
        public string TrayNoA { get; set; } = "";

        /// <summary>
        /// 屏蔽工位
        /// </summary>
        [SugarColumn(ColumnDescription = "屏蔽工位A")]
        public string ShieldStationA { get; set; } = "";

        /// <summary>
        /// NG代码
        /// </summary>
        [SugarColumn(ColumnDescription = "NG代码A")]
        public string NgCodeA { get; set; } = "";

        /// <summary>
        /// 二维码打标内容
        /// </summary>
        [SugarColumn(ColumnDescription = "序列码A")]
        public string MarkingNoA { get; set; } = "";

        /// <summary>
        /// 二维码打标内容
        /// </summary>
        [SugarColumn(ColumnDescription = "序列码")]
        public string MarkingNo{ get; set; } = "";

        /// <summary>
        /// 小内圈分选数据
        /// </summary>
        [SugarColumn(ColumnDescription = "小内圈分选数据")]
        public string SmallInnerRingSortingData { get; set; } = "";

        /// <summary>
        /// 外法兰分选数据
        /// </summary>
        [SugarColumn(ColumnDescription = "外法兰分选数据")]
        public string OuterFlangeSortingData { get; set; } = "";

        /// <summary>
        /// 内法兰分选数据
        /// </summary>
        [SugarColumn(ColumnDescription = "内法兰分选数据")]
        public string InnerFlangeSortingData { get; set; } = "";

        /// <summary>
        /// A面钢球组差
        /// </summary>
        [SugarColumn(ColumnDescription = "A面钢球组差")]
        public string ASideSteelBallGroupDiff { get; set; } = "";

        /// <summary>
        /// B面钢球组差
        /// </summary>
        [SugarColumn(ColumnDescription = "B面钢球组差")]
        public string BSideSteelBallGroupDiff { get; set; } = "";

        /// <summary>
        /// B面钢球注脂量
        /// </summary>
        [SugarColumn(ColumnDescription = "B面钢球注脂量")]
        public string BSideSteelBallGreaseVolume { get; set; } = "";

        /// <summary>
        /// 密封圈注脂量
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈注脂量")]
        public string SealRingGreaseVolume { get; set; } = "";

        /// <summary>
        /// A面钢球注脂量
        /// </summary>
        [SugarColumn(ColumnDescription = "A面钢球注脂量")]
        public string ASideSteelBallGreaseVolume { get; set; } = "";

        /// <summary>
        /// 密封圈压装力
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装力")]
        public string SealRingPressForce { get; set; } = "";

        /// <summary>
        /// 密封圈压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装位移A")]
        public string SealRingPressDisplacementA { get; set; } = "";

        /// <summary>
        /// 挡水环压装力
        /// </summary>
        [SugarColumn(ColumnDescription = "挡水环压装力")]
        public string WaterBafflePressForce { get; set; } = "";

        /// <summary>
        /// 挡水环压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "挡水环压装位移")]
        public string WaterBafflePressDisplacement { get; set; } = "";

        /// <summary>
        /// 小内圈合套压力
        /// </summary>
        [SugarColumn(ColumnDescription = "小内圈合套压力")]
        public string SmallInnerRingAssemblePressure { get; set; } = "";

        /// <summary>
        /// 小内圈合套位移
        /// </summary>
        [SugarColumn(ColumnDescription = "小内圈合套位移")]
        public string SmallInnerRingAssembleDisplacement { get; set; } = "";

        /// <summary>
        /// 密封圈平行差
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈平行差")]
        public string SealRingParallelDiff { get; set; } = "";

        /// <summary>
        /// 挡水环平行差
        /// </summary>
        [SugarColumn(ColumnDescription = "挡水环平行差")]
        public string WaterBaffleParallelDiff { get; set; } = "";

        /// <summary>
        /// 托盘号
        /// </summary>
        [SugarColumn(ColumnDescription = "托盘号B")]
        public string TrayNoB { get; set; } = "";

        /// <summary>
        /// 屏蔽工位
        /// </summary>
        [SugarColumn(ColumnDescription = "屏蔽工位B")]
        public string ShieldStationB { get; set; } = "";

        /// <summary>
        /// NG代码
        /// </summary>
        [SugarColumn(ColumnDescription = "NG代码B")]
        public string NgCodeB { get; set; } = "";

        /// <summary>
        /// A线托盘编号
        /// </summary>
        [SugarColumn(ColumnDescription = "A线托盘编号")]
        public string LineATrayNo { get; set; } = "";

        /// <summary>
        /// 识别代码
        /// </summary>
        [SugarColumn(ColumnDescription = "识别代码")]
        public string IdentificationCode { get; set; } = "";

        /// <summary>
        /// 正游隙检测值
        /// </summary>
        [SugarColumn(ColumnDescription = "正游隙检测值")]
        public string PositiveClearanceValue { get; set; } = "";

        /// <summary>
        /// 位移量
        /// </summary>
        [SugarColumn(ColumnDescription = "位移量")]
        public string DisplacementValue { get; set; } = "";

        /// <summary>
        /// 负游隙检测值
        /// </summary>
        [SugarColumn(ColumnDescription = "负游隙检测值")]
        public string NegativeClearanceValue { get; set; } = "";

        /// <summary>
        /// 铆接前成型高度
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接前成型高度")]
        public string PreRivetingHeight { get; set; } = "";

        /// <summary>
        /// 铆接成型力
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接成型力")]
        public string RivetingForce { get; set; } = "";

        /// <summary>
        /// 铆接成型位移
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接成型位移")]
        public string RivetingDisplacement { get; set; } = "";

        /// <summary>
        /// 铆接后成型高度
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接后成型高度")]
        public string PostRivetingHeight { get; set; } = "";

        /// <summary>
        /// 铆接后成型形状
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接后成型形状")]
        public string PostRivetingShape { get; set; } = "";

        /// <summary>
        /// 铆接后成型外径
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接后成型外径")]
        public string PostRivetingOuterDiameter { get; set; } = "";



        /// <summary>
        /// 径跳值
        /// </summary>
        [SugarColumn(ColumnDescription = "径跳值")]
        public string RadialRunout { get; set; } = "";

        /// <summary>
        /// 端跳值
        /// </summary>
        [SugarColumn(ColumnDescription = "端跳值")]
        public string EndRunout { get; set; } = "";

        /// <summary>
        /// 扭矩检测值
        /// </summary>
        [SugarColumn(ColumnDescription = "扭矩检测值")]
        public string TorqueValue { get; set; } = "";

        /// <summary>
        /// ABS检测峰值
        /// </summary>
        [SugarColumn(ColumnDescription = "ABS检测峰值")]
        public string AbsPeakValue { get; set; } = "";

        /// <summary>
        /// ABS检测谷值
        /// </summary>
        [SugarColumn(ColumnDescription = "ABS检测谷值")]
        public string AbsValleyValue { get; set; } = "";

        /// <summary>
        /// ABS检测齿数
        /// </summary>
        [SugarColumn(ColumnDescription = "ABS检测齿数")]
        public string AbsToothCount { get; set; } = "";

        /// <summary>
        /// 密封圈压装压力
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装压力")]
        public string SealRingPressPressure { get; set; } = "";

        /// <summary>
        /// 密封圈压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装位移B")]
        public string SealRingPressDisplacementB { get; set; } = "";

        /// <summary>
        /// 齿圈压装压力
        /// </summary>
        [SugarColumn(ColumnDescription = "齿圈压装压力")]
        public string GearRingPressPressure { get; set; } = "";

        /// <summary>
        /// 齿圈压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "齿圈压装位移")]
        public string GearRingPressDisplacement { get; set; } = "";

        /// <summary>
        /// 防尘盖压装压力
        /// </summary>
        [SugarColumn(ColumnDescription = "防尘盖压装压力")]
        public string DustCoverPressPressure { get; set; } = "";

        /// <summary>
        /// 防尘盖压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "防尘盖压装位移")]
        public string DustCoverPressDisplacement { get; set; } = "";

        /// <summary>
        /// 磁性圈平行差检测传感器1
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差检测传感器1")]
        public string MagneticRingParallelSensor1 { get; set; } = "";

        /// <summary>
        /// 磁性圈平行差检测传感器2
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差检测传感器2")]
        public string MagneticRingParallelSensor2 { get; set; } = "";

        /// <summary>
        /// 磁性圈平行差检测传感器3
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差检测传感器3")]
        public string MagneticRingParallelSensor3 { get; set; } = "";

        /// <summary>
        /// 磁性圈平行差
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差")]
        public string MagneticRingParallelDiff { get; set; } = "";

        /// <summary>
        /// 振动下LOAD
        /// </summary>
        [SugarColumn(ColumnDescription = "振动下LOAD")]
        public string VibrationLowerLOAD { get; set; } = "";

        /// <summary>
        /// 振动下LH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动下LH")]
        public string VibrationLowerLH { get; set; } = "";

        /// <summary>
        /// 振动下RH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动下RH")]
        public string VibrationLowerRH { get; set; } = "";

        /// <summary>
        /// 振动上LOAD
        /// </summary>
        [SugarColumn(ColumnDescription = "振动上LOAD")]
        public string VibrationUpperLOAD { get; set; } = "";

        /// <summary>
        /// 振动上LH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动上LH")]
        public string VibrationUpperLH { get; set; } = "";

        /// <summary>
        /// 振动上RH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动上RH")]
        public string VibrationUpperRH { get; set; } = "";
    }
}
