using Autofac;
using Microsoft.AspNet.SignalR;
using SL.MLineDataPrecisionTracking.Core.Hubs;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Infrastructure;
using SL.MLineDataPrecisionTracking.Models.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Core.Middleware
{
    public static class CoreMiddleware
    {
        public static void AddCoreMiddleware(this ContainerBuilder services)
        {
            services.RegisterType<PlcAddressExcelImportService>().SingleInstance();
            services
           .Register(c => GlobalHost.ConnectionManager.GetHubContext<ChatHub>())
           .As<IHubContext>()
           .SingleInstance();
            Assembly assembly = typeof(ProLineDataCollectionServiceAbstract).Assembly;
            services
                .RegisterAssemblyTypes(assembly)
                .Where(t =>
                    t.IsClass
                    && !t.IsAbstract
                    && typeof(ProLineDataCollectionServiceAbstract).IsAssignableFrom(t)
                )
                .As<ProLineDataCollectionServiceAbstract>() // 🔥 全部绑定到同一个接口
                .SingleInstance();
        }
    }
}
