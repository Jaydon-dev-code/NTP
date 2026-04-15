using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Routing;
using SL.MLineDataPrecisionTracking.Core.Middleware;

namespace SL.MLineDataPrecisionTracking.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        // 全局容器（整个程序唯一）
        public static IContainer Container
        {
            get; private set;
        }
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            InitAutofac();
        }
        /// <summary>
        /// Autofac IOC 容器注册
        /// </summary>
        private void InitAutofac()
        {
            var builder = new ContainerBuilder();


            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();

            // 4. 构建容器
            Container = builder.Build();

        }
    }
}

