using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Autofac.Core;
using Autofac.Integration.WebApi;
using Owin;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Service.Services;

namespace SL.MLineDataPrecisionTracking.Service
{
    public class Startup
    {
        public static Autofac.IContainer Container;

        public void Configuration(IAppBuilder app)
        {
            InitAutofac();

            //// ↓↓↓ 现在这个方法一定能找到了
            app.UseAutofacMiddleware(Container);

            //// WebAPI 设置
            var config = new HttpConfiguration();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(Container);
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{action}/{id}",
                new { id = RouteParameter.Optional }
            );

            app.UseAutofacWebApi(config);
            app.UseWebApi(config);
        }

        private void InitAutofac()
        {
            var builder = new ContainerBuilder();
            builder.AddSqlSugerMiddleware();
            builder.AddInfrastructureMiddleware();
            builder.AddLogMiddleware();
            builder.AddCoreMiddleware();

            // 注册控制器 + 业务类
            builder.RegisterType<DataCollectionServiceManager>().SingleInstance();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<OwinHostService>().As<OwinHostService>();
            Container = builder.Build();
        }
    }
}
