using CommunityToolkit.Mvvm.ComponentModel;
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
    public class HeatTreatmentDataDto : ObservableObject
    {
        public HeatTreatmentDataDto(Tb_HeatTreatmentData tb_LineSummary)
        {
            // 自动映射所有同名属性
            AutoMapProperties(tb_LineSummary, this);
        }
        [Description("时间")]
        public DateTime RecordTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 加热时间（单位：秒/分钟，根据业务定义）
        /// </summary>
        [Description("加热时间（秒）")]
        public string HeatingTime { get; set; }

        /// <summary>
        /// 冷却开始时间（yyyy-MM-dd HH:mm:ss 格式）
        /// </summary>
        [Description("冷却时间（秒）")]
        public string CoolingStartTime { get; set; }

        /// <summary>
        /// 冷却时间（单位：秒/分钟，根据业务定义）
        /// </summary>
        [Description("冷却时间（秒）")]
        public string CoolingTime { get; set; }

        /// <summary>
        /// 输出功率（单位：kW/W）
        /// </summary>
        [Description("输出功率（kw）")]
        public string OutputPower { get; set; }

        /// <summary>
        /// 输出电压（单位：V）
        /// </summary>
        [Description("输出电压（V）")]
        public string OutputVoltage { get; set; }

        /// <summary>
        /// 输出频率（单位：Hz）
        /// </summary>
        [Description("输出频率（kHz）")]
        public string OutputFrequency { get; set; }

        /// <summary>
        /// 外喷冷却水量
        /// </summary>
        [Description("外喷冷却水量（L/分）")]
        public string ExternalSprayCoolingWater { get; set; }

        /// <summary>
        /// 辅喷冷却水量
        /// </summary>
        [Description("辅喷冷却水量（L/分）")]
        public string AuxiliarySprayCoolingWater { get; set; }

        /// <summary>
        /// 内喷冷却水量
        /// </summary>
        [Description("内喷冷却水量（L/分）")]
        public string InternalSprayCoolingWater { get; set; }

        /// <summary>
        /// 二维码打标内容
        /// </summary>
        [Description("序列码")]
        public string MarkingNo { get; set; }

        private bool _isHave = true;
        [Description("是否存在记录")]
        public bool IsHave
        {
            get => _isHave;
            set => SetProperty(ref _isHave, value);
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
                var targetProp = Array.Find(
                    targetProperties,
                    p => p.Name == sourceProp.Name && p.CanWrite && sourceProp.CanRead
                );

                if (targetProp != null)
                {
                    // 类型兼容就赋值
                    object value = sourceProp.GetValue(source);
                    targetProp.SetValue(target, value);
                }
            }
        }
    }
}
