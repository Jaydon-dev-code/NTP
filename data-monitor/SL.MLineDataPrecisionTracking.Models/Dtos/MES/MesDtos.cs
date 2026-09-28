using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.MES

{
    /// <summary>
    /// 接口一：生产工单新增请求体（MES → 工控机 POST /order/release）
    /// </summary>
    public class MesOrderReleaseRequest
    {
        /// <summary>工单号</summary>
        [JsonProperty("workOrderNo")]
        public string WorkOrderNo { get; set; }

        /// <summary>成品料号</summary>
        [JsonProperty("materialCode")]
        public string MaterialCode { get; set; }

        /// <summary>成品物料描述</summary>
        [JsonProperty("materialName")]
        public string MaterialName { get; set; }

        /// <summary>计划数量</summary>
        [JsonProperty("planQty")]
        public decimal PlanQty { get; set; }

        /// <summary>计划生产日期</summary>
        [JsonProperty("planDate")]
        public DateTime? PlanDate { get; set; }

        /// <summary>BOM 组件</summary>
        [JsonProperty("orderBoms")]
        public List<MesOrderBomItem> OrderBoms { get; set; }
    }

    /// <summary>
    /// 接口一：BOM 组件项
    /// </summary>
    public class MesOrderBomItem
    {
        /// <summary>组件料号</summary>
        [JsonProperty("componentCode")]
        public string ComponentCode { get; set; }

        /// <summary>组件名称</summary>
        [JsonProperty("componentName")]
        public string ComponentName { get; set; }

        /// <summary>组件数量</summary>
        [JsonProperty("Qty")]
        public decimal Qty { get; set; }
    }

    /// <summary>
    /// 接口二：生产工单更新请求体（MES → 工控机 POST /order/update）
    /// </summary>
    public class MesOrderUpdateRequest
    {
        /// <summary>工单号</summary>
        [JsonProperty("workOrderNo")]
        public string WorkOrderNo { get; set; }

        /// <summary>计划数量</summary>
        [JsonProperty("planQty")]
        public decimal PlanQty { get; set; }

        /// <summary>删除标识：true 删除工单</summary>
        [JsonProperty("deleted")]
        public bool Deleted { get; set; }
    }

    /// <summary>
    /// 接口三：产出上报请求体（工控机 → MES POST /process/completion）
    /// </summary>
    public class MesProcessCompletionRequest
    {
        /// <summary>工单号</summary>
        [JsonProperty("workOrderNo")]
        public string WorkOrderNo { get; set; }

        /// <summary>托盘号</summary>
        [JsonProperty("palletNo")]
        public string PalletNo { get; set; }

        /// <summary>合格数量</summary>
        [JsonProperty("goodQty")]
        public decimal GoodQty { get; set; }

        /// <summary>不合格数量</summary>
        [JsonProperty("badQty")]
        public decimal BadQty { get; set; }

        /// <summary>不合格原因</summary>
        [JsonProperty("badReason")]
        public string BadReason { get; set; }

        /// <summary>机台编号（与设备台账编号一致）</summary>
        [JsonProperty("equipCode")]
        public string EquipCode { get; set; }

        /// <summary>机器人（AGV 编号）</summary>
        [JsonProperty("agvCode")]
        public string AgvCode { get; set; }

        /// <summary>创建人员（固定值）</summary>
        [JsonProperty("createBy")]
        public string CreateBy { get; set; }
    }

    /// <summary>
    /// MES 接口统一响应体（对应文档 {"code": 200, "message": "OK"}）
    /// </summary>
    public class MesApiResponse
    {
        /// <summary>状态码：200 成功</summary>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>提示信息</summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        public static MesApiResponse Success() =>
            new MesApiResponse { Code = 200, Message = "OK" };

        public static MesApiResponse Fail(string message, int code = 500) =>
            new MesApiResponse { Code = code, Message = message };
    }
}
