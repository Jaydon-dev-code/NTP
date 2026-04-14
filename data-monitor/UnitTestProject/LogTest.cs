using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using SL.MLineDataPrecisionTracking.Client.Middleware;
using System;

namespace UnitTestProject
{
    [TestClass]
    public class LogTest
    {
        [TestMethod]
        public void TestLog()
        {
            var builder = new ContainerBuilder();
            builder.AddLogMiddleware();
            string userName = "xiaoWang";
            Log.Debug("this debug log");
            Log.Debug("this debug param log {userName}",userName);

            Log.Information("this info  log ");
            Log.Information("this info param log {userName}", userName);

            Log.Warning("this Warning  log ");
            Log.Warning("this debug Warning log {userName}", userName);

            Log.Error("this Error  log");
            try
            {
                throw new Exception("测试异常");
            }
            catch (Exception ex)
            {

                Log.Error("this Error param log {ex}", ex);
            }
        
        }
    }
}
