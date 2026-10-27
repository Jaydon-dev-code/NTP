using SqlSugar;
using System;

namespace SL.MLineDataPrecisionTracking.Models.Entities.Mes
{
    /// <summary>
    /// MES 推送游标表（每张三方测量表一条记录，CursorKey=表名）
    /// </summary>
    [SugarTable("Mes_PushCursor")]
    public class Tb_MesPushCursor
    {
        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 游标键（= 三方表名）
        /// </summary>
        [SugarColumn(ColumnDescription = "游标键", Length = 100)]
        public string CursorKey { get; set; }

        /// <summary>
        /// 上次推送的最大测量时间
        /// </summary>
        [SugarColumn(ColumnDescription = "上次最大测量时间")]
        public DateTime LastMeasureTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SugarColumn(ColumnDescription = "更新时间")]
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }
}
