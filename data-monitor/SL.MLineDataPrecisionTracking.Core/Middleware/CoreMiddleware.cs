using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNet.SignalR;
using SL.MLineDataPrecisionTracking.Core.Hubs;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection;
using SL.MLineDataPrecisionTracking.Infrastructure;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Core.Middleware
{
    public static class CoreMiddleware
    {
        public static void AddCoreMiddleware(this ContainerBuilder services)
        {
            services.RegisterType<PlcAddressExcelImportService>().SingleInstance();
            services.RegisterType<EnergyRangeExcelImportService>().SingleInstance();
            services
                .Register(c => GlobalHost.ConnectionManager.GetHubContext<ChatHub>())
                .As<IHubContext>()
                .SingleInstance();
            Assembly assembly = typeof(DataCollectionServiceAbstract).Assembly;
            services
                .RegisterAssemblyTypes(assembly)
                .Where(t =>
                    t.IsClass
                    && !t.IsAbstract
                    && typeof(DataCollectionServiceAbstract).IsAssignableFrom(t)
                )
                .As<DataCollectionServiceAbstract>() // 🔥 全部绑定到同一个接口
                .SingleInstance();
        }
    }
}
