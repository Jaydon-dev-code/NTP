using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    /// <summary>
    /// 服务状态记录表
    /// </summary>
    [SugarTable("ServiceStatus")]
    public class Tb_ServiceStatus
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 服务ID
        /// </summary>
        [SugarColumn(ColumnDescription = "服务ID")]
        public string ServiceId { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        [SugarColumn(ColumnDescription = "服务名称")]
        public string ServiceName { get; set; }

        /// <summary>
        /// 服务类型
        /// </summary>
        [SugarColumn(ColumnDescription = "服务类型")]
        public string ServiceType { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(ColumnDescription = "是否启用")]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 最后操作时间
        /// </summary>
        [SugarColumn(ColumnDescription = "最后操作时间")]
        public DateTime LastOperationTime { get; set; }

        /// <summary>
        /// 最后操作人
        /// </summary>
        [SugarColumn(ColumnDescription = "最后操作人")]
        public string LastOperator { get; set; }

        /// <summary>
        /// 操作备注
        /// </summary>
        [SugarColumn(ColumnDescription = "操作备注")]
        public string Remark { get; set; }
    }
}