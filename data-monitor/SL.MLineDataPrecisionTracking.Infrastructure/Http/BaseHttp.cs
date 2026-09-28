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
using Org.BouncyCastle.Utilities.IO.Pem;
using Serilog;
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Http
{
    public abstract class BaseHttp
    {
        protected HttpClient _client;

        public BaseHttp()
        {
            Init();
        }

        protected abstract void Init();

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

        protected async Task<T> PostExternalAsync<T>(string url, object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var resultJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(resultJson);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"POST 异常：{ex.Message}");
                return default;
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
            }
        }
        #endregion
    }
}
