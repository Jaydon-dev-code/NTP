using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using SL.MLineDataPrecisionTracking.Infrastructure.Http;
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace SL.MLineDataPrecisionTracking.Client.Http
{
    public abstract class BaseHttpClient : BaseHttp
    {
        protected override Uri InitHttpUri() =>
            new Uri($"{ConfigurationManager.AppSettings["ServiceUri"]}/api/{_controllerName}/");
    }
}
