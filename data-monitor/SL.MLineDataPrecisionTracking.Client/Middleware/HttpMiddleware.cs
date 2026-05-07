using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using SL.MLineDataPrecisionTracking.Client.Http;

namespace SL.MLineDataPrecisionTracking.Client.Middleware
{
    public static class HttpMiddleware
    {
        public static void AddHttpMiddleware(this ContainerBuilder services)
        {
            // 注册所有 Http 相关的类
            Assembly assembly = Assembly.GetExecutingAssembly();
            var httpTypes = assembly
                .GetTypes()
                .Where(t =>
                    t.IsClass
                    && !t.IsAbstract
                    && !t.IsGenericType
                    && !t.IsInterface
                    && t.Namespace == "SL.MLineDataPrecisionTracking.Client.Http"
                )
                .Where(t => !t.Name.StartsWith("<"))
                .ToList();

            foreach (var type in httpTypes)
            {
                services.RegisterType(type).SingleInstance();
            }
        }
    }
}
