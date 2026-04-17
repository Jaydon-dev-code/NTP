
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Dtos
{
    public class LineSummaryDto
    {
        public LineSummaryDto(Tb_LineSummary tb_LineSummary)
        {
            // 自动映射所有同名属性
            AutoMapProperties(tb_LineSummary, this);
        }
        /// <summary>
        /// 反射自动赋值同名属性
        /// </summary>
        private void AutoMapProperties(object source, object target)
        {
            if (source == null || target == null)
                return;

            PropertyInfo[] sourceProperties = source.GetType().GetProperties();
            PropertyInfo[] targetProperties = target.GetType().GetProperties();

            foreach (var sourceProp in sourceProperties)
            {
                // 找目标对象中同名、可读、可写的属性
                var targetProp = Array.Find(targetProperties, p =>
                    p.Name == sourceProp.Name &&
                    p.CanWrite &&
                    sourceProp.CanRead);

                if (targetProp != null)
                {
                    // 类型兼容就赋值
                    object value = sourceProp.GetValue(source);
                    targetProp.SetValue(target, value);
                }
            }
        }
        public DateTime RecordTime { get; set; } = DateTime.Now;

        [SugarColumn(ColumnDescription = "型号名称A")]
        public string ModelNameA { get; set; }

        [SugarColumn(ColumnDescription = "型号名称B")]
        public string ModelNameB { get; set; }

        /// <summary>
        /// 托盘号
        /// </summary>
        [Description( "托盘号")]
        public int TrayNoA { get; set; }
        /// <summary>
        /// 托盘号
        /// </summary>
        [Description("托盘号")]
        public int TrayNoB { get; set; }

        /// <summary>
        /// 屏蔽工位
        /// </summary>
        [Description( "屏蔽工位A")]
        public int ShieldStationA { get; set; }

        /// <summary>
        /// NG代码
        /// </summary>
        [Description( "NG代码A")]
        public int NgCodeA { get; set; }

        /// <summary>
        /// 小内圈分选数据
        /// </summary>
        [Description( "小内圈分选数据")]
        public float SmallInnerRingSortingData { get; set; }

        /// <summary>
        /// 外法兰分选数据
        /// </summary>
        [Description( "外法兰分选数据")]
        public float OuterFlangeSortingData { get; set; }

        /// <summary>
        /// 内法兰分选数据
        /// </summary>
        [Description( "内法兰分选数据")]
        public float InnerFlangeSortingData { get; set; }

        /// <summary>
        /// A面钢球组差
        /// </summary>
        [Description( "A面钢球组差")]
        public float ASideSteelBallGroupDiff { get; set; }

        /// <summary>
        /// B面钢球组差
        /// </summary>
        [Description( "B面钢球组差")]
        public float BSideSteelBallGroupDiff { get; set; }

        /// <summary>
        /// B面钢球注脂量
        /// </summary>
        [Description( "B面钢球注脂量")]
        public float BSideSteelBallGreaseVolume { get; set; }

        /// <summary>
        /// 密封圈注脂量
        /// </summary>
        [Description( "密封圈注脂量")]
        public float SealRingGreaseVolume { get; set; }

        /// <summary>
        /// A面钢球注脂量
        /// </summary>
        [Description( "A面钢球注脂量")]
        public float ASideSteelBallGreaseVolume { get; set; }

        /// <summary>
        /// 密封圈压装力
        /// </summary>
        [Description( "密封圈压装力")]
        public float SealRingPressForce { get; set; }

        /// <summary>
        /// 密封圈压装位移
        /// </summary>
        [Description( "密封圈压装位移A")]
        public float SealRingPressDisplacementA { get; set; }

        /// <summary>
        /// 挡水环压装力
        /// </summary>
        [Description( "挡水环压装力")]
        public float WaterBafflePressForce { get; set; }

        /// <summary>
        /// 挡水环压装位移
        /// </summary>
        [Description( "挡水环压装位移")]
        public float WaterBafflePressDisplacement { get; set; }

        /// <summary>
        /// 小内圈合套压力
        /// </summary>
        [Description( "小内圈合套压力")]
        public float SmallInnerRingAssemblePressure { get; set; }

        /// <summary>
        /// 小内圈合套位移
        /// </summary>
        [Description( "小内圈合套位移")]
        public float SmallInnerRingAssembleDisplacement { get; set; }

        /// <summary>
        /// 密封圈平行差
        /// </summary>
        [Description( "密封圈平行差")]
        public float SealRingParallelDiff { get; set; }

        /// <summary>
        /// 挡水环平行差
        /// </summary>
        [Description( "挡水环平行差")]
        public float WaterBaffleParallelDiff { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        [Description( "型号")]
        public int ModelNo { get; set; }

        /// <summary>
        /// 屏蔽工位
        /// </summary>
        [Description( "屏蔽工位B")]
        public int ShieldStationB { get; set; }

        /// <summary>
        /// NG代码
        /// </summary>
        [Description( "NG代码B")]
        public int NgCodeB { get; set; }

        ///// <summary>
        ///// A线托盘编号
        ///// </summary>
        //[Description( "A线托盘编号")]
        //public int LineATrayNo { get; set; }

        /// <summary>
        /// 识别代码
        /// </summary>
        [Description( "识别代码")]
        public int IdentificationCode { get; set; }

        /// <summary>
        /// 正游隙检测值
        /// </summary>
        [Description( "正游隙检测值")]
        public float PositiveClearanceValue { get; set; }

        /// <summary>
        /// 位移量
        /// </summary>
        [Description( "位移量")]
        public float DisplacementValue { get; set; }

        /// <summary>
        /// 负游隙检测值
        /// </summary>
        [Description( "负游隙检测值")]
        public float NegativeClearanceValue { get; set; }

        /// <summary>
        /// 铆接前成型高度
        /// </summary>
        [Description( "铆接前成型高度")]
        public decimal PreRivetingHeight { get; set; }

        /// <summary>
        /// 铆接成型力
        /// </summary>
        [Description( "铆接成型力")]
        public decimal RivetingForce { get; set; }

        /// <summary>
        /// 铆接成型位移
        /// </summary>
        [Description( "铆接成型位移")]
        public decimal RivetingDisplacement { get; set; }

        /// <summary>
        /// 铆接后成型高度
        /// </summary>
        [Description( "铆接后成型高度")]
        public decimal PostRivetingHeight { get; set; }

        /// <summary>
        /// 铆接后成型形状
        /// </summary>
        [Description( "铆接后成型形状")]
        public string PostRivetingShape { get; set; }

        /// <summary>
        /// 铆接后成型外径
        /// </summary>
        [Description( "铆接后成型外径")]
        public decimal PostRivetingOuterDiameter { get; set; }

        /// <summary>
        /// 二维码打标内容
        /// </summary>
        [Description( "二维码打标内容")]
        public string QrCodeContent { get; set; }

        /// <summary>
        /// 径跳值
        /// </summary>
        [Description( "径跳值")]
        public decimal RadialRunout { get; set; }

        /// <summary>
        /// 端跳值
        /// </summary>
        [Description( "端跳值")]
        public decimal EndRunout { get; set; }

        /// <summary>
        /// 扭矩检测值
        /// </summary>
        [Description( "扭矩检测值")]
        public decimal TorqueValue { get; set; }

        /// <summary>
        /// ABS检测峰值
        /// </summary>
        [Description( "ABS检测峰值")]
        public decimal AbsPeakValue { get; set; }

        /// <summary>
        /// ABS检测谷值
        /// </summary>
        [Description( "ABS检测谷值")]
        public decimal AbsValleyValue { get; set; }

        /// <summary>
        /// ABS检测齿数
        /// </summary>
        [Description( "ABS检测齿数")]
        public int? AbsToothCount { get; set; }

        /// <summary>
        /// 密封圈压装压力
        /// </summary>
        [Description( "密封圈压装压力")]
        public decimal SealRingPressPressure { get; set; }

        /// <summary>
        /// 密封圈压装位移
        /// </summary>
        [Description( "密封圈压装位移B")]
        public decimal SealRingPressDisplacementB { get; set; }

        /// <summary>
        /// 齿圈压装压力
        /// </summary>
        [Description( "齿圈压装压力")]
        public decimal GearRingPressPressure { get; set; }

        /// <summary>
        /// 齿圈压装位移
        /// </summary>
        [Description( "齿圈压装位移")]
        public decimal GearRingPressDisplacement { get; set; }

        /// <summary>
        /// 防尘盖压装压力
        /// </summary>
        [Description( "防尘盖压装压力")]
        public decimal DustCoverPressPressure { get; set; }

        /// <summary>
        /// 防尘盖压装位移
        /// </summary>
        [Description( "防尘盖压装位移")]
        public decimal DustCoverPressDisplacement { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器1
        /// </summary>
        [Description( "磁性圈平行差检测传感器1")]
        public decimal MagneticRingParallelSensor1 { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器2
        /// </summary>
        [Description( "磁性圈平行差检测传感器2")]
        public decimal MagneticRingParallelSensor2 { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器3
        /// </summary>
        [Description( "磁性圈平行差检测传感器3")]
        public decimal MagneticRingParallelSensor3 { get; set; }

        /// <summary>
        /// 磁性圈平行差
        /// </summary>
        [Description( "磁性圈平行差")]
        public decimal MagneticRingParallelDiff { get; set; }

        /// <summary>
        /// 振动下LOAD
        /// </summary>
        [Description( "振动下LOAD")]
        public decimal VibrationLowerLOAD { get; set; }

        /// <summary>
        /// 振动下LH
        /// </summary>
        [Description( "振动下LH")]
        public decimal VibrationLowerLH { get; set; }

        /// <summary>
        /// 振动下RH
        /// </summary>
        [Description( "振动下RH")]
        public decimal VibrationLowerRH { get; set; }

        /// <summary>
        /// 振动上LOAD
        /// </summary>
        [Description( "振动上LOAD")]
        public decimal VibrationUpperLOAD { get; set; }

        /// <summary>
        /// 振动上LH
        /// </summary>
        [Description( "振动上LH")]
        public decimal VibrationUpperLH { get; set; }

        /// <summary>
        /// 振动上RH
        /// </summary>
        [Description( "振动上RH")]
        public decimal VibrationUpperRH { get; set; }
    }
}
