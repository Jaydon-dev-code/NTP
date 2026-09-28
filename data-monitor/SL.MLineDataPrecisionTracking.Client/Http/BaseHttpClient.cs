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
        protected abstract string _controllerName { get; }

        protected override void Init()
        {
            _client = new HttpClient
            {
                // 这里配置你的后端地址
                BaseAddress = new Uri(
                    $"{ConfigurationManager.AppSettings["ServiceUri"]}/api/{_controllerName}/"
                ),
            };

            // 默认 JSON 请求
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
            );
        }
    }
}
