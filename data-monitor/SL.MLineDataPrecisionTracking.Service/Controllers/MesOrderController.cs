using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using NPOI.OpenXmlFormats.Spreadsheet;
using SL.MLineDataPrecisionTracking.Core.Services.Mes;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;
using SL.MLineDataPrecisionTracking.Models.Entities.Mes;
using SL.MLineDataPrecisionTracking.Models.Enum;

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
        private readonly MesOrderOP _mesOrderOP;

        public MesOrderController(MesOrderOP mesOrderOP)
        {
            _mesOrderOP = mesOrderOP;
        }

        /// <summary>
        /// 接口一：生产工单新增
        /// </summary>
        [HttpPost]
        [Route("release")]
        public async Task<ApiResult> Release([FromBody] MesOrderReleaseDto dto)
        {
            return await _mesOrderOP.Release(dto);
        }

        /// <summary>
        /// 接口二：生产工单更新（修改计划数量或删除工单）
        /// </summary>
        [HttpPost]
        [Route("update")]
        public async Task<ApiResult> Update([FromBody] MesOrderUpdateDto dto)
        {
            return await _mesOrderOP.Update(dto);
        }

        /// <summary>
        /// 查询工单列表（客户端订单展示）
        /// </summary>
        [HttpPost]
        [Route("query")]
        public async Task<ApiResult<System.Collections.Generic.List<Tb_MesOrder>>> Query()
        {
            return await _mesOrderOP.Query();
        }

        /// <summary>
        /// 订单状态切换（0待执行 / 1执行中）
        /// </summary>
        [HttpPost]
        [Route("setStatus")]
        public async Task<ApiResult> SetStatus([FromBody] MesOrderStatusDto dto)
        {
            return await _mesOrderOP.SetStatus(dto);
        }
    }
}
