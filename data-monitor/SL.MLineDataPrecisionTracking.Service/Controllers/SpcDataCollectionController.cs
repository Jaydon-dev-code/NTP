using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection;
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    /// <summary>
    /// SPC 采集管理控制器 — 管理 SPC 特性采集任务的启停、启用/禁用
    /// </summary>
    public class SpcDataCollectionController : ApiController
    {
        private readonly SPCDataCollectionManager _manager;

        public SpcDataCollectionController(SPCDataCollectionManager manager)
        {
            _manager = manager;
        }

        [HttpPost]
        public ApiResult<List<SPCDataCollectionInfo>> GetAll()
        {
            return ApiResult<List<SPCDataCollectionInfo>>.Success(_manager.GetAll());
        }

        [HttpPost]
        public ApiResult Start([FromBody] int characteristicId)
        {
            try
            {
                return _manager.Start(characteristicId);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        [HttpPost]
        public ApiResult Stop([FromBody] int characteristicId)
        {
            try
            {
                return _manager.Stop(characteristicId);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        [HttpPost]
        public ApiResult SetEnabled([FromBody] SPCEnabledDto dto)
        {
            try
            {
                return _manager.SetEnabled(dto.CharacteristicId, dto.IsEnabled);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        [HttpPost]
        public ApiResult StartAll()
        {
            try
            {
                _manager.StartAll();
                return ApiResult.Success("已启动全部");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        [HttpPost]
        public ApiResult StopAll()
        {
            try
            {
                _manager.StopAll();
                return ApiResult.Success("已停止全部");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ApiResult> Reload()
        {
            try
            {
                await _manager.LoadAsync();
                return ApiResult.Success("已重新加载");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }
    }

    public class SPCEnabledDto
    {
        public int CharacteristicId { get; set; }
        public bool IsEnabled { get; set; }
    }
}
