using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

namespace SL.MLineDataPrecisionTracking.Service.Middleware
{
    /// <summary>
    /// CORS 跨域中间件 — 允许前端（Vue 开发服务器等跨域来源）访问 WebApi。
    /// </summary>
    public class CorsMiddleware : OwinMiddleware
    {
        public CorsMiddleware(OwinMiddleware next)
            : base(next) { }

        public override async Task Invoke(IOwinContext context)
        {
            var headers = context.Response.Headers;
            headers.Append("Access-Control-Allow-Origin", "*");
            headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");
            headers.Append("Access-Control-Max-Age", "3600");

            // 预检请求直接返回，不进入后续管道
            if (string.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 200;
                return;
            }

            await Next.Invoke(context);
        }
    }

    public static class CorsMiddlewareExtensions
    {
        public static IAppBuilder UseCorsMiddleware(this IAppBuilder app)
        {
            return app.Use<CorsMiddleware>();
        }
    }
}
