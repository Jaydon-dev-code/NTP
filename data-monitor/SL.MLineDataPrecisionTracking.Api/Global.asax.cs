using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.Description;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.WebApi;
using SL.MLineDataPrecisionTracking.Api.Services;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;

namespace SL.MLineDataPrecisionTracking.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        // 全局容器（整个程序唯一）
        public static IContainer Container { get; private set; }

        public static int ServerPort { get; private set; }

        protected void Application_Start()
        {
            InitAutofac();
            GlobalConfiguration.Configuration.DependencyResolver =
                new AutofacWebApiDependencyResolver(Container);
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // 初始化并启动数据采集服务管理器
            var dataCollectionServiceManager = Container.Resolve<DataCollectionServiceManager>();

            dataCollectionServiceManager.StartAllAsync();
        }

        /// <summary>
        /// Autofac IOC 容器注册
        /// </summary>
        private void InitAutofac()
        {
            var builder = new ContainerBuilder();
            builder.AddSqlSugerMiddleware();
            builder.AddInfrastructureMiddleware();

            builder.AddLogMiddleware();
            builder.AddCoreMiddleware();
            builder.RegisterType<DataCollectionServiceManager>().SingleInstance();

            // ========== 关键：注册 Web API 控制器 ==========
            builder.RegisterApiControllers(System.Reflection.Assembly.GetExecutingAssembly());

            // 构建容器
            Container = builder.Build();
        }

        /// <summary>
        /// 自动获取并输出所有 Web API 接口
        /// </summary>
        private void PrintAllApiRoutes()
        {
            var config = GlobalConfiguration.Configuration;
            var explorer = config.Services.GetApiExplorer();

            Debug.WriteLine("========== 已开启的 API 接口列表 ==========");

            foreach (ApiDescription api in explorer.ApiDescriptions)
            {
                string method = api.HttpMethod.Method;
                string path = api.RelativePath;

                // 输出格式：GET  api/Test/Get
                Debug.WriteLine($"{method.ToUpper(), -6} {path}");
            }

            Debug.WriteLine("==========================================");
        }
    }
}
