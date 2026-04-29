using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace SL.MLineDataPrecisionTracking.Service
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            var service = new OwinHostService();

            // 直接运行 = 控制台模式（F5调试用）
            if (Environment.UserInteractive)
            {
                Console.WriteLine("=== 调试模式启动 ===");
                try
                {
                    service.StartService();
                    while (true)
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("启动异常：" + ex);
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { new OwinHostService() };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
