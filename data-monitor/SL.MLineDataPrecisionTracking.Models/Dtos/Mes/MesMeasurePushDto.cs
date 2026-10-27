using System;
using System.Collections.Generic;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Mes
{
    /// <summary>
    /// MES 测量数据推送报文（自定义，工控机→MES）
    /// </summary>
    public class MesMeasurePushDto
    {
        /// <summary>
        /// 来源表名（三方库表名，中文）
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 工单号（存在执行中订单时携带，否则为空）
        /// </summary>
        public string WorkOrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 批次号（表名|批次最大时间|条数，供 MES 幂等）
        /// </summary>
        public string BatchNo { get; set; }

        /// <summary>
        /// 本批最大测量时间
        /// </summary>
        public DateTime MeasureTime { get; set; }

        /// <summary>
        /// 本批条数
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        /// 创建人员（固定值）
        /// </summary>
        public string CreateBy { get; set; } = "工控机";

        /// <summary>
        /// 测量数据行（中文列名做键，除 image 外全列）
        /// </summary>
        public List<Dictionary<string, object>> Records { get; set; } =
            new List<Dictionary<string, object>>();
    }
}
