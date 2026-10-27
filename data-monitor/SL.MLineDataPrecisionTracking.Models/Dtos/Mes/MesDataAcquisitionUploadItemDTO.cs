using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Mes
{
    public class MesDataAcquisitionUploadItemDTO
    {
        /// <summary>
        /// 工厂编码
        /// </summary>
        [JsonProperty("plantCode")]
        public string PlantCode { get; set; }

        /// <summary>
        /// SN号
        /// </summary>
        [JsonProperty("serialNo")]
        public string SerialNo { get; set; }
        [JsonProperty("workOrderNo")]
        public string WorkOrderNo { get; set; }

        /// <summary>
        /// 测试项编号（数采点位）
        /// </summary>
        [JsonProperty("testCode")]
        public string TestCode { get; set; }

        /// <summary>
        /// 设备编码
        /// </summary>
        [JsonProperty("equipCode")]
        public string EquipCode { get; set; }

        /// <summary>
        /// 测试时间，格式 yyyy-MM-dd HH:mm:ss
        /// </summary>
        [JsonProperty("testTime")]
        public string TestTime { get; set; }

        /// <summary>
        /// 测试值（数采值）
        /// </summary>
        [JsonProperty("testValue")]
        public string? TestValue { get; set; }

        /// <summary>
        /// 测试结果；不传则后端按上下限判定
        /// </summary>
        [JsonProperty("testResult")]
        public string? TestResult { get; set; }

        /// <summary>
        /// 工序编码；空按 "" 处理
        /// </summary>
        [JsonProperty("routeCode")]
        public string? RouteCode { get; set; }

        /// <summary>
        /// NG原因
        /// </summary>
        [JsonProperty("failedReason")]
        public string? FailedReason { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [JsonProperty("remark")]
        public string? Remark { get; set; }

        /// <summary>
        /// 产品物料编码
        /// </summary>
        [JsonProperty("[proMaterialCode")]
        public string? ProMaterialCode { get; set; }
    }
}
