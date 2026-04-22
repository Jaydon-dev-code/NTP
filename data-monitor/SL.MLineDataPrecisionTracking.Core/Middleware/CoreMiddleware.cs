using Autofac;
using SL.MLineDataPrecisionTracking.Core.Services;
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
            // 1. 加载 DLL 文件（默认从程序运行目录找）
            //string dllPath = Path.Combine(AppContext.BaseDirectory, "SL.MLineDataPrecisionTracking.Core.dll");
            //Assembly assembly = Assembly.LoadFrom(dllPath);

            // 2. 目标命名空间
            //string targetNamespace = "SL.MLineDataPrecisionTracking.Core";

            // 3. 获取该命名空间下 所有 非抽象、非泛型、非静态类
            //var types = assembly.GetTypes()
            //    .Where(t =>
            //        t.IsClass &&                  // 是类
            //        !t.IsAbstract &&              // 不是抽象类
            //        !t.IsGenericType &&          // 不是泛型类
            //        !t.IsInterface &&            // 不是接口
            //        t.Namespace.Contains(targetNamespace) // 严格匹配命名空间
            //    )
            //    .ToList();

            // 4. 批量单例注入（自身 → 自身）
            //foreach (var type in types)
            //{
            //    services.RegisterType(type).SingleInstance();
            //}
            services.RegisterType<PlcAddressExcelImportService>().SingleInstance();
            services.RegisterType<A_ProLineDataCollectionService>().As<ProLineDataCollectionServiceAbstract<Tb_LineA>>().SingleInstance();
            services.RegisterType<B_ProLineDataCollectionService>().As<ProLineDataCollectionServiceAbstract<Tb_LineB>>().SingleInstance();
            services.RegisterType<Rcl_ProLineDataCollectionService>().As<ProLineDataCollectionServiceAbstract<Tb_HeatTreatmentData>>().SingleInstance();

        }
    }
}
