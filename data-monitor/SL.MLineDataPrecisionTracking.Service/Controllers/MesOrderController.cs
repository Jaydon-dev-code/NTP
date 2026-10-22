using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;
using SL.MLineDataPrecisionTracking.Models.Entities.Mes;
using SL.MLineDataPrecisionTracking.Models.Enum;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    /// <summary>
    /// MES 生产工单接口
    /// 接口一：生产工单新增 /order/release
    /// 接口二：生产工单更新 /order/update
    /// 订单状态切换：/order/setStatus
    /// </summary>
    [RoutePrefix("order")]
    public class MesOrderController : ApiController
    {
        private readonly Tb_MesOrderRepository _mesOrderRepository;

        public MesOrderController(Tb_MesOrderRepository mesOrderRepository)
        {
            _mesOrderRepository = mesOrderRepository;
        }

        /// <summary>
        /// 接口一：生产工单新增
        /// </summary>
        [HttpPost]
        [Route("release")]
        public async Task<ApiResult> Release([FromBody] MesOrderReleaseDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.WorkOrderNo))
            {
                return new ApiResult { Code = 500, IsSuccess = false, Message = "工单号不能为空" };
            }

            var exists = await _mesOrderRepository.GetByWorkOrderNoAsync(dto.WorkOrderNo);
            if (exists != null)
            {
                return new ApiResult { Code = 500, IsSuccess = false, Message = "工单已存在" };
            }

            var order = new Tb_MesOrder
            {
                WorkOrderNo = dto.WorkOrderNo,
                MaterialCode = dto.MaterialCode,
                MaterialName = dto.MaterialName,
                PlanQty = dto.PlanQty,
                PlanDate = dto.PlanDate,
                Status = 0,
                CreateTime = DateTime.Now,
            };

            var orderId = await _mesOrderRepository.InsertAsync(order);

            if (dto.OrderBoms != null && dto.OrderBoms.Any())
            {
                var boms = dto.OrderBoms
                    .Select(b => new Tb_MesOrderBom
                    {
                        MesOrderId = orderId,
                        ComponentCode = b.ComponentCode,
                        ComponentName = b.ComponentName,
                        Qty = b.Qty,
                        CreateTime = DateTime.Now,
                    })
                    .ToList();
                await _mesOrderRepository.InsertBomsAsync(boms);
            }

            return new ApiResult { Code = 200, IsSuccess = true, Message = "OK" };
        }

        /// <summary>
        /// 接口二：生产工单更新（修改计划数量或删除工单）
        /// </summary>
        [HttpPost]
        [Route("update")]
        public async Task<ApiResult> Update([FromBody] MesOrderUpdateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.WorkOrderNo))
            {
                return new ApiResult { Code = 500, IsSuccess = false, Message = "工单号不能为空" };
            }

            var exists = await _mesOrderRepository.GetByWorkOrderNoAsync(dto.WorkOrderNo);
            if (exists == null)
            {
                return new ApiResult { Code = 500, IsSuccess = false, Message = "工单不存在" };
            }

            if (dto.Deleted)
            {
                await _mesOrderRepository.DeleteByWorkOrderNoAsync(dto.WorkOrderNo);
            }
            else
            {
                await _mesOrderRepository.UpdatePlanQtyAsync(dto.WorkOrderNo, dto.PlanQty);
            }

            return new ApiResult { Code = 200, IsSuccess = true, Message = "OK" };
        }

        /// <summary>
        /// 查询工单列表（客户端订单展示）
        /// </summary>
        [HttpPost]
        [Route("query")]
        public async Task<ApiResult<System.Collections.Generic.List<Tb_MesOrder>>> Query()
        {
            return ApiResult<System.Collections.Generic.List<Tb_MesOrder>>.Success(
                await _mesOrderRepository.GetListAsync()
            );
        }

        /// <summary>
        /// 订单状态切换（0待执行 / 1执行中）
        /// </summary>
        [HttpPost]
        [Route("setStatus")]
        public async Task<ApiResult> SetStatus([FromBody] MesOrderStatusDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.WorkOrderNo))
            {
                return new ApiResult { Code = 500, IsSuccess = false, Message = "工单号不能为空" };
            }

            if (dto.Status != (int)MesOrderStatusEnum.Executing && dto.Status != (int)MesOrderStatusEnum.Pending)
            {
                return new ApiResult { Code = 500, IsSuccess = false, Message = "状态值无效" };
            }

            var exists = await _mesOrderRepository.GetByWorkOrderNoAsync(dto.WorkOrderNo);
            if (exists == null)
            {
                return new ApiResult { Code = 500, IsSuccess = false, Message = "工单不存在" };
            }

            // 设为执行中时，先把其他执行中的工单改回待执行（同一时间只允许一个执行中）
            if (dto.Status == (int)MesOrderStatusEnum.Executing)
            {
                await _mesOrderRepository.ResetOtherExecutingAsync(dto.WorkOrderNo);
            }

            await _mesOrderRepository.SetStatusAsync(dto.WorkOrderNo, (MesOrderStatusEnum)dto.Status);
            return new ApiResult { Code = 200, IsSuccess = true, Message = "OK" };
        }
    }
}
