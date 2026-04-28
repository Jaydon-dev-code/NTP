using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using Serilog;
using SL.MLineDataPrecisionTracking.Models.Domain;
using static System.Net.WebRequestMethods;

namespace SL.MLineDataPrecisionTracking.Client.Http
{
    public abstract class BaseHttp
    {
        protected readonly HttpClient _client;

        protected abstract string _controllerName { get; }
        public BaseHttp()
        {
            _client = new HttpClient
            {
                // 这里配置你的后端地址
                BaseAddress = new Uri($"{ConfigurationManager.AppSettings["ServiceUri"]}/api/{_controllerName}/"),
            };

            // 默认 JSON 请求
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        #region GET 请求
        //protected async Task<T> GetAsync<T>(string url)
        //{
        //    try
        //    {
        //        var response = await _client.GetAsync(url);
        //        response.EnsureSuccessStatusCode();
        //        var json = await response.Content.ReadAsStringAsync();
        //        return JsonConvert.DeserializeObject<T>(json);
        //    }
        //    catch (Exception ex)
        //    {
        //        // 你可以加日志
        //        Console.WriteLine($"GET 异常：{ex.Message}");
        //        return default;
        //    }
        //}
        #endregion

        #region POST 请求
        protected async Task<ApiResult<T>> PostAsync<T>(string url, object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var resultJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApiResult<T>>(resultJson);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"POST 异常：{ex.Message}");
                return ApiResult<T>.Fail(ex.Message); ;
            }
        }

        protected async Task<ApiResult> PostAsync(string url, object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var resultJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApiResult>(resultJson);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"POST 异常：{ex.Message}");
                return ApiResult.Fail(ex.Message); ;
            }
        }
        #endregion
    }
}
