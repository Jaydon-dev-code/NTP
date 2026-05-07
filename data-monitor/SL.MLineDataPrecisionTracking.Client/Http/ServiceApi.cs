using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace SL.MLineDataPrecisionTracking.Client.Http
{
    /// <summary>
    /// 服务管理 API 类
    /// </summary>
    public class ServiceApi : BaseHttp
    {
        protected override string _controllerName => "Service";

        /// <summary>
        /// 获取所有服务状态
        /// </summary>
        /// <returns>服务状态列表</returns>
        public async Task<ApiResult<List<ServiceInfo>>> GetAllServicesAsync()
        {
            try
            {
                return await PostAsync<List<ServiceInfo>>("GetAllServices", null);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"获取服务状态失败: {ex.Message}");
                return ApiResult<List<ServiceInfo>>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 获取单个服务状态
        /// </summary>
        /// <param name="id">服务ID</param>
        /// <returns>服务状态</returns>
        public async Task<ApiResult<ServiceInfo>> GetServiceAsync(string id)
        {
            try
            {
                return await PostAsync<ServiceInfo>("ServiceInfo", new { id });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"获取服务状态失败: {ex.Message}");
                return ApiResult<ServiceInfo>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="id">服务ID</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> StartServiceAsync(string id)
        {
            try
            {
                return await PostAsync("StartService",  id );
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"启动服务失败: {ex.Message}");
                return ApiResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="id">服务ID</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> StopServiceAsync(string id)
        {
            try
            {
                return await PostAsync("StopService", id );
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"停止服务失败: {ex.Message}");
                return ApiResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 重启服务
        /// </summary>
        /// <param name="id">服务ID</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> RestartServiceAsync(string id)
        {
            try
            {
                return await PostAsync("RestartService", id );
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"重启服务失败: {ex.Message}");
                return ApiResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 启用/禁用服务
        /// </summary>
        /// <param name="id">服务ID</param>
        /// <param name="enabled">是否启用</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> SetServiceEnabledAsync(ServiceInfo serviceInfo)
        {
            try
            {
                return await PostAsync("SetServiceEnabled", serviceInfo);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"设置服务状态失败: {ex.Message}");
                return ApiResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 启动所有服务
        /// </summary>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> StartAllServicesAsync()
        {
            try
            {
                return await PostAsync("StartAllServices", null);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"启动所有服务失败: {ex.Message}");
                return ApiResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 停止所有服务
        /// </summary>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> StopAllServicesAsync()
        {
            try
            {
                return await PostAsync("StopAllServices", null);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"停止所有服务失败: {ex.Message}");
                return ApiResult.Fail(ex.Message);
            }
        }
    }


}