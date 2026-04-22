using Autofac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Client.Middleware
{
    public static class ViewMiddleware
    {
        public static void AddViewMiddleware(this ContainerBuilder services)
        {
            // 1. 加载 DLL 文件（默认从程序运行目录找）
            string dllPath = Path.Combine(AppContext.BaseDirectory, "SL.MLineDataPrecisionTracking.Client.dll");
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 2. 目标命名空间
            string targetNamespaceView = "SL.MLineDataPrecisionTracking.Client.View";
            string targetNamespaceViewModel = "SL.MLineDataPrecisionTracking.Client.ViewModel";

            // 3. 获取该命名空间下 所有 非抽象、非泛型、非静态类
            var types = assembly.GetTypes()
                .Where(t =>
                    t.IsClass &&                  // 是类
                    !t.IsAbstract &&              // 不是抽象类
                    !t.IsGenericType &&          // 不是泛型类
                    !t.IsInterface &&            // 不是接口
                    t.Namespace.Contains(targetNamespaceView) || t.Namespace.Contains(targetNamespaceViewModel)// 严格匹配命名空间
                ).Where(t => !t.Name.StartsWith("<"))
                .ToList();

            // 4. 批量单例注入（自身 → 自身）
            foreach (var type in types)   
            {
                services.RegisterType(type).InstancePerDependency();
            }

        }
    }
}
