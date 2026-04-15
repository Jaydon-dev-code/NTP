using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    [SugarTable("LineB")]
    public class TB_LineB
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// 屏蔽工位
        /// </summary>
        public string ShieldStation { get; set; }

        /// <summary>
        /// NG代码
        /// </summary>
        public string NgCode { get; set; }

        /// <summary>
        /// A线托盘编号
        /// </summary>
        public string LineATrayNo { get; set; }

        /// <summary>
        /// 识别代码
        /// </summary>
        public string IdentificationCode { get; set; }

        /// <summary>
        /// 正游隙检测值
        /// </summary>
        public decimal? PositiveClearanceValue { get; set; }

        /// <summary>
        /// 位移量
        /// </summary>
        public decimal? DisplacementValue { get; set; }

        /// <summary>
        /// 负游隙检测值
        /// </summary>
        public decimal? NegativeClearanceValue { get; set; }

        /// <summary>
        /// 铆接前成型高度
        /// </summary>
        public decimal? PreRivetingHeight { get; set; }

        /// <summary>
        /// 铆接成型力
        /// </summary>
        public decimal? RivetingForce { get; set; }

        /// <summary>
        /// 铆接成型位移
        /// </summary>
        public decimal? RivetingDisplacement { get; set; }

        /// <summary>
        /// 铆接后成型高度
        /// </summary>
        public decimal? PostRivetingHeight { get; set; }

        /// <summary>
        /// 铆接后成型形状
        /// </summary>
        public string PostRivetingShape { get; set; }

        /// <summary>
        /// 铆接后成型外径
        /// </summary>
        public decimal? PostRivetingOuterDiameter { get; set; }

        /// <summary>
        /// 二维码打标内容
        /// </summary>
        public string QrCodeContent { get; set; }

        /// <summary>
        /// 径跳值
        /// </summary>
        public decimal? RadialRunout { get; set; }

        /// <summary>
        /// 端跳值
        /// </summary>
        public decimal? EndRunout { get; set; }

        /// <summary>
        /// 扭矩检测值
        /// </summary>
        public decimal? TorqueValue { get; set; }

        /// <summary>
        /// ABS检测峰值
        /// </summary>
        public decimal? AbsPeakValue { get; set; }

        /// <summary>
        /// ABS检测谷值
        /// </summary>
        public decimal? AbsValleyValue { get; set; }

        /// <summary>
        /// ABS检测齿数
        /// </summary>
        public int? AbsToothCount { get; set; }

        /// <summary>
        /// 密封圈压装压力
        /// </summary>
        public decimal? SealRingPressPressure { get; set; }

        /// <summary>
        /// 密封圈压装位移
        /// </summary>
        public decimal? SealRingPressDisplacement { get; set; }

        /// <summary>
        /// 齿圈压装压力（未使用）
        /// </summary>
        public decimal? GearRingPressPressure { get; set; }

        /// <summary>
        /// 齿圈压装位移（未使用）
        /// </summary>
        public decimal? GearRingPressDisplacement { get; set; }

        /// <summary>
        /// 防尘盖压装压力
        /// </summary>
        public decimal DustCoverPressPressure { get; set; }

        /// <summary>
        /// 防尘盖压装位移
        /// </summary>
        public decimal? DustCoverPressDisplacement { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器1
        /// </summary>
        public decimal? MagneticRingParallelSensor1 { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器2
        /// </summary>
        public decimal? MagneticRingParallelSensor2 { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器3
        /// </summary>
        public decimal? MagneticRingParallelSensor3 { get; set; }

        /// <summary>
        /// 磁性圈平行差
        /// </summary>
        public decimal? MagneticRingParallelDiff { get; set; }

        /// <summary>
        /// 振动下LOAD
        /// </summary>
        public decimal? VibrationLowerLoad { get; set; }

        /// <summary>
        /// 振动下LH
        /// </summary>
        public decimal? VibrationLowerLh { get; set; }

        /// <summary>
        /// 振动下RH
        /// </summary>
        public decimal? VibrationLowerRh { get; set; }

        /// <summary>
        /// 振动上LOAD
        /// </summary>
        public decimal? VibrationUpperLoad { get; set; }

        /// <summary>
        /// 振动上LH
        /// </summary>
        public decimal? VibrationUpperLh { get; set; }

        /// <summary>
        /// 振动上RH
        /// </summary>
        public decimal? VibrationUpperRh { get; set; }
    }
}
