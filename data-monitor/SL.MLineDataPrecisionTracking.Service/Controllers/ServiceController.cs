using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection;
using SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection;
using SL.MLineDataPrecisionTracking.Service.Services;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    /// <summary>
    /// 服务管理控制器
    /// </summary>
    public class ServiceController : ApiController
    {
        DataCollectionServiceManager _dataCollectionServiceManager;
        StationCollectionManager _stationCollectionManager;
        SPCDataCollectionManager _spcDataCollectionManager;
        CurrentModelSyncService _currentModelSyncService;

        const string StationManagerId = "StationCollectionManager";
        const string SpcManagerId = "SPCDataCollectionManager";
        const string ModelSyncId = "CurrentModelSyncService";

        public ServiceController(
            DataCollectionServiceManager dataCollectionServiceManager,
            StationCollectionManager stationCollectionManager,
            SPCDataCollectionManager spcDataCollectionManager,
            CurrentModelSyncService currentModelSyncService)
        {
            _dataCollectionServiceManager = dataCollectionServiceManager;
            _stationCollectionManager = stationCollectionManager;
            _spcDataCollectionManager = spcDataCollectionManager;
            _currentModelSyncService = currentModelSyncService;
        }

        private string ModelSyncRemark()
        {
            if (!ThirdPartyDbConfig.Enabled)
                return "三方开关关闭(ThirdPartyDb_Enabled=false)";
            if (!ThirdPartyDbConfig.IsConfigured)
                return "未配置连接串/型号SQL";
            if (!_currentModelSyncService.IsEnabled)
                return "服务已禁用";
            return $"同步间隔 {ThirdPartyDbConfig.SyncIntervalSeconds}s · SPC 附属";
        }

        /// <summary>
        /// 获取所有服务状态
        /// </summary>
        /// <returns>服务状态列表</returns>
        [HttpPost]
        public async Task<ApiResult<List<DataCollectionServiceInfo>>> GetAllServices()
        {
            return ApiResult<List<DataCollectionServiceInfo>>.Success(
                await _dataCollectionServiceManager.GetAllServicesAsync()
            );
        }

        /// <summary>
        /// 获取单个服务状态
        /// </summary>
        /// <param name="id">服务ID</param>
        /// <returns>服务状态</returns>
        [HttpPost]
        public async Task<ApiResult<DataCollectionServiceInfo>> ServiceInfo([FromBody] string id)
        {
            return ApiResult<DataCollectionServiceInfo>.Success(
                await _dataCollectionServiceManager.GetServiceAsync(id)
            );
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="id">服务ID</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<ApiResult> StartService([FromBody] string id)
        {
            // 工位采集调度服务特殊处理
            if (id == StationManagerId)
            {
                if (!_stationCollectionManager.IsEnabled)
                    return ApiResult.Fail("服务未启用");
                if (_stationCollectionManager.WorkersRunning)
                    return ApiResult.Fail("服务已经在运行中");
                await _stationCollectionManager.RestoreAsync();
                return ApiResult.Success("工位采集调度服务已启动");
            }

            // SPC 采集管理服务特殊处理
            if (id == SpcManagerId)
            {
                if (!_spcDataCollectionManager.IsEnabled)
                    return ApiResult.Fail("服务未启用");
                if (_spcDataCollectionManager.WorkersRunning)
                    return ApiResult.Fail("服务已经在运行中");
                await _spcDataCollectionManager.LoadAsync();
                return ApiResult.Success("SPC 采集管理服务已启动");
            }

            // SPC 型号同步服务（隶属 SPC，可独立启动）
            if (id == ModelSyncId)
            {
                if (!_currentModelSyncService.IsEnabled)
                    return ApiResult.Fail("服务未启用");
                if (_currentModelSyncService.IsRunning)
                    return ApiResult.Fail("服务已经在运行中");
                _currentModelSyncService.Start();
                if (!_currentModelSyncService.IsRunning)
                    return ApiResult.Fail(ModelSyncRemark());
                return ApiResult.Success("SPC 型号同步服务已启动");
            }

            var service = await _dataCollectionServiceManager.GetServiceAsync(id);
            if (service == null)
            {
                return ApiResult.Fail("未查询到需要启动的服务");
            }

            if (service.IsRunning)
            {
                return ApiResult.Fail("服务已经在运行中");
            }

            if (!service.IsEnabled)
            {
                return ApiResult.Fail("服务未启用");
            }

            try
            {
                await _dataCollectionServiceManager.StartServiceAsync(id);
                return ApiResult.Success($"服务 {service.Name} 已启动");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"服务启用出现错误。\r\n{ex.Message}");
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="id">服务ID</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<ApiResult> StopService([FromBody] string id)
        {
            // 工位采集调度服务特殊处理
            if (id == StationManagerId)
            {
                if (!_stationCollectionManager.WorkersRunning)
                    return ApiResult.Fail("服务已经停止");
                _stationCollectionManager.StopWorkers();
                return ApiResult.Success("工位采集调度服务已停止");
            }

            // SPC 采集管理服务特殊处理
            if (id == SpcManagerId)
            {
                if (!_spcDataCollectionManager.WorkersRunning)
                    return ApiResult.Fail("服务已经停止");
                _spcDataCollectionManager.StopWorkers();
                return ApiResult.Success("SPC 采集管理服务已停止");
            }

            // SPC 型号同步服务
            if (id == ModelSyncId)
            {
                if (!_currentModelSyncService.IsRunning)
                    return ApiResult.Fail("服务已经停止");
                _currentModelSyncService.Stop();
                return ApiResult.Success("SPC 型号同步服务已停止");
            }

            var service = await _dataCollectionServiceManager.GetServiceAsync(id);
            if (service == null)
            {
                return ApiResult.Fail("未查询到需要启动的服务");
            }

            if (!service.IsRunning)
            {
                return ApiResult.Fail("服务已经停止");
            }

            try
            {
                await _dataCollectionServiceManager.StopServiceAsync(id);
                return ApiResult.Success($"服务{service.Name}已停止");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"服务停止出现错误。\r\n{ex.Message}");
            }
        }

        /// <summary>
        /// 重启服务
        /// </summary>
        /// <param name="id">服务ID</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<ApiResult> RestartService([FromBody] string id)
        {
            // 工位采集调度服务特殊处理
            if (id == StationManagerId)
            {
                _stationCollectionManager.StopWorkers();
                await _stationCollectionManager.RestoreAsync();
                return ApiResult.Success("工位采集调度服务已重启");
            }

            // SPC 采集管理服务特殊处理
            if (id == SpcManagerId)
            {
                _spcDataCollectionManager.StopWorkers();
                await _spcDataCollectionManager.LoadAsync();
                return ApiResult.Success("SPC 采集管理服务已重启");
            }

            // SPC 型号同步服务
            if (id == ModelSyncId)
            {
                _currentModelSyncService.Stop();
                _currentModelSyncService.Start();
                if (!_currentModelSyncService.IsRunning)
                    return ApiResult.Fail(ModelSyncRemark());
                return ApiResult.Success("SPC 型号同步服务已重启");
            }

            var service = await _dataCollectionServiceManager.GetServiceAsync(id);
            if (service == null)
            {
                return ApiResult.Fail("未查询到需要重启的服务");
            }

            if (!service.IsEnabled)
            {
                return ApiResult.Fail("服务未启用");
            }

            try
            {
                await _dataCollectionServiceManager.RestartServiceAsync(id);
                return ApiResult.Success($"服务 {service.Name} 已重启");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"服务重启出现错误。\r\n{ex.Message}");
            }
        }

        /// <summary>
        /// 启用/禁用服务
        /// </summary>
        /// <param name="id">服务ID</param>
        /// <param name="enabled">是否启用</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<ApiResult> SetServiceEnabled([FromBody] ServiceInfo serviceInfo)
        {
            // 工位采集调度服务特殊处理
            if (serviceInfo.ServiceId == StationManagerId)
            {
                _stationCollectionManager.IsEnabled = serviceInfo.IsEnabled;
                if (!serviceInfo.IsEnabled && _stationCollectionManager.WorkersRunning)
                {
                    _stationCollectionManager.StopWorkers();
                }
                return ApiResult.Success(
                    $"工位采集调度服务已{(serviceInfo.IsEnabled ? "启用" : "禁用")}"
                );
            }

            // SPC 采集管理服务特殊处理
            if (serviceInfo.ServiceId == SpcManagerId)
            {
                _spcDataCollectionManager.IsEnabled = serviceInfo.IsEnabled;
                if (!serviceInfo.IsEnabled && _spcDataCollectionManager.WorkersRunning)
                {
                    _spcDataCollectionManager.StopWorkers();
                }
                return ApiResult.Success(
                    $"SPC 采集管理服务已{(serviceInfo.IsEnabled ? "启用" : "禁用")}"
                );
            }

            // SPC 型号同步服务
            if (serviceInfo.ServiceId == ModelSyncId)
            {
                _currentModelSyncService.IsEnabled = serviceInfo.IsEnabled;
                if (!serviceInfo.IsEnabled && _currentModelSyncService.IsRunning)
                {
                    _currentModelSyncService.Stop();
                }
                return ApiResult.Success(
                    $"SPC 型号同步服务已{(serviceInfo.IsEnabled ? "启用" : "禁用")}"
                );
            }

            var service = await _dataCollectionServiceManager.GetServiceAsync(serviceInfo.ServiceId);
            if (service == null)
            {
                return ApiResult.Fail("未查询到需要重启的服务");
            }

            try
            {
                await _dataCollectionServiceManager.SetServiceEnabledAsync(
                    serviceInfo.ServiceId,
                    serviceInfo.IsEnabled
                );
                return ApiResult.Success(
                    $"服务 {service.Name}已{(serviceInfo.IsEnabled ? "启用" : "禁用")}"
                );
            }
            catch (Exception ex)
            {
                return ApiResult.Success(
                    $"服务 {service.Name}{(serviceInfo.IsEnabled ? "启用" : "禁用")}异常。\r\n{ex.Message}"
                );
            }
        }

        /// <summary>
        /// 启动所有服务
        /// </summary>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<ApiResult> StartAllServices()
        {
            try
            {
                await _dataCollectionServiceManager.StartAllAsync();
                return ApiResult.Success($"所有服务已启动");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"所有服务已启动出现错误。\r\n{ex.Message}");
            }
        }

        /// <summary>
        /// 停止所有服务
        /// </summary>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<ApiResult> StopAllServices()
        {
            try
            {
                await _dataCollectionServiceManager.StopAllAsync();
                return ApiResult.Success($"所有服务已停止");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"所有服务已停止出现错误。\r\n{ex.Message}");
            }
        }

        #region Web 端服务管理

        /// <summary>
        /// 获取所有 Web 端服务状态（含工位调度、SPC 采集、型号同步）
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<List<DataCollectionServiceInfo>>> GetWebServices()
        {
            var list = await _dataCollectionServiceManager.GetAllServicesByCategoryAsync(ServiceCategory.Web);

            list.Add(new DataCollectionServiceInfo
            {
                ServiceId = StationManagerId,
                Name = "工位采集调度服务",
                ServiceType = nameof(StationCollectionManager),
                Category = ServiceCategory.Web,
                IsRunning = _stationCollectionManager.WorkersRunning,
                IsEnabled = _stationCollectionManager.IsEnabled,
                LastUpdateTime = DateTime.Now,
                LastOperator = "System",
                Remark = $"Worker数: {_stationCollectionManager.WorkerCount}",
            });

            list.Add(new DataCollectionServiceInfo
            {
                ServiceId = SpcManagerId,
                Name = "SPC 采集管理服务",
                ServiceType = nameof(SPCDataCollectionManager),
                Category = ServiceCategory.Web,
                IsRunning = _spcDataCollectionManager.WorkersRunning,
                IsEnabled = _spcDataCollectionManager.IsEnabled,
                LastUpdateTime = DateTime.Now,
                LastOperator = "System",
                Remark = $"Worker数: {_spcDataCollectionManager.WorkerCount}",
            });

            // 型号同步：隶属 SPC 采集服务组，可独立启停
            list.Add(new DataCollectionServiceInfo
            {
                ServiceId = ModelSyncId,
                Name = "SPC 型号同步服务",
                ServiceType = nameof(CurrentModelSyncService),
                Category = ServiceCategory.Web,
                IsRunning = _currentModelSyncService.IsRunning,
                IsEnabled = _currentModelSyncService.IsEnabled,
                LastUpdateTime = DateTime.Now,
                LastOperator = "System",
                Remark = ModelSyncRemark(),
            });

            return ApiResult<List<DataCollectionServiceInfo>>.Success(list);
        }

        /// <summary>
        /// 启动所有 Web 端服务（含工位 / SPC / 型号同步）
        /// </summary>
        [HttpPost]
        public async Task<ApiResult> StartAllWebServices()
        {
            try
            {
                await _dataCollectionServiceManager.StartAllByCategoryAsync(ServiceCategory.Web);

                if (_stationCollectionManager.IsEnabled && !_stationCollectionManager.WorkersRunning)
                    await _stationCollectionManager.RestoreAsync();

                if (_spcDataCollectionManager.IsEnabled && !_spcDataCollectionManager.WorkersRunning)
                    await _spcDataCollectionManager.LoadAsync();

                if (_currentModelSyncService.IsEnabled && !_currentModelSyncService.IsRunning)
                    _currentModelSyncService.Start();

                return ApiResult.Success("所有 Web 端服务已启动");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"启动 Web 端服务失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 停止所有 Web 端服务（含工位 / SPC / 型号同步）
        /// </summary>
        [HttpPost]
        public async Task<ApiResult> StopAllWebServices()
        {
            try
            {
                await _dataCollectionServiceManager.StopAllByCategoryAsync(ServiceCategory.Web);

                if (_spcDataCollectionManager.WorkersRunning)
                    _spcDataCollectionManager.StopWorkers();

                if (_currentModelSyncService.IsRunning)
                    _currentModelSyncService.Stop();

                if (_stationCollectionManager.WorkersRunning)
                    _stationCollectionManager.StopWorkers();

                return ApiResult.Success("所有 Web 端服务已停止");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"停止 Web 端服务失败: {ex.Message}");
            }
        }

        #endregion
    }
}
