using Newtonsoft.Json;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    [SugarTable("EnergyRangeDetail")]
    public class Tb_EnergyRangeDetail
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public int EnergyRangeId { get; set; }

        [SugarColumn(ColumnDescription = "时间")]
        public int Time { get; set; }

        [SugarColumn(ColumnDescription = "下限值", ColumnDataType = "numeric(18,2)")]
        public decimal LowerLimit { get; set; }

        [SugarColumn(ColumnDescription = "上限值", ColumnDataType = "numeric(18,2)")]
        public decimal UpperLimit { get; set; }

        [JsonIgnore]
        [Navigate(NavigateType.OneToOne, nameof(EnergyRangeId))]
        public Tb_EnergyRange EnergyRange { get; set; }
    }
}
