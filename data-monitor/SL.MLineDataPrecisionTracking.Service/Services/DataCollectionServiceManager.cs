using Autofac;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Service.Services
{
    public class DataCollectionServiceInfo
    {
        public string ServiceId { get; set; }
        public string Name { get; set; }
        public string ServiceType { get; set; }
        public bool IsRunning { get; set; }
        public bool IsEnabled { get; set; }
        public ProLineDataCollectionServiceAbstract ServiceInstance { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string LastOperator { get; set; }
        public string Remark { get; set; }
    }

    public class DataCollectionServiceManager
    {
        private readonly Dictionary<string, DataCollectionServiceInfo> _services =
            new Dictionary<string, DataCollectionServiceInfo>();

        private readonly Tb_ServiceStatusRepository _serviceStatusRepository;
        private readonly object _lock = new object();
        private bool _isStopped = false;

        public DataCollectionServiceManager(Tb_ServiceStatusRepository serviceStatusRepository)
        {
            _serviceStatusRepository = serviceStatusRepository;
            InitializeServices();
        }

        private void InitializeServices()
        {
            var allServices = Startup.Container.Resolve<
                IEnumerable<ProLineDataCollectionServiceAbstract>
            >();
            var serviceStatuses = new List<Tb_ServiceStatus>();

            foreach (var service in allServices)
            {
                var serviceType = service.GetType();
                var serviceId = GetServiceId(serviceType.Name);
                var serviceName = GetServiceName(serviceType.Name);

                var dbStatus = _serviceStatusRepository.GetByServiceIdAsync(serviceId).Result;
                var isEnabled = dbStatus?.IsEnabled ?? true;

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
            }

            _serviceStatusRepository.InitServiceStatusAsync(serviceStatuses);
        }

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

            foreach (var serviceId in serviceIdsToStart)
            {
                await StartServiceAsync(serviceId);
            }
        }

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

            foreach (var serviceId in serviceIdsToStop)
            {
                await StopServiceAsync(serviceId);
            }
        }

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

            if (serviceStatus != null)
            {
                await _serviceStatusRepository.UpdateAsync(serviceStatus);
            }
        }

        public async Task StopServiceAsync(string serviceId)
        {
            Tb_ServiceStatus serviceStatus = null;

            lock (_lock)
            {
                if (!_services.ContainsKey(serviceId))
                    return;

                var serviceInfo = _services[serviceId];
                if (!serviceInfo.IsRunning)
                    return;

                try
                {
                    serviceInfo.ServiceInstance.Stop();
                    serviceInfo.IsRunning = false;
                    serviceInfo.LastUpdateTime = DateTime.Now;

                    serviceStatus = new Tb_ServiceStatus
                    {
                        ServiceId = serviceId,
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
                    Serilog.Log.Error($"停止服务 {serviceId} 失败: {ex.Message}");
                    return;
                }
            }

            if (serviceStatus != null)
            {
                await _serviceStatusRepository.UpdateAsync(serviceStatus);
            }
        }

        public async Task RestartServiceAsync(string serviceId)
        {
            Tb_ServiceStatus serviceStatus = null;

            lock (_lock)
            {
                if (!_services.ContainsKey(serviceId))
                    return;

                var serviceInfo = _services[serviceId];
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

                    serviceStatus = new Tb_ServiceStatus
                    {
                        ServiceId = serviceId,
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
                    Serilog.Log.Error($"重启服务 {serviceId} 失败: {ex.Message}");
                    return;
                }
            }

            if (serviceStatus != null)
            {
                await _serviceStatusRepository.UpdateAsync(serviceStatus);
            }
        }

        public async Task SetServiceEnabledAsync(string serviceId, bool enabled)
        {
            Tb_ServiceStatus serviceStatus = null;
            bool shouldStopService = false;

            lock (_lock)
            {
                if (!_services.ContainsKey(serviceId))
                    return;

                var serviceInfo = _services[serviceId];
                serviceInfo.IsEnabled = enabled;
                serviceInfo.LastUpdateTime = DateTime.Now;

                serviceStatus = new Tb_ServiceStatus
                {
                    ServiceId = serviceId,
                    ServiceName = serviceInfo.Name,
                    ServiceType = serviceInfo.ServiceType,
                    IsEnabled = enabled,
                    LastOperationTime = DateTime.Now,
                    LastOperator = "System",
                    Remark = enabled ? "启用服务" : "禁用服务",
                };

                if (!enabled && serviceInfo.IsRunning)
                {
                    shouldStopService = true;
                }
            }

            if (serviceStatus != null)
            {
                await _serviceStatusRepository.UpdateAsync(serviceStatus);
            }

            if (shouldStopService)
            {
                await StopServiceAsync(serviceId);
            }
        }

        public async Task<List<DataCollectionServiceInfo>> GetAllServicesAsync()
        {
            var dbStatuses = await _serviceStatusRepository.GetAllServiceStatusAsync();
            var statusMap = dbStatuses.ToDictionary(x => x.ServiceId);

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

        public async Task<DataCollectionServiceInfo> GetServiceAsync(string serviceId)
        {
            DataCollectionServiceInfo service = null;

            lock (_lock)
            {
                if (_services.TryGetValue(serviceId, out var foundService))
                {
                    service = foundService;
                    service.IsRunning =
                        service.ServiceInstance.Status == Core.Services.ServiceStatus.Running;
                }
            }

            if (service != null)
            {
                var dbStatus = await _serviceStatusRepository.GetByServiceIdAsync(serviceId);
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

        public void Stop()
        {
            lock (_lock)
            {
                _isStopped = true;
                StopAllAsync().Wait();
            }
        }
    }
}