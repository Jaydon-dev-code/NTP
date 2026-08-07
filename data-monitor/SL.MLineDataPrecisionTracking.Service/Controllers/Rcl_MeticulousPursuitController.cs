using Serilog;
using Serilog.Core;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage.Factory6Workshop6_3AssemblyLine;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Dtos.Request;
using SL.MLineDataPrecisionTracking.Models.Dtos.Response;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_3AssemblyLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    public class Rcl_MeticulousPursuitController : ApiController
    {
        Tb_HeatTreatmentDataRepository _heatTreatmentDataRepository;
        EnergyRangeExcelImportService _importService;
        Tb_Factory6Workshop6_3Line_EnergyRangeRepository _energyRangeRepository;
        Tb_Factory6Workshop6_3Line_EnergyRangePointRepository _energyRangePointRepository;
        McpCommunication _mcp;
        Tb_EquipmentRepository _equipmentRepository;

        static CancellationTokenSource _simulationCts;
        static readonly object _simulationLock = new();

        public Rcl_MeticulousPursuitController(
            Tb_HeatTreatmentDataRepository heatTreatmentDataRepository, EnergyRangeExcelImportService importService,
            Tb_Factory6Workshop6_3Line_EnergyRangeRepository energyRangeRepository,
            Tb_Factory6Workshop6_3Line_EnergyRangePointRepository energyRangePointRepository,
            McpCommunication mcp,
            Tb_EquipmentRepository equipmentRepository
        )
        {
            _heatTreatmentDataRepository = heatTreatmentDataRepository;
            _importService=importService;
            _energyRangeRepository = energyRangeRepository;
            _energyRangePointRepository = energyRangePointRepository;
            _mcp = mcp;
            _equipmentRepository = equipmentRepository;
        }

       
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="heatTreatmentDataQueryRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<HeatTreatmentDataQueryResponseDto>> QueryablToPagee(
            [FromBody] HeatTreatmentDataQueryRequestDto heatTreatmentDataQueryRequest
        )
        {
            try
            {
                var expression = GetExpression(heatTreatmentDataQueryRequest.RefinedSearch);
                if (expression.IsSuccess is false)
                {
                    return ApiResult<HeatTreatmentDataQueryResponseDto>.Fail(expression.Message);
                }
                var re = await _heatTreatmentDataRepository.QueryableAsync(
                    expression.Data,
                    x => x.RecordTime,
                    heatTreatmentDataQueryRequest.PageIndex,
                    heatTreatmentDataQueryRequest.DataCountPerPage
                );
                return ApiResult<HeatTreatmentDataQueryResponseDto>.Success(
                    new HeatTreatmentDataQueryResponseDto(re.List, re.TotalCount, re.TotalPage)
                );
            }
            catch (Exception ex)
            {
                Log.Warning("热处理查询数据失败！。\r\n{ex.Message}", ex.Message);
                return ApiResult<HeatTreatmentDataQueryResponseDto>.Fail(
                    $"热处理查询数据失败！。\r\n{ex.Message}"
                );
            }
        }

        /// <summary>
        /// 条码查询
        /// </summary>
        /// <param name="heatTreatmentDataQueryRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<HeatTreatmentDataQueryResponseDto>> MarkingNoQuery(
            [FromBody] HeatTreatmentDataQueryRequestDto heatTreatmentDataQueryRequest
        )
        {
            try
            {
                if (string.IsNullOrEmpty(heatTreatmentDataQueryRequest?.RefinedSearch?.MarkingNo))
                {
                    return ApiResult<HeatTreatmentDataQueryResponseDto>.Success(
                        new HeatTreatmentDataQueryResponseDto(
                            new List<Tb_HeatTreatmentData>(),
                            0,
                            0
                        )
                    );
                }
                var re = await _heatTreatmentDataRepository.QueryableFirstAsync(x =>
                    x.MarkingNo == heatTreatmentDataQueryRequest.RefinedSearch.MarkingNo
                );
                return ApiResult<HeatTreatmentDataQueryResponseDto>.Success(
                    new HeatTreatmentDataQueryResponseDto(
                      re==null?null:new List<Tb_HeatTreatmentData>() { re},
                        0,
                        0
                    )
                );
            }
            catch (Exception ex)
            {
                Log.Warning("热处理查询数据失败！。\r\n{ex.Message}", ex.Message);
                return ApiResult<HeatTreatmentDataQueryResponseDto>.Fail(
                    $"热处理查询数据失败！。\r\n{ex.Message}"
                );
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="heatTreatmentDataQueryRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<HeatTreatmentDataQueryResponseDto>> SaveQuery(
            [FromBody] HeatTreatmentDataQueryRequestDto heatTreatmentDataQueryRequest
        )
        {
            try
            {
                var expression = GetExpression(heatTreatmentDataQueryRequest.RefinedSearch);
                if (expression.IsSuccess is false)
                {
                    return ApiResult<HeatTreatmentDataQueryResponseDto>.Fail(expression.Message);
                }
                var re = await _heatTreatmentDataRepository.QueryableAsync(expression.Data);
                return ApiResult<HeatTreatmentDataQueryResponseDto>.Success(
                    new HeatTreatmentDataQueryResponseDto(re, 0, 0)
                );
            }
            catch (Exception ex)
            {
                Log.Warning("热处理查询数据失败！。\r\n{ex.Message}", ex.Message);
                return ApiResult<HeatTreatmentDataQueryResponseDto>.Fail(
                    $"热处理查询数据失败！。\r\n{ex.Message}"
                );
            }
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
        public async Task<ApiResult> SetCurrentStationModel(int id, string station)
        {
            try
            {
                await _importService.SetCurrentStationModelAsync(id, station);
                return ApiResult.Success();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"设置失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult<EnergyRangeQueryResponseDto>> GetEnergyRanges(
            [FromBody] EnergyRangeQueryRequestDto request
        )
        {
            try
            {
                var expression = SqlSugar.Expressionable
                    .Create<Tb_Factory6Workshop6_3Line_EnergyRange>()
                    .And(x =>
                        x.RecordTime >= request.StartTime && x.RecordTime <= request.EndTime
                    )
                    .ToExpression();

                var re = await _energyRangeRepository.QueryableAsync(
                    expression,
                    x => x.RecordTime,
                    request.PageIndex,
                    request.DataCountPerPage
                );
                return ApiResult<EnergyRangeQueryResponseDto>.Success(
                    new EnergyRangeQueryResponseDto(re.List, re.TotalCount, re.TotalPage)
                );
            }
            catch (Exception ex)
            {
                Log.Warning("能量范围查询失败！。\r\n{ex.Message}", ex.Message);
                return ApiResult<EnergyRangeQueryResponseDto>.Fail(
                    $"能量范围查询失败！。\r\n{ex.Message}"
                );
            }
        }

        [HttpPost]
        public async Task<ApiResult<EnergyRangePointQueryResponseDto>> GetEnergyRangePoints(
            [FromBody] EnergyRangePointQueryRequestDto request
        )
        {
            try
            {
                // 已知子项 Count，按条件一次查出全部点（Take(count) 限定，避免全表扫描），不分页
                var list = await _energyRangePointRepository.QueryableAllAsync(
                    x => x.EnergyRange_RecordTime == request.EnergyRangeRecordTime,
                    request.Count
                );
                return ApiResult<EnergyRangePointQueryResponseDto>.Success(
                    new EnergyRangePointQueryResponseDto(list, list.Count, 1)
                );
            }
            catch (Exception ex)
            {
                Log.Warning("能量记录查询失败！。\r\n{ex.Message}", ex.Message);
                return ApiResult<EnergyRangePointQueryResponseDto>.Fail(
                    $"能量记录查询失败！。\r\n{ex.Message}"
                );
            }
        }

        /// <summary>
        /// 开启/关闭能量模拟推送（正在推送时调用则停止，停止时调用则开启）
        /// 数据直接写入 PLC（能量/能量时间/能量采集许可 点），由真实采集 EnergyCollectionLoopAsync 读取后推送前端并存库
        /// 接口地址：POST /api/Rcl_MeticulousPursuit/ToggleEnergySimulation
        /// </summary>
        [HttpPost]
        public ApiResult ToggleEnergySimulation()
        {
            lock (_simulationLock)
            {
                if (_simulationCts != null && !_simulationCts.IsCancellationRequested)
                {
                    _simulationCts.Cancel();
                    _simulationCts.Dispose();
                    _simulationCts = null;
                    return ApiResult.Success("能量模拟推送已停止");
                }

                _simulationCts = new CancellationTokenSource();
                var token = _simulationCts.Token;

                Task.Run(() => PushEnergySimulation(token, loop: true), token);

                return ApiResult.Success("能量模拟推送已开启");
            }
        }

        /// <summary>
        /// 测试接口：模拟推送一次（一个工件，最多 10 秒，前 2 秒递增到 7000，之后 7000~8500 随机），运行完自动停止
        /// 数据直接写入 PLC（能量/能量时间/能量采集许可 点），由真实采集 EnergyCollectionLoopAsync 读取后推送前端并存库
        /// 接口地址：POST /api/Rcl_MeticulousPursuit/SimulateEnergyOnce
        /// </summary>
        [HttpPost]
        public ApiResult SimulateEnergyOnce()
        {
            Task.Run(() => PushEnergySimulation(CancellationToken.None, loop: false));
            return ApiResult.Success("能量模拟推送已开始（运行一次）");
        }

        async Task PushEnergySimulation(CancellationToken token, bool loop)
        {
            var (valuePoint, timePoint, permitPoint) = await LoadEnergyPlcPointsAsync();
            if (valuePoint == null || timePoint == null || permitPoint == null)
                return;

            var rnd = new Random();

            // 置位能量采集许可，让真实采集循环开始采集
            await WritePlcPointAsync(permitPoint, true);

            try
            {
                do
                {
                    // 每个工件最多 10 秒，每 0.1 秒写一点到 PLC
                    for (int i = 0; i < 100; i++)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        // 与生产一致：数据放大 10 倍（时间 0~100，能量 7000~8500），写入 PLC 均为整数
                        int time = i;
                        int value;

                        // 前 2 秒能量逐渐递增到 7000
                        if (i <= 20)
                        {
                            double progress = i / 20.0;
                            value = (int)Math.Round(
                                7000 * progress + (rnd.NextDouble() - 0.5) * 200
                            );
                            if (value < 0)
                                value = 0;
                        }
                        else
                        {
                            // 2 秒后在 7000 ~ 8500 之间随机波动
                            value = (int)Math.Round(7000 + rnd.NextDouble() * 1500);
                        }

                        await WritePlcPointAsync(timePoint, time);
                        await WritePlcPointAsync(valuePoint, value);

                        try
                        {
                            await Task.Delay(100, token);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                    }
                } while (loop && !token.IsCancellationRequested);
            }
            finally
            {
                // 复位能量采集许可，触发真实采集循环落库
                await WritePlcPointAsync(permitPoint, false);
            }
        }

        async Task<(DevPlcPointDto ValuePoint, DevPlcPointDto TimePoint, DevPlcPointDto PermitPoint)>
            LoadEnergyPlcPointsAsync()
        {
            var linePoint = await _equipmentRepository.GetEquipmentAllAsync(x =>
                x.DeviceName == "热处理"
            );
            if (linePoint is null)
                return (null, null, null);

            var points = new List<DevPlcPointDto>();
            foreach (var plcLinkeInfo in linePoint.PlcConnections)
            {
                foreach (var plcAddres in plcLinkeInfo.Points)
                {
                    points.Add(
                        new DevPlcPointDto(
                            linePoint.DeviceName,
                            plcAddres.PointName,
                            plcLinkeInfo.IpAddress,
                            plcLinkeInfo.Port,
                            plcAddres.Area,
                            plcAddres.DataType.ToTypeCode(),
                            plcAddres.Address,
                            plcAddres.Length,
                            plcAddres.ReadFormula,
                            plcAddres.WriteFormula
                        )
                    );
                }
            }

            var valuePoint = points.FirstOrDefault(x => x.PointName == "能量");
            var timePoint = points.FirstOrDefault(x => x.PointName == "能量时间");
            var permitPoint = points.FirstOrDefault(x => x.PointName == "能量采集许可");

            return (valuePoint, timePoint, permitPoint);
        }

        async Task WritePlcPointAsync(DevPlcPointDto point, object value)
        {
            point.Value = new List<object> { value };
            var result = await _mcp.WriteAsync(point);
            if (result.IsSuccess is false)
            {
                Log.Warning(
                    "[能量模拟]【{_serviceName}】写入 PLC 点位失败：{pointName} - {Message}",
                    "Rcl_MeticulousPursuit",
                    point.PointName,
                    result.Message
                );
            }
        }

        private string GetQueryString(string key)
        {
            var query = Request.GetQueryNameValuePairs();
            var pair = query.FirstOrDefault(x => x.Key == key);
            return pair.Value;
        }
        Result<Expression<Func<Tb_HeatTreatmentData, bool>>> GetExpression(
            RefinedSearchCriteria search
        )
        {
            var exp = SqlSugar.Expressionable.Create<Tb_HeatTreatmentData>();
            var startTime = search.StartDate.Date + search.StartTime.TimeOfDay;
            var endTime = search.EndDate.Date + search.EndTime.TimeOfDay;
            if (startTime > endTime)
            {
                return Result<Expression<Func<Tb_HeatTreatmentData, bool>>>.Fail(
                    "起始时间不能大于结束时间！"
                );
            }
            else
            {
                exp.And(x => x.RecordTime >= startTime && x.RecordTime <= endTime);
            }

            if (string.IsNullOrEmpty(search.MarkingNo) is false)
            {
                exp.And(x => x.MarkingNo.Contains(search.MarkingNo));
            }

            return Result<Expression<Func<Tb_HeatTreatmentData, bool>>>.Success(exp.ToExpression());
        }
    }
}
