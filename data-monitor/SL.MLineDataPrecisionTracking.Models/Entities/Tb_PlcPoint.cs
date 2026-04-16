using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Models.Entities
{
    /// <summary>
    /// PLC采集点位（核心）
    /// </summary>
    [SugarTable("PlcPoint")]
    public class Tb_PlcPoint
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public int PlcConnectionId { get; set; }

        /// <summary>变量名称</summary>
        public string PointName { get; set; }

        /// <summary>描述</summary>
        public string Description { get; set; }

        /// <summary>区块 D/M/X/Y/R</summary>
        public string Area { get; set; }

        /// <summary>起始地址</summary>
        public int Address { get; set; }

        /// <summary>数据类型 Int16/Int32/Float/Bool/String</summary>
        public string DataType { get; set; }

        /// <summary>数组长度/连续读取长度</summary>
        public int Length { get; set; } = 1;

        public int Sort { get; set; }

        /// <summary>
        /// 下发公式
        /// </summary>
        public string WriteFormula { get; set; } = "";
        public string ReadFormula { get; set; } = "";
        public string Remark { get; set; } = "";
    }
}
