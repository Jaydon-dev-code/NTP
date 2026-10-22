using SL.MLineDataPrecisionTracking.Models.Enum;
using System;
using System.Collections.Generic;

namespace SL.MLineDataPrecisionTracking.Models.Dtos.Mes
{
    /// <summary>
    /// 接口一：生产工单新增（/order/release）请求体
    /// </summary>
    public class MesOrderReleaseDto
    {
        /// <summary>
        /// 工单号
        /// </summary>
        public string WorkOrderNo { get; set; }

        /// <summary>
        /// 成品料号
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 成品物料描述
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 计划数量
        /// </summary>
        public double PlanQty { get; set; }

        /// <summary>
        /// 计划生产日期
        /// </summary>
        public DateTime PlanDate { get; set; }

        /// <summary>
        /// BOM 组件
        /// </summary>
        public List<MesOrderBomDto> OrderBoms { get; set; } = new List<MesOrderBomDto>();
    }

    /// <summary>
    /// 接口一 BOM 组件项
    /// </summary>
    public class MesOrderBomDto
    {
        /// <summary>
        /// 组件料号
        /// </summary>
        public string ComponentCode { get; set; }

        /// <summary>
        /// 组件名称
        /// </summary>
        public string ComponentName { get; set; }

        /// <summary>
        /// 组件数量
        /// </summary>
        public double Qty { get; set; }
    }

    /// <summary>
    /// 接口二：生产工单更新（/order/update）请求体
    /// </summary>
    public class MesOrderUpdateDto
    {
        /// <summary>
        /// 工单号
        /// </summary>
        public string WorkOrderNo { get; set; }

        /// <summary>
        /// 计划数量
        /// </summary>
        public double PlanQty { get; set; }

        /// <summary>
        /// 删除标识：true 删除
        /// </summary>
        public bool Deleted { get; set; }
    }

    /// <summary>
    /// 订单状态切换请求体
    /// </summary>
    public class MesOrderStatusDto
    {
        /// <summary>
        /// 工单号
        /// </summary>
        public string WorkOrderNo { get; set; }

        /// <summary>
        /// 目标状态：0待执行 1执行中
        /// </summary>
        public int Status { get; set; }
    }
}
