using SqlSugar;
using System;
using System.Collections.Generic;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    [SugarTable("EnergyRange")]
    public class Tb_EnergyRange
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        [SugarColumn(ColumnDescription = "产品型号")]
        public string ProductModel { get; set; }

        [SugarColumn(ColumnDescription = "工位分配", IsNullable = true)]
        public string Station { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Navigate(NavigateType.OneToMany, nameof(Tb_EnergyRangeDetail.EnergyRangeId))]
        public List<Tb_EnergyRangeDetail> Details { get; set; }
    }
}
