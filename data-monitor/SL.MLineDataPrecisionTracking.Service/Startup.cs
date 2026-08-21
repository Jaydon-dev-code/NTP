using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Autofac.Core;
using Autofac.Integration.WebApi;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Service.Middleware;
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
            if (WebHelper.IsHaveWebHtml())
            {
                app.UseCorsMiddleware();
                LodeHtml(app);
            }

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

            app.MapSignalR();
        }

        void LodeHtml(IAppBuilder app)
        {
            var fileSystem = new PhysicalFileSystem(@"./StaticHtml");

            // 默认首页
            var defaultOpts = new DefaultFilesOptions { FileSystem = fileSystem };
            defaultOpts.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(defaultOpts);

            // 提供html/css/js
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    FileSystem = fileSystem,
                    ServeUnknownFileTypes = true,
                    // ✅ 关闭浏览器缓存，实现修改html立刻生效
                    //    OnPrepareResponse = ctx =>
                    //    {
                    //        // 全部使用英文半角 "-"
                    //        ctx.OwinContext.Response.Headers.Remove("Cache-Control");
                    //        ctx.OwinContext.Response.Headers.Remove("ETag");
                    //        ctx.OwinContext.Response.Headers.Remove("Last-Modified");

                    //        ctx.OwinContext.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                    //    }
                }
            );
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
