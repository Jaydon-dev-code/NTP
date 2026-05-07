using Autofac;
using SL.MLineDataPrecisionTracking.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Core.Middleware
{
    public static class InfrastructureMiddleware
    {
        public static void AddInfrastructureMiddleware(this ContainerBuilder services)
        {
            Assembly assembly = typeof(InfrastructureAssemblyMarker).Assembly;

            // 2. 目标命名空间
            string targetNamespace = "SL.MLineDataPrecisionTracking.Infrastructure";

            // 3. 获取该命名空间下 所有 非抽象、非泛型、非静态类
            var types = assembly.GetTypes()
                .Where(t =>
                    t.IsClass &&                  // 是类
                    !t.IsAbstract &&              // 不是抽象类
                    !t.IsGenericType &&          // 不是泛型类
                    !t.IsInterface &&            // 不是接口
                    t.Namespace.Contains(targetNamespace) // 严格匹配命名空间
                )
                .ToList();

            // 4. 批量单例注入（自身 → 自身）
            foreach (var type in types)
            {
                services.RegisterType(type).SingleInstance();
            }

        }
    }
}
