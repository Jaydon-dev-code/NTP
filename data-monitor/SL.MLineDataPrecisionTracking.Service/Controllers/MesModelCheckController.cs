using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Line2;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    /// <summary>
    /// 6分厂2号线型号校验接口（全自动记录：无手动开始/结束，仅提供状态查询供前台补显）
    /// </summary>
    [RoutePrefix("modelCheck")]
    public class MesModelCheckController : ApiController
    {
        private readonly Factory6Line2ModelCheckService _modelCheckService;

        public MesModelCheckController(IEnumerable<DataCollectionServiceAbstract> services)
        {
            _modelCheckService = services.OfType<Factory6Line2ModelCheckService>().FirstOrDefault();
        }

        /// <summary>
        /// 查询当前告警状态（前台启动/重连时调用，补显告警窗）
        /// </summary>
        [HttpPost]
        [Route("status")]
        public ApiResult<MesModelAlertStatusDto> Status()
        {
            if (_modelCheckService == null)
            {
                return ApiResult<MesModelAlertStatusDto>.Fail("型号校验服务未启动");
            }
            return ApiResult<MesModelAlertStatusDto>.Success(_modelCheckService.GetAlertStatus());
        }
    }
}
