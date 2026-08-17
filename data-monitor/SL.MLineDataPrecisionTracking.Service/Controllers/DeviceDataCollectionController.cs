using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    /// <summary>
    /// 设备采集管理控制器 — 工位采集服务注册/注销、运行控制、状态查询、心跳
    /// </summary>
    public class DeviceDataCollectionController : ApiController
    {
        private readonly StationCollectionManager _collectionManager;

        public DeviceDataCollectionController(StationCollectionManager collectionManager)
        {
            _collectionManager = collectionManager;
        }

        #region 注册 / 注销

        /// <summary>
        /// 注册一台设备采集服务（可传 MQTT 配置，为空时取产线/厂配置）
        /// </summary>
        [HttpPost]
        public async Task<ApiResult> RegisterDevice([FromBody] DeviceRegisterDto dto)
        {
            try
            {
                return await _collectionManager.RegisterDeviceAsync(dto.EquipmentId, dto.MqttConfig);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"注册设备采集失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 按产线批量注册
        /// </summary>
        [HttpPost]
        public async Task<ApiResult> RegisterLine([FromBody] int lineId)
        {
            try
            {
                return await _collectionManager.RegisterLineAsync(lineId);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"按产线注册失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 按厂批量注册
        /// </summary>
        [HttpPost]
        public async Task<ApiResult> RegisterFactory([FromBody] int factoryId)
        {
            try
            {
                return await _collectionManager.RegisterFactoryAsync(factoryId);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"按厂注册失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> UnregisterDevice([FromBody] string equipmentId)
        {
            try
            {
                return await _collectionManager.UnregisterDeviceAsync(equipmentId);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"注销设备采集失败: {ex.Message}");
            }
        }

        #endregion

        #region 运行控制

        [HttpPost]
        public async Task<ApiResult> Start([FromBody] string equipmentId)
        {
            try
            {
                return await _collectionManager.StartAsync(equipmentId);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"启动采集失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> Stop([FromBody] string equipmentId)
        {
            try
            {
                return await _collectionManager.StopAsync(equipmentId);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"停止采集失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> Restart([FromBody] string equipmentId)
        {
            try
            {
                return await _collectionManager.RestartAsync(equipmentId);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"重启采集失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> SetEnabled([FromBody] DeviceEnabledDto dto)
        {
            try
            {
                return await _collectionManager.SetEnabledAsync(dto.EquipmentId, dto.IsEnabled);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"设置启用失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> StartAll()
        {
            try
            {
                await _collectionManager.StartAllAsync();
                return ApiResult.Success("全部采集已启动");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"启动全部采集失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> StopAll()
        {
            try
            {
                await _collectionManager.StopAllAsync();
                return ApiResult.Success("全部采集已停止");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"停止全部采集失败: {ex.Message}");
            }
        }

        #endregion

        #region 查询

        [HttpPost]
        public async Task<ApiResult<List<StationCollectionInfo>>>
            GetAllRegistered()
        {
            try
            {
                return await _collectionManager.GetAllRegisteredAsync();
            }
            catch (Exception ex)
            {
                return ApiResult<List<StationCollectionInfo>>.Fail(
                    $"查询已注册设备失败: {ex.Message}"
                );
            }
        }

        [HttpPost]
        public async Task<ApiResult<StationCollectionInfo>>
            GetRegistered([FromBody] string equipmentId)
        {
            try
            {
                return await _collectionManager.GetRegisteredAsync(equipmentId);
            }
            catch (Exception ex)
            {
                return ApiResult<StationCollectionInfo>.Fail(
                    $"查询设备采集失败: {ex.Message}"
                );
            }
        }

        [HttpPost]
        public async Task<ApiResult<Dictionary<string, object>>> GetDeviceValues(
            [FromBody] string equipmentId
        )
        {
            try
            {
                return await _collectionManager.GetDeviceValuesAsync(equipmentId);
            }
            catch (Exception ex)
            {
                return ApiResult<Dictionary<string, object>>.Fail($"查询设备点位失败: {ex.Message}");
            }
        }

        #endregion

        #region 心跳（上线/离线信号）

       
        #endregion
    }
}

