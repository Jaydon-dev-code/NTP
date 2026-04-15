using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Serilog;

namespace SL.MLineDataPrecisionTracking.Client.Middleware
{
    public static class LogMiddleware
    {
        public static void AddLogMiddleware(this ContainerBuilder services)
        {
#if DEBUG
            var minLogEventLevel = Serilog.Events.LogEventLevel.Debug;
#else
            var minLogEventLevel = Serilog.Events.LogEventLevel.Information;
#endif

            Log.Logger = new LoggerConfiguration()
                // 核心：Debug模式才开启Debug级别，否则只记录Info及以上
                .MinimumLevel.Is(minLogEventLevel)
                // 彻底屏蔽所有系统日志
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Fatal)
                .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Fatal)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: Path.Combine(AppContext.BaseDirectory, "Logs/log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31,
                    flushToDiskInterval: TimeSpan.FromSeconds(1),
                    encoding: System.Text.Encoding.UTF8
                )
                .CreateLogger();
        }
    }
}
