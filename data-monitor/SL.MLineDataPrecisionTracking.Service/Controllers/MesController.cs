using System;
using System.Threading.Tasks;
using System.Web.Http;
using SL.MLineDataPrecisionTracking.Core.Mes;
using SL.MLineDataPrecisionTracking.Models.Dtos.MES;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    /// <summary>
    /// MES 对接控制器 — 上位机与 MES 三个接口统一封装
    /// 路由按接口文档使用根路径（/order/release、/order/update、/process/completion），
    /// 不走默认的 api/{controller}/{action} 约定路由；
    /// 响应统一为文档格式 {"code":200,"message":"OK"}（MesApiResponse）。
    /// </summary>
    public class MesController : ApiController
    {
        private readonly MesOrderService _mesOrderService;

        // MesOrderService 经 Autofac 注入（CoreMiddleware 中注册）
        public MesController(MesOrderService mesOrderService)
        {
            _mesOrderService = mesOrderService;
        }

        /// <summary>
        /// 接口一：生产工单新增（MES → 工控机 POST /order/release）
        /// </summary>
        [HttpPost]
        [Route("~/order/release")]
        public async Task<MesApiResponse> ReleaseOrder([FromBody] MesOrderReleaseRequest request)
        {
            try
            {
                return await _mesOrderService.ReceiveOrderAsync(request);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "MES 工单新增失败");
                return MesApiResponse.Fail($"工单新增失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 接口二：生产工单更新 / 删除（MES → 工控机 POST /order/update）
        /// </summary>
        [HttpPost]
        [Route("~/order/update")]
        public async Task<MesApiResponse> UpdateOrder([FromBody] MesOrderUpdateRequest request)
        {
            try
            {
                return await _mesOrderService.UpdateOrderAsync(request);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "MES 工单更新失败");
                return MesApiResponse.Fail($"工单更新失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 接口三：产出上报（文档方向为工控机 → MES POST /process/completion，此处预留统一入口，业务暂未实现）
        /// </summary>
        [HttpPost]
        [Route("~/process/completion")]
        public async Task<MesApiResponse> ReportCompletion(
            [FromBody] MesProcessCompletionRequest request
        )
        {
            try
            {
                return await _mesOrderService.ReportCompletionAsync(request);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "MES 产出上报失败");
                return MesApiResponse.Fail($"产出上报失败: {ex.Message}");
            }
        }
    }
}
