using SL.MLineDataPrecisionTracking.Infrastructure.Http;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage.MES;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.MES;
using SL.MLineDataPrecisionTracking.Models.Entities.MES;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Core.Mes
{
    /// <summary>
    /// MES 对接服务：工单接收（接口一）、工单更新（接口二）、产出上报（接口三）
    /// </summary>
    public class MesOrderService : BaseHttp
    {
        private readonly Tb_MesWorkOrderRepository _workOrderRepository;

        public MesOrderService(Tb_MesWorkOrderRepository workOrderRepository)
        {
            _workOrderRepository = workOrderRepository;
        }

        protected override void Init()
        {
            _client = new HttpClient
            {
                // 这里配置你的后端地址
                BaseAddress = new Uri($"{ConfigurationManager.AppSettings["MESApiUri"]}/"),
            };

            // 默认 JSON 请求
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        /// <summary>
        /// 接口一：接收 MES 下发的生产工单（POST /order/release）
        /// </summary>
        public async Task<MesApiResponse> ReceiveOrderAsync(MesOrderReleaseRequest request)
        {
            // ① 入参校验：工单号、成品料号必填，计划数量须为正数
            if (request == null || string.IsNullOrWhiteSpace(request.WorkOrderNo))
                return MesApiResponse.Fail("工单号不能为空");

            if (string.IsNullOrWhiteSpace(request.MaterialCode))
                return MesApiResponse.Fail("成品料号不能为空");

            if (request.PlanQty <= 0)
                return MesApiResponse.Fail("计划数量必须大于0");

            // ② 工单号查重：同一工单不允许重复下发
            var exists = await _workOrderRepository.AnyAsync(x =>
                x.WorkOrderNo == request.WorkOrderNo
            );
            if (exists)
                return MesApiResponse.Fail($"工单号 \"{request.WorkOrderNo}\" 已存在");

            // ③ 组装工单主表记录
            var order = new Tb_Mes_WorkOrder
            {
                WorkOrderNo = request.WorkOrderNo,
                MaterialCode = request.MaterialCode,
                MaterialName = request.MaterialName ?? "",
                PlanQty = request.PlanQty,
                PlanDate = request.PlanDate,
                CreateTime = DateTime.Now,
                IsDeleted = false
            };

            // ④ 组装 BOM 明细：每条明细以工单号作为关联键（MES 未下发 BOM 时为空列表）
            var boms = new List<Tb_Mes_WorkOrderBom>();
            if (request.OrderBoms != null)
            {
                foreach (var item in request.OrderBoms)
                {
                    boms.Add(
                        new Tb_Mes_WorkOrderBom
                        {
                            WorkOrderNo = request.WorkOrderNo,
                            ComponentCode = item.ComponentCode,
                            ComponentName = item.ComponentName ?? "",
                            Qty = item.Qty,
                            CreateTime = DateTime.Now,
                        }
                    );
                }
            }

            // ⑤ 主表 + 明细在同一事务中落库，任一步失败整体回滚
            await _workOrderRepository.InsertOrderWithBomsAsync(order, boms);
            return MesApiResponse.Success();
        }

        /// <summary>
        /// 接口二：更新工单计划数量，或删除工单（POST /order/update）
        /// </summary>
        public async Task<MesApiResponse> UpdateOrderAsync(MesOrderUpdateRequest request)
        {
            // ① 入参校验：工单号必填
            if (request == null || string.IsNullOrWhiteSpace(request.WorkOrderNo))
                return MesApiResponse.Fail("工单号不能为空");

            // ② 校验工单存在，避免对不存在的工单做更新/删除
            var exists = await _workOrderRepository.AnyAsync(x =>
                x.WorkOrderNo == request.WorkOrderNo
            );
            if (!exists)
                return MesApiResponse.Fail($"工单号 \"{request.WorkOrderNo}\" 不存在");

            // ③ 按删除标识分流：deleted=true 整单删除（含 BOM 明细）；false 仅更新计划数量
            //    （MES 可能同时下发 planQty 与 deleted=true，此时以删除为准）
            if (request.Deleted)
            {
                await _workOrderRepository.DeleteOrderAsync(request.WorkOrderNo);
            }
            else
            {
                await _workOrderRepository.UpdatePlanQtyAsync(request.WorkOrderNo, request.PlanQty);
            }

            return MesApiResponse.Success();
        }

        /// <summary>
        /// 接口三：产出上报（工控机 → MES POST /process/completion）— 暂未实现
        /// </summary>
        /// <param name="request">产出上报请求体</param>
        public async Task<MesApiResponse> ReportCompletionAsync(MesProcessCompletionRequest request)
        {
            return await PostExternalAsync<MesApiResponse>("/process/completion", request);
        }
    }
}
