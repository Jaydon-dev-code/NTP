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
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Http
{
    public abstract class BaseHttp
    {
        protected HttpClient _client;

        protected abstract string _controllerName { get; }

        public BaseHttp()
        {
            Intit();
        }

        protected abstract Uri InitHttpUri();
        protected virtual void Intit()
        {
            _client = new HttpClient
            {
                // 这里配置你的后端地址
                BaseAddress = InitHttpUri(),
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

        #region 文件上传
        protected async Task<ApiResult> PostFileAsync(string url, string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return ApiResult.Fail("文件不存在");
                }

                using (var content = new MultipartFormDataContent())
                {
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        var fileName = Path.GetFileName(filePath);
                        var streamContent = new StreamContent(fileStream);
                        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                        );
                        content.Add(streamContent, "file", fileName);

                        var response = await _client.PostAsync(url, content);
                        response.EnsureSuccessStatusCode();

                        var resultJson = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<ApiResult>(resultJson);
                    }
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"文件上传异常：{ex.Message}");
                return ApiResult.Fail(ex.Message);
            }
        }

        protected async Task<ApiResult> PostFileAsync(string url, Stream stream, string fileName)
        {
            try
            {
                if (stream == null)
                {
                    return ApiResult.Fail("文件流为空");
                }

                using (var content = new MultipartFormDataContent())
                {
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    );
                    content.Add(streamContent, "file", fileName);

                    var response = await _client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();

                    var resultJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ApiResult>(resultJson);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"文件上传异常：{ex.Message}");
                return ApiResult.Fail(ex.Message);
            }
        }
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
                return ApiResult<T>.Fail(ex.Message);
                
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
                return ApiResult.Fail(ex.Message);
                ;
            }
        }
        /// <summary>
        /// 三方对外的post 请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected async Task<ApiResult<T>> PostOtherAsync<T>(string url, object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var resultJson = await response.Content.ReadAsStringAsync();
                return ApiResult<T>.Success(JsonConvert.DeserializeObject<T>(resultJson));
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"POST 异常：{ex.Message}");
                return ApiResult<T>.Fail(ex.Message);

            }
        }

        #endregion
    }
}
