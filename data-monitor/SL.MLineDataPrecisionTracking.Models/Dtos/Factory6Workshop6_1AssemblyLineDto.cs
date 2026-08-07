using System;
using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_1AssemblyLine;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Dtos
{
    public class Factory6Workshop6_1AssemblyLineDto : ObservableObject
    {
        public Factory6Workshop6_1AssemblyLineDto(
            Tb_Factory6Workshop6_1AssemblyLineABSummary entity
        )
        {
            AutoMapProperties(entity, this);
        }

        public static Factory6Workshop6_1AssemblyLineDto NotFindMakringNo(string makingNo)
        {
            return new Factory6Workshop6_1AssemblyLineDto()
            {
                MarkingNo = makingNo,
                IsHave = false,
                Result = ResultEnum.NG,
            };
        }

        Factory6Workshop6_1AssemblyLineDto() { }

        private void AutoMapProperties(object source, object target)
        {
            if (source == null || target == null)
                return;

            PropertyInfo[] sourceProperties = source.GetType().GetProperties();
            PropertyInfo[] targetProperties = target.GetType().GetProperties();

            foreach (var sourceProp in sourceProperties)
            {
                var targetProp = Array.Find(
                    targetProperties,
                    p => p.Name == sourceProp.Name && p.CanWrite && sourceProp.CanRead
                );

                if (targetProp != null)
                {
                    object value = sourceProp.GetValue(source);
                    targetProp.SetValue(target, value);
                }
            }
        }

        [Description("记录时间")]
        public DateTime? RecordTime { get; set; }

        [Description("型号名称")]
        public string ModelName { get; set; }

        [Description("型号编号")]
        public string ModelNo { get; set; }

        [Description("检测结果")]
        public ResultEnum Result { get; set; }

        [Description("序列码")]
        public string MarkingNo { get; set; }

        [Description("外法兰半成品码")]
        public string OuterFlangeSemiFinishedCode { get; set; }

        [Description("内法兰半成品码")]
        public string InnerFlangeSemiFinishedCode { get; set; }

        [Description("A线成品时间")]
        public DateTime? ALineRecordTime { get; set; }

        [Description("屏蔽工位A")]
        public string ShieldStationA { get; set; }

        [Description("NG代码A")]
        public string NgCodeA { get; set; }

        [Description("A面钢球组差")]
        public string ASideSteelBallGroupDiff { get; set; }

        [Description("B面钢球组差")]
        public string BSideSteelBallGroupDiff { get; set; }

        [Description("A面钢球注脂量")]
        public string ASideSteelBallGreaseVolume { get; set; }

        [Description("密封圈平行差")]
        public string SealRingParallelDiff { get; set; }

        [Description("B面钢球注脂量")]
        public string BSideSteelBallGreaseVolume { get; set; }

        [Description("托盘号B")]
        public string TrayNoB { get; set; }

        [Description("屏蔽工位B")]
        public string ShieldStationB { get; set; }

        [Description("NG代码B")]
        public string NgCodeB { get; set; }

        [Description("A线托盘编号")]
        public string LineATrayNo { get; set; }

        [Description("识别代码")]
        public string IdentificationCode { get; set; }

        [Description("正游隙检测值")]
        public string PositiveClearanceValue { get; set; }

        /// <summary>
        /// 游隙压力值
        /// </summary>
        [Description("游隙压力值")]
        public string ClearancePressureValue { get; set; }

        [Description("位移量")]
        public string DisplacementValue { get; set; }

        [Description("负游隙检测值")]
        public string NegativeClearanceValue { get; set; }

        /// <summary>
        /// 实测间隙
        /// </summary>
        [Description("实测间隙")]
        public string ActualGap { get; set; }

        [Description("铆接前成型高度")]
        public string PreRivetingHeight { get; set; }

        [Description("密封圈压装压力")]
        public string SealRingPressPressure { get; set; }

        [Description("密封圈压装位移B")]
        public string SealRingPressDisplacementB { get; set; }

        [Description("磁性圈平行差检测传感器1")]
        public string MagneticRingParallelSensor1 { get; set; }

        [Description("磁性圈平行差检测传感器2")]
        public string MagneticRingParallelSensor2 { get; set; }

        [Description("磁性圈平行差检测传感器3")]
        public string MagneticRingParallelSensor3 { get; set; }

        /// <summary>
        /// 磁性圈平行差平均值
        /// </summary>
        [SugarColumn(ColumnDescription = "磁性圈平行差平均值", DefaultValue = " ")]
        public string MagneticRingParallelAVG { get; set; } = "";

        [Description("端跳值轴")]
        public string EndRunoutAxis { get; set; }

        [Description("端跳值平面")]
        public string EndRunoutPlane { get; set; }

        [Description("端跳值高度")]
        public string EndRunoutHeight { get; set; }

        /// <summary>
        /// 1400 TQ
        /// </summary>
        [Description("1400 TQ")]
        public string TQ_1400 { get; set; }

        /// <summary>
        /// 1400 TQ
        /// </summary>
        [Description("1400 M/HIGH")]
        public string M_HIGH_1400 { get; set; }

        /// <summary>
        /// 1600 P/M1400=ABSSINGLE MAX
        /// </summary>
        [Description("1600 P/M1400=ABSSINGLE MAX")]
        public string P_1600_M1400_ABSSINGLE_MAX { get; set; }

        /// <summary>
        /// 1600 P/M1400=ABSSINGLE min
        /// </summary>
        [Description("1600 P/M1400=ABSSINGLE min")]
        public string P_1600_M1400_ABSSINGLE_min { get; set; }

        /// <summary>
        /// 1600 P/M1400=ABSTOTAL
        /// </summary>
        [Description("1600 P/M1400=ABSTOTAL")]
        public string P_1600_M1400_ABSTOTAL { get; set; }

        /// <summary>
        /// 1600 P/M1400=ABSTOOTH
        /// </summary>
        [Description("1600 P/M1400=ABSTOOTH")]
        public string P_1600_M1400_ABSTOOTH { get; set; }

        /// <summary>
        /// 震动检测结果
        /// </summary>
        [Description("震动检测结果")]
        public string VibrationDetectionResults { get; set; }

        /// <summary>
        /// 跳动检测值
        /// </summary>
        [Description("跳动检测值")]
        public string RunoutValue { get; set; }

        /// <summary>
        /// 扁平度
        /// </summary>
        [Description("扁平度")]
        public string Flatness { get; set; }

        /// <summary>
        /// 1150 GONO CHECK
        /// </summary>
        [Description("1150 GONO CHECK")]
        public string GONO_CHECK_1150 { get; set; }

        private bool _isHave = true;

        public bool IsHave
        {
            get => _isHave;
            set => SetProperty(ref _isHave, value);
        }
    }
}
