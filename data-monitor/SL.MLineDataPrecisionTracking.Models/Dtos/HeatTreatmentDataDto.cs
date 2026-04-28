using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Models.Dtos
{
    /// <summary>
    /// 热处理数据DTO
    /// </summary>
    public class HeatTreatmentDataDto
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
            InternalSprayCoolingWater = entity.InternalSprayCoolingWater;
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
        public DateTime RecordTime { get; set; }

        /// <summary>
        /// 加热时间
        /// </summary>
        public string HeatingTime { get; set; }

        /// <summary>
        /// 冷却开始时间
        /// </summary>
        public string CoolingStartTime { get; set; }

        /// <summary>
        /// 冷却时间
        /// </summary>
        public string CoolingTime { get; set; }

        /// <summary>
        /// 输出功率
        /// </summary>
        public string OutputPower { get; set; }

        /// <summary>
        /// 输出电压
        /// </summary>
        public string OutputVoltage { get; set; }

        /// <summary>
        /// 输出频率
        /// </summary>
        public string OutputFrequency { get; set; }

        /// <summary>
        /// 外喷冷却水量
        /// </summary>
        public string ExternalSprayCoolingWater { get; set; }

        /// <summary>
        /// 辅喷冷却水量
        /// </summary>
        public string AuxiliarySprayCoolingWater { get; set; }

        /// <summary>
        /// 内喷冷却水量
        /// </summary>
        public string InternalSprayCoolingWater { get; set; }

        /// <summary>
        /// 二维码打标内容
        /// </summary>
        public string MarkingNo { get; set; }

        /// <summary>
        /// 是否存在
        /// </summary>
        public bool IsHave { get; set; }
    }
}