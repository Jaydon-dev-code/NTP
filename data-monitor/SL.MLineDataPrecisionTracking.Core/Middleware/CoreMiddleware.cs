using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Infrastructure;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Core.Middleware
{
    public static class CoreMiddleware
    {
        public static void AddCoreMiddleware(this ContainerBuilder services)
        {
            services.RegisterType<PlcAddressExcelImportService>().SingleInstance();

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
