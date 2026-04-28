using Autofac;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace SL.MLineDataPrecisionTracking.Api.Services
{
    /// <summary>
    /// 数据采集服务信息
    /// </summary>
    public class DataCollectionServiceInfo
    {
        /// <summary>
        /// 服务标识
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 服务类型
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>
        /// 服务是否运行中
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// 服务是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 关联的服务实例
        /// </summary>
        public ProLineDataCollectionServiceAbstract ServiceInstance { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// 最后操作人
        /// </summary>
        public string LastOperator { get; set; }

        /// <summary>
        /// 操作备注
        /// </summary>
        public string Remark { get; set; }
    }

    /// <summary>
    /// 数据采集服务管理器 - 作为后台服务运行
    /// </summary>
    public class DataCollectionServiceManager : IRegisteredObject
    {
        private readonly Dictionary<string, DataCollectionServiceInfo> _services =
            new Dictionary<string, DataCollectionServiceInfo>();

        private readonly Tb_ServiceStatusRepository _serviceStatusRepository;
        private readonly object _lock = new object();
        private bool _isStopped = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="container">依赖注入容器</param>
        /// <param name="serviceStatusRepository">服务状态仓储</param>
        public DataCollectionServiceManager(Tb_ServiceStatusRepository serviceStatusRepository)
        {
            _serviceStatusRepository = serviceStatusRepository;
            HostingEnvironment.RegisterObject(this);
            InitializeServices();
        }

        /// <summary>
        /// 初始化所有数据采集服务
        /// </summary>
        private void InitializeServices()
        {
            var allServices = WebApiApplication.Container.Resolve<
                IEnumerable<ProLineDataCollectionServiceAbstract>
            >();
            var serviceStatuses = new List<Tb_ServiceStatus>();

            int index = 0;
            foreach (var service in allServices)
            {
                var serviceType = service.GetType();
                var serviceId = GetServiceId(serviceType.Name);
                var serviceName = GetServiceName(serviceType.Name);

                // 从数据库读取服务状态
                var dbStatus = _serviceStatusRepository.GetByServiceIdAsync(serviceId).Result;
                var isEnabled = dbStatus?.IsEnabled ?? true; // 默认启用

                _services[serviceId] = new DataCollectionServiceInfo
                {
                    ServiceId = serviceId,
                    Name = serviceName,
                    ServiceType = serviceType.Name,
                    IsRunning = false,
                    IsEnabled = isEnabled,
                    ServiceInstance = service,
                    LastUpdateTime = DateTime.Now,
                    LastOperator = dbStatus?.LastOperator ?? "System",
                    Remark = dbStatus?.Remark ?? "服务初始化",
                };

                // 准备初始化数据
                serviceStatuses.Add(
                    new Tb_ServiceStatus
                    {
                        ServiceId = serviceId,
                        ServiceName = serviceName,
                        ServiceType = serviceType.Name,
                        IsEnabled = isEnabled,
                        LastOperationTime = DateTime.Now,
                        LastOperator = dbStatus?.LastOperator ?? "System",
                        Remark = dbStatus?.Remark ?? "服务初始化",
                    }
                );

                index++;
            }

            // 初始化服务状态记录
            _serviceStatusRepository.InitServiceStatusAsync(serviceStatuses).Wait();
        }

        /// <summary>
        /// 根据服务类型名称获取服务ID
        /// </summary>
        private string GetServiceId(string typeName)
        {
            switch (typeName)
            {
                case "A_ProLineDataCollectionService":
                    return "LineA";
                case "B_ProLineDataCollectionService":
                    return "LineB";
                case "Rcl_ProLineDataCollectionService":
                    return "HeatTreatment";
                default:
                    return typeName;
            }
        }

        /// <summary>
        /// 根据服务类型名称获取服务显示名称
        /// </summary>
        private string GetServiceName(string typeName)
        {
            switch (typeName)
            {
                case "A_ProLineDataCollectionService":
                    return "A线数据采集服务";
                case "B_ProLineDataCollectionService":
                    return "B线数据采集服务";
                case "Rcl_ProLineDataCollectionService":
                    return "热处理数据采集服务";
                default:
                    return typeName;
            }
        }

        /// <summary>
        /// 启动所有服务
        /// </summary>
        public async Task StartAllAsync()
        {
            List<string> serviceIdsToStart;

            lock (_lock)
            {
                serviceIdsToStart = _services
                    .Values.Where(s => !s.IsRunning && s.IsEnabled)
                    .Select(s => s.ServiceId)
                    .ToList();
            }

            // 在锁外执行异步操作
            foreach (var serviceId in serviceIdsToStart)
            {
                await StartServiceAsync(serviceId);
            }
        }

        /// <summary>
        /// 停止所有服务
        /// </summary>
        public async Task StopAllAsync()
        {
            List<string> serviceIdsToStop;

            lock (_lock)
            {
                serviceIdsToStop = _services
                    .Values.Where(s => s.IsRunning)
                    .Select(s => s.ServiceId)
                    .ToList();
            }

            // 在锁外执行异步操作
            foreach (var serviceId in serviceIdsToStop)
            {
                await StopServiceAsync(serviceId);
            }
        }

        /// <summary>
        /// 启动指定服务
        /// </summary>
        /// <param name="serviceId">服务ID</param>
        public async Task StartServiceAsync(string serviceId)
        {
            Tb_ServiceStatus serviceStatus = null;

            lock (_lock)
            {
                if (!_services.ContainsKey(serviceId))
                    return;

                var serviceInfo = _services[serviceId];
                if (serviceInfo.IsRunning || !serviceInfo.IsEnabled)
                    return;

                try
                {
                    serviceInfo.ServiceInstance.Start();
                    serviceInfo.IsRunning = true;
                    serviceInfo.LastUpdateTime = DateTime.Now;

                    // 准备数据库记录
                    serviceStatus = new Tb_ServiceStatus
                    {
                        ServiceId = serviceId,
                        ServiceName = serviceInfo.Name,
                        ServiceType = serviceInfo.ServiceType,
                        IsEnabled = serviceInfo.IsEnabled,
                        LastOperationTime = DateTime.Now,
                        LastOperator = "System",
                        Remark = "启动服务",
                    };
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error($"启动服务 {serviceId} 失败: {ex.Message}");
                    return;
                }
            }

            // 在锁外执行异步数据库操作
            if (serviceStatus != null)
            {
                await _serviceStatusRepository.UpdateByServiceIdAsync(serviceStatus);
            }
        }

        /// <summary>
        /// 停止指定服务
        /// </summary>
        /// <param name="servicesId">服务ID</param>
        public async Task StopServiceAsync(string servicesId)
        {
            Tb_ServiceStatus serviceStatus = null;

            lock (_lock)
            {
                if (!_services.ContainsKey(servicesId))
                    return;

                var serviceInfo = _services[servicesId];
                if (!serviceInfo.IsRunning)
                    return;

                try
                {
                    serviceInfo.ServiceInstance.Stop();
                    serviceInfo.IsRunning = false;
                    serviceInfo.LastUpdateTime = DateTime.Now;

                    // 准备数据库记录
                    serviceStatus = new Tb_ServiceStatus
                    {
                        ServiceId = servicesId,
                        ServiceName = serviceInfo.Name,
                        ServiceType = serviceInfo.ServiceType,
                        IsEnabled = serviceInfo.IsEnabled,
                        LastOperationTime = DateTime.Now,
                        LastOperator = "System",
                        Remark = "停止服务",
                    };
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error($"停止服务 {servicesId} 失败: {ex.Message}");
                    return;
                }
            }

            // 在锁外执行异步数据库操作
            if (serviceStatus != null)
            {
                await _serviceStatusRepository.UpdateByServiceIdAsync(serviceStatus);
            }
        }

        /// <summary>
        /// 重启指定服务
        /// </summary>
        /// <param name="servicesId">服务ID</param>
        public async Task RestartServiceAsync(string servicesId)
        {
            Tb_ServiceStatus serviceStatus = null;

            lock (_lock)
            {
                if (!_services.ContainsKey(servicesId))
                    return;

                var serviceInfo = _services[servicesId];
                if (!serviceInfo.IsEnabled)
                    return;

                try
                {
                    if (serviceInfo.IsRunning)
                    {
                        serviceInfo.ServiceInstance.Stop();
                        serviceInfo.IsRunning = false;
                        Thread.Sleep(1000);
                    }
                    serviceInfo.ServiceInstance.Start();
                    serviceInfo.IsRunning = true;
                    serviceInfo.LastUpdateTime = DateTime.Now;

                    // 准备数据库记录
                    serviceStatus = new Tb_ServiceStatus
                    {
                        ServiceId = servicesId,
                        ServiceName = serviceInfo.Name,
                        ServiceType = serviceInfo.ServiceType,
                        IsEnabled = serviceInfo.IsEnabled,
                        LastOperationTime = DateTime.Now,
                        LastOperator = "System",
                        Remark = "重启服务",
                    };
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error($"重启服务 {servicesId} 失败: {ex.Message}");
                    return;
                }
            }

            // 在锁外执行异步数据库操作
            if (serviceStatus != null)
            {
                await _serviceStatusRepository.UpdateByServiceIdAsync(serviceStatus);
            }
        }

        /// <summary>
        /// 启用/禁用服务
        /// </summary>
        /// <param name="servicesId">服务ID</param>
        /// <param name="enabled">是否启用</param>
        public async Task SetServiceEnabledAsync(string servicesId, bool enabled)
        {
            Tb_ServiceStatus serviceStatus = null;
            bool shouldStopService = false;

            lock (_lock)
            {
                if (!_services.ContainsKey(servicesId))
                    return;

                var serviceInfo = _services[servicesId];
                serviceInfo.IsEnabled = enabled;
                serviceInfo.LastUpdateTime = DateTime.Now;

                // 准备数据库记录
                serviceStatus = new Tb_ServiceStatus
                {
                    ServiceId = servicesId,
                    ServiceName = serviceInfo.Name,
                    ServiceType = serviceInfo.ServiceType,
                    IsEnabled = enabled,
                    LastOperationTime = DateTime.Now,
                    LastOperator = "System",
                    Remark = enabled ? "启用服务" : "禁用服务",
                };

                // 如果禁用服务，标记需要停止
                if (!enabled && serviceInfo.IsRunning)
                {
                    shouldStopService = true;
                }
            }

            // 在锁外执行异步数据库操作
            if (serviceStatus != null)
            {
                await _serviceStatusRepository.UpdateByServiceIdAsync(serviceStatus);
            }

            // 如果需要停止服务，在锁外调用（StopServiceAsync 有自己的锁）
            if (shouldStopService)
            {
                await StopServiceAsync(servicesId);
            }
        }

        /// <summary>
        /// 获取所有服务信息
        /// </summary>
        /// <returns>服务信息列表</returns>
        public async Task<List<DataCollectionServiceInfo>> GetAllServicesAsync()
        {
            // 从数据库同步状态`
            var dbStatuses = await _serviceStatusRepository.GetAllServiceStatusAsync();
            var statusMap = dbStatuses.ToDictionary(x => x.ServiceId);

            // 同步状态
            foreach (var service in _services.Values)
            {
                if (statusMap.TryGetValue(service.ServiceId, out var dbStatus))
                {
                    service.IsEnabled = dbStatus.IsEnabled;
                    service.LastOperator = dbStatus.LastOperator;
                    service.Remark = dbStatus.Remark;
                    service.LastUpdateTime = dbStatus.LastOperationTime;
                }
                service.IsRunning =
                    service.ServiceInstance.Status == Core.Services.ServiceStatus.Running;
            }
            return _services.Values.ToList();
        }

        /// <summary>
        /// 获取指定服务信息
        /// </summary>
        /// <param name="servicesId">服务ID</param>
        /// <returns>服务信息</returns>
        public async Task<DataCollectionServiceInfo> GetServiceAsync(string servicesId)
        {
            DataCollectionServiceInfo service = null;

            lock (_lock)
            {
                if (_services.TryGetValue(servicesId, out var foundService))
                {
                    service = foundService;
                    service.IsRunning =
                        service.ServiceInstance.Status == Core.Services.ServiceStatus.Running;
                }
            }

            if (service != null)
            {
                // 在锁外执行异步数据库操作
                var dbStatus = await _serviceStatusRepository.GetByServiceIdAsync(servicesId);
                if (dbStatus != null)
                {
                    service.IsEnabled = dbStatus.IsEnabled;
                    service.LastOperator = dbStatus.LastOperator;
                    service.Remark = dbStatus.Remark;
                    service.LastUpdateTime = dbStatus.LastOperationTime;
                }
            }

            return service;
        }

        /// <summary>
        /// 实现 IRegisteredObject 接口 - 停止所有服务
        /// </summary>
        public void Stop(bool immediate)
        {
            lock (_lock)
            {
                _isStopped = true;
                StopAllAsync().Wait();
                HostingEnvironment.UnregisterObject(this);
            }
        }
    }
}
