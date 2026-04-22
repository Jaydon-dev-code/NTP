using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    [SugarTable("HeatTreatmentData")]
    public class Tb_HeatTreatmentData
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public DateTime RecordTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 加热时间（单位：秒/分钟，根据业务定义）
        /// </summary>
        [SugarColumn(ColumnDescription = "加热时间")]
        public string HeatingTime { get; set; }

        /// <summary>
        /// 冷却开始时间（yyyy-MM-dd HH:mm:ss 格式）
        /// </summary>
        [SugarColumn(ColumnDescription = "冷却时间")]
        public DateTime CoolingStartTime { get; set; }

        /// <summary>
        /// 冷却时间（单位：秒/分钟，根据业务定义）
        /// </summary>
        [SugarColumn(ColumnDescription = "冷却时间")]
        public string CoolingTime { get; set; }

        /// <summary>
        /// 输出功率（单位：kW/W）
        /// </summary>
        [SugarColumn(ColumnDescription = "输出功率")]
        public string OutputPower { get; set; }

        /// <summary>
        /// 输出电压（单位：V）
        /// </summary>
        [SugarColumn(ColumnDescription = "输出电压")]
        public string OutputVoltage { get; set; }

        /// <summary>
        /// 输出频率（单位：Hz）
        /// </summary>
        [SugarColumn(ColumnDescription = "输出频率")]
        public string OutputFrequency { get; set; }

        /// <summary>
        /// 外喷冷却水量
        /// </summary>
        [SugarColumn(ColumnDescription = "外喷冷却水量")]
        public string ExternalSprayCoolingWater { get; set; }

        /// <summary>
        /// 辅喷冷却水量
        /// </summary>
        [SugarColumn(ColumnDescription = "辅喷冷却水量")]
        public string AuxiliarySprayCoolingWater { get; set; }

        /// <summary>
        /// 内喷冷却水量
        /// </summary>
        [SugarColumn(ColumnDescription = "内喷冷却水量")]
        public string InternalSprayCoolingWater { get; set; }

        /// <summary>
        /// 二维码打标内容
        /// </summary>
        [SugarColumn(ColumnDescription = "二维码打标内容")]
        public string MarkingNo { get; set; }
    }
}