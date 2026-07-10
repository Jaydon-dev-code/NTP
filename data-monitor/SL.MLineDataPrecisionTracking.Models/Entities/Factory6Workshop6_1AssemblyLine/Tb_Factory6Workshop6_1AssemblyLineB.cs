using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_1AssemblyLine
{
    [SugarTable("Factory6Workshop6_1AssemblyLineB")]
    public class Tb_Factory6Workshop6_1AssemblyLineB
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// ColumnDescription = "a线主键ID"
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int ALineFID { get; set; }

        /// <summary>
        /// A线采集时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ALineRecordTime { get; set; }

        /// <summary>
        /// 二维码打标内容
        /// </summary>
        [SugarColumn(ColumnDescription = "序列码")]
        public string MarkingNo { get; set; }

        /// <summary>
        /// ABS检测齿数
        /// </summary>
        [SugarColumn(ColumnDescription = "ABS检测齿数")]
        public string AbsToothCount { get; set; }

        /// <summary>
        /// 托盘号
        /// </summary>
        [SugarColumn(ColumnDescription = "托盘号B")]
        public string TrayNoB { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        [SugarColumn(ColumnDescription = "型号B")]
        public string ModelNoB { get; set; }

        /// <summary>
        /// 屏蔽工位
        /// </summary>
        [SugarColumn(ColumnDescription = "屏蔽工位B", ColumnDataType = "varchar(500)")]
        public string ShieldStationB { get; set; }

        /// <summary>
        /// NG代码
        /// </summary>
        [SugarColumn(ColumnDescription = "NG代码B")]
        public string NgCodeB { get; set; }

        /// <summary>
        /// A线托盘编号
        /// </summary>
        [SugarColumn(ColumnDescription = "A线托盘编号")]
        public string LineATrayNo { get; set; }

        /// <summary>
        /// 识别代码
        /// </summary>
        [SugarColumn(ColumnDescription = "识别代码")]
        public string IdentificationCode { get; set; }

        /// <summary>
        /// 正游隙检测值
        /// </summary>
        [SugarColumn(ColumnDescription = "正游隙检测值")]
        public string PositiveClearanceValue { get; set; }

        /// <summary>
        /// 位移量
        /// </summary>
        [SugarColumn(ColumnDescription = "位移量")]
        public string DisplacementValue { get; set; }

        /// <summary>
        /// 负游隙检测值
        /// </summary>
        [SugarColumn(ColumnDescription = "负游隙检测值")]
        public string NegativeClearanceValue { get; set; }

        /// <summary>
        /// 铆接前成型高度
        /// </summary>
        [SugarColumn(ColumnDescription = "铆接前成型高度")]
        public string PreRivetingHeight { get; set; }

        /// <summary>
        /// 密封圈压装压力
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装压力")]
        public string SealRingPressPressure { get; set; }

        /// <summary>
        /// 密封圈压装位移
        /// </summary>
        [SugarColumn(ColumnDescription = "密封圈压装位移B")]
        public string SealRingPressDisplacementB { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器1
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差检测传感器1")]
        public string MagneticRingParallelSensor1 { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器2
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差检测传感器2")]
        public string MagneticRingParallelSensor2 { get; set; }

        /// <summary>
        /// 磁性圈平行差检测传感器3
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差检测传感器3")]
        public string MagneticRingParallelSensor3 { get; set; }

        /// <summary>
        /// 振动下LOAD
        /// </summary>
        [SugarColumn(ColumnDescription = "振动下LOAD")]
        public string VibrationLowerLOAD { get; set; }

        /// <summary>
        /// 振动下LH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动下LH")]
        public string VibrationLowerLH { get; set; }

        /// <summary>
        /// 振动下RH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动下RH")]
        public string VibrationLowerRH { get; set; }

        /// <summary>
        /// 振动上LOAD
        /// </summary>
        [SugarColumn(ColumnDescription = "振动上LOAD")]
        public string VibrationUpperLOAD { get; set; }

        /// <summary>
        /// 振动上LH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动上LH")]
        public string VibrationUpperLH { get; set; }

        /// <summary>
        /// 振动上RH
        /// </summary>
        [SugarColumn(ColumnDescription = "振动上RH")]
        public string VibrationUpperRH { get; set; }

        /// <summary>
        /// ABS检测峰值
        /// </summary>
        [SugarColumn(ColumnDescription = "ABS检测峰值")]
        public string AbsPeakValue { get; set; }

        /// <summary>
        /// ABS检测谷值
        /// </summary>
        [SugarColumn(ColumnDescription = "ABS检测谷值")]
        public string AbsValleyValue { get; set; }

        /// <summary>
        /// 径跳值
        /// </summary>
        [SugarColumn(ColumnDescription = "径跳值")]
        public string RadialRunout { get; set; }

        /// <summary>
        /// 磁性圈平行差
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差")]
        public string MagneticRingParallelDiff { get; set; }

        /// <summary>
        /// 端跳值轴
        /// </summary>
        [SugarColumn(ColumnDescription = "端跳值轴")]
        public string EndRunoutAxis { get; set; }

        /// <summary>
        /// 端跳值平面
        /// </summary>
        [SugarColumn(ColumnDescription = "端跳值平面")]
        public string EndRunoutPlane { get; set; }

        /// <summary>
        /// 端跳值高度
        /// </summary>
        [SugarColumn(ColumnDescription = "端跳值高度")]
        public string EndRunoutHeight { get; set; }
    }
}
