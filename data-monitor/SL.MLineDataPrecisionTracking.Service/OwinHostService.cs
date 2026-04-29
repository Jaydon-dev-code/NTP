using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin.Hosting;
using Owin;
using Serilog;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Service.Services;

namespace SL.MLineDataPrecisionTracking.Service
{
    public partial class OwinHostService : ServiceBase
    {
        private IDisposable _webApp;

        public OwinHostService()
        {
            InitializeComponent();
        }

        public void StartService()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            _webApp = WebApp.Start<Startup>(url: ConfigurationManager.AppSettings["ServiceUri"]);
            Log.Information("服务已启动。");
        }

        protected override void OnStop()
        {
            _webApp?.Dispose();
            Log.Information("服务已关闭。");
        }
    }
}
