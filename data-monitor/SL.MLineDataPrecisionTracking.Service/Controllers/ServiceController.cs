using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using SL.MLineDataPrecisionTracking.Service.Services;
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    /// <summary>
    /// 服务管理控制器
    /// </summary>
    public class ServiceController : ApiController
    {
        DataCollectionServiceManager _dataCollectionServiceManager;

        public ServiceController(DataCollectionServiceManager dataCollectionServiceManager)
        {
            _dataCollectionServiceManager = dataCollectionServiceManager;
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
    }
}
