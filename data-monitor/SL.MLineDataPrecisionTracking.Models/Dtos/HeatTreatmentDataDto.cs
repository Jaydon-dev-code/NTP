using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Models.Dtos
{
    /// <summary>
    /// 热处理数据DTO
    /// </summary>
    public class HeatTreatmentDataDto : ObservableObject
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="entity">实体对象</param>
        public HeatTreatmentDataDto(Tb_HeatTreatmentData entity)
        {
            Id = entity.Id;
            RecordTime = entity.RecordTime;
            HeatingTime = entity.HeatingTime;
            CoolingStartTime = entity.CoolingStartTime;
            CoolingTime = entity.CoolingTime;
            OutputPower = entity.OutputPower;
            OutputVoltage = entity.OutputVoltage;
            OutputFrequency = entity.OutputFrequency;
            ExternalSprayCoolingWater = entity.ExternalSprayCoolingWater;
            AuxiliarySprayCoolingWater = entity.AuxiliarySprayCoolingWater;
            MarkingNo = entity.MarkingNo;
            IsHave = true;
        }

        /// <summary>
        /// 构造函数（无数据时）
        /// </summary>
        /// <param name="markingNo">条码号</param>
        public HeatTreatmentDataDto(string markingNo)
        {
            MarkingNo = markingNo;
            IsHave = false;
        }

        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 记录时间
        /// </summary>
        [Description("记录时间")]
        public DateTime RecordTime { get; set; }

        /// <summary>
        /// 二维码打标内容
        /// </summary>
        [Description("序列码")]
        public string MarkingNo { get; set; }

        /// <summary>
        /// 加热时间
        /// </summary>
        [Description("加热时间（秒）")]
        public string HeatingTime { get; set; }

        /// <summary>
        /// 冷却开始时间
        /// </summary>
        [Description("冷却开始时间（秒）")]
        public string CoolingStartTime { get; set; }

        /// <summary>
        /// 冷却时间
        /// </summary>
        [Description("冷却时间（秒）")]
        public string CoolingTime { get; set; }

        /// <summary>
        /// 输出功率
        /// </summary>
        [Description("输出功率（kw）")]
        public string OutputPower { get; set; }

        /// <summary>
        /// 输出电压
        /// </summary>
        [Description("输出电压（V）")]
        public string OutputVoltage { get; set; }

        /// <summary>
        /// 输出频率
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

        private bool _isHave = true;

        [Description("是否存在记录")]
        public bool IsHave
        {
            get => _isHave;
            set => SetProperty(ref _isHave, value);
        }
    }
}
