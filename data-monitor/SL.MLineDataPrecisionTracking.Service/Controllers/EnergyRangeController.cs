using Microsoft.AspNet.SignalR;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    public class EnergyRangeController : ApiController
    {
        EnergyRangeExcelImportService _importService;
        IHubContext _hubContext;
        static CancellationTokenSource _simulationCts;

        public EnergyRangeController(EnergyRangeExcelImportService importService, IHubContext hubContext)
        {
            _importService = importService;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<ApiResult> Import()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return ApiResult.Fail("请求内容不是multipart/form-data格式");
                }

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (var content in provider.Contents)
                {
                    var fileName = content.Headers.ContentDisposition.FileName.Trim('\"');

                    var extension = Path.GetExtension(fileName)?.ToLower();
                    if (extension != ".xlsx")
                    {
                        return ApiResult.Fail("无效的文件格式，只支持.xlsx文件");
                    }

                    using (var stream = await content.ReadAsStreamAsync())
                    {
                        var overwriteStr = GetQueryString("overwrite");
                        bool.TryParse(overwriteStr, out bool overwrite);
                        return await _importService.ImportAsync(stream, overwrite);
                    }
                }

                return ApiResult.Fail("未找到上传的文件");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"导入失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult<List<Tb_EnergyRange>>> GetAll()
        {
            try
            {
                var data = await _importService.GetAllAsync();
                return ApiResult<List<Tb_EnergyRange>>.Success(data);
            }
            catch (Exception ex)
            {
                return ApiResult<List<Tb_EnergyRange>>.Fail($"获取数据失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> DeleteNav([FromBody] int id)
        {
            try
            {
                await _importService.DeleteNavAsync(x => x.Id == id);
                return ApiResult.Success();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"更新失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> ToggleEnabled(int id, bool isEnabled)
        {
            try
            {
                await _importService.ToggleEnabledAsync(id, isEnabled);
                return ApiResult.Success();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"更新失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> SetCurrentModel(int id)
        {
            try
            {
                await _importService.SetCurrentModelAsync(id);
                return ApiResult.Success();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"设置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// POST /api/EnergyRange/StartSimulation
        /// </summary>
        [HttpPost]
        public ApiResult StartSimulation()
        {
            if (_simulationCts != null)
            {
                _simulationCts.Cancel();
            }
            _simulationCts = new CancellationTokenSource();
            var token = _simulationCts.Token;

            Task.Run(async () =>
            {
                var rnd = new Random();

                while (!token.IsCancellationRequested)
                {
                    double y = rnd.NextDouble() * 5;
                    double targetAt05 = 80 + rnd.NextDouble() * 5;

                    for (int i = 0; i < 90; i++)
                    {
                        if (token.IsCancellationRequested) return;

                        double x = i * 0.1;

                        if (i > 0 && i <= 5)
                        {
                            double progress = i / 5.0;
                            y = (1 - progress) * y + progress * targetAt05;
                            y += (rnd.NextDouble() - 0.5) * 2;
                        }
                        else if (i > 5)
                        {
                            double next = 90 + rnd.NextDouble() * 10;
                            if (Math.Abs(next - y) > 2)
                                next = y + (next > y ? 1 : -1) * (1 + rnd.NextDouble());
                            next = Math.Max(90, Math.Min(100, next));
                            y = next;
                        }

                        var simPoint = new PointData<double, double>(Math.Round(x, 1), Math.Round(y, 1));
                        _hubContext.Clients.All.EnergyRangeUpdated(simPoint);

                        await Task.Delay(100, token);
                    }
                }
            }, token);

            return ApiResult.Success("模拟推送已启动");
        }

        /// <summary>
        /// POST /api/EnergyRange/StopSimulation
        /// </summary>
        [HttpPost]
        public ApiResult StopSimulation()
        {
            if (_simulationCts != null)
            {
                _simulationCts.Cancel();
                _simulationCts = null;
            }
            return ApiResult.Success("模拟推送已停止");
        }

        /// <summary>
        /// POST /api/EnergyRange/StartSimulationOnePice
        /// </summary>
        [HttpPost]
        public ApiResult StartSimulationOnePice()
        {
            if (_simulationCts != null)
            {
                _simulationCts.Cancel();
            }
            _simulationCts = new CancellationTokenSource();
            var token = _simulationCts.Token;

            Task.Run(async () =>
            {
                var rnd = new Random();


                double y = rnd.NextDouble() * 5;
                double targetAt05 = 80 + rnd.NextDouble() * 5;

                for (int i = 0; i < 100; i++)
                {

                    double x = i * 0.1;

                    if (i > 0 && i <= 5)
                    {
                        double progress = i / 5.0;
                        y = (1 - progress) * y + progress * targetAt05;
                        y += (rnd.NextDouble() - 0.5) * 2;
                    }
                    else if (i > 5)
                    {
                        double next = 90 + rnd.NextDouble() * 10;
                        if (Math.Abs(next - y) > 2)
                            next = y + (next > y ? 1 : -1) * (1 + rnd.NextDouble());
                        next = Math.Max(90, Math.Min(100, next));
                        y = next;
                    }

                    var simPoint = new PointData<double, double>(Math.Round(x, 1), Math.Round(y, 1));
                    _hubContext.Clients.All.EnergyRangeUpdated(simPoint);

                    await Task.Delay(100, token);
                }
            });
            return ApiResult.Success("模拟推送已启动");
        }
        private string GetQueryString(string key)
        {
            var query = Request.GetQueryNameValuePairs();
            var pair = query.FirstOrDefault(x => x.Key == key);
            return pair.Value;
        }
    }
}
