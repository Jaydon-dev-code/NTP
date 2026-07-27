using Microsoft.AspNet.SignalR;
using Serilog;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Dtos.Request;
using SL.MLineDataPrecisionTracking.Models.Dtos.Response;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using SL.MLineDataPrecisionTracking.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

using IClientProxy = Microsoft.AspNet.SignalR.Hubs.IClientProxy;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    public class Factory4Workshop4_10LineController : ApiController
    {
        Tb_Factory4Workshop4_10LineSummaryRepository _summaryRepository;
        IHubContext _chatHub;

        static CancellationTokenSource _testCts;
        static readonly object _testLock = new();

        public Factory4Workshop4_10LineController(
            Tb_Factory4Workshop4_10LineSummaryRepository summaryRepository,
            IHubContext chatHub)
        {
            _summaryRepository = summaryRepository;
            _chatHub = chatHub;
        }

        [HttpPost]
        public async Task<ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>> QueryablToPagee(
            [FromBody] Factory4Workshop4_10LineSummaryQueryRequestDto request)
        {
            try
            {
                var expression = GetExpression(request);
                if (expression.IsSuccess == false)
                {
                    return ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>.Fail(expression.Message);
                }
                var re = await _summaryRepository.QueryableAsync(
                    expression.Data,
                    x => x.RecordTime,
                    request.PageNumber,
                    request.PageSize
                );
                return ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>.Success(
                    new Factory4Workshop4_10LineSummaryQueryResponseDto(re.List, re.TotalCount, re.TotalPage)
                );
            }
            catch (Exception ex)
            {
                Log.Warning("4-10线数据查询失败！\r\n{ex.Message}", ex.Message);
                return ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>.Fail($"4-10线数据查询失败！\r\n{ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>> SNQuery(
            [FromBody] Factory4Workshop4_10LineSummaryQueryRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.SN))
                {
                    return ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>.Success(
                        new Factory4Workshop4_10LineSummaryQueryResponseDto(new List<Tb_Factory4Workshop4_10LineSummary>(), 0, 0)
                    );
                }
                var re = await _summaryRepository.QueryableFirstAsync(x => x.SN == request.SN);
                return ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>.Success(
                    new Factory4Workshop4_10LineSummaryQueryResponseDto(
                        re == null ? null : new List<Tb_Factory4Workshop4_10LineSummary> { re },
                        0, 0
                    )
                );
            }
            catch (Exception ex)
            {
                Log.Warning("4-10线SN查询失败！\r\n{ex.Message}", ex.Message);
                return ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>.Fail($"4-10线SN查询失败！\r\n{ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>> SaveQuery(
            [FromBody] Factory4Workshop4_10LineSummaryQueryRequestDto request)
        {
            try
            {
                var expression = GetExpression(request);
                if (expression.IsSuccess == false)
                {
                    return ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>.Fail(expression.Message);
                }
                var re = await _summaryRepository.QueryableAsync(expression.Data);
                return ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>.Success(
                    new Factory4Workshop4_10LineSummaryQueryResponseDto(re, 0, 0)
                );
            }
            catch (Exception ex)
            {
                Log.Warning("4-10线数据导出失败！\r\n{ex.Message}", ex.Message);
                return ApiResult<Factory4Workshop4_10LineSummaryQueryResponseDto>.Fail($"4-10线数据导出失败！\r\n{ex.Message}");
            }
        }

        [HttpPost]
        public ApiResult<string> TestData()
        {
            lock (_testLock)
            {
                if (_testCts != null && !_testCts.IsCancellationRequested)
                {
                    _testCts.Cancel();
                    _testCts.Dispose();
                    _testCts = null;
                    return ApiResult<string>.Success("测试数据已停止推送");
                }

                _testCts = new CancellationTokenSource();
                var token = _testCts.Token;

                Task.Run(() => PushTestData(token), token);

                return ApiResult<string>.Success("测试数据已开始推送");
            }
        }

        void PushTestData(CancellationToken token)
        {
            var rng = new Random();
            var results = new[] { ResultEnum.OK, ResultEnum.NG };
            var all = (IClientProxy)_chatHub.Clients.All;

            while (!token.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var sn = $"TEST{now:HHmmssfff}";

                all.Invoke("VibData", new Factory4Workshop4_10Line_VibDto
                {
                    SN = sn,
                    VibCrackResult = results[rng.Next(2)],
                    RecordTime = now,
                });

                all.Invoke("RivetingData", new Factory4Workshop4_10Line_RivetAndCrack_RivetingDto
                {
                    SN = sn,
                    RivetingResult = results[rng.Next(2)],
                    RivetingTime = now,
                });

                all.Invoke("SpinRivetingData", new Factory4Workshop4_10Line_RivetAndCrack_SpinRivetingDto
                {
                    SN = sn,
                    RivetingInspection1Height = $"{rng.NextDouble() * 10:F3}",
                    RivetingInspection2Height = $"{rng.NextDouble() * 10:F3}",
                    SpiralRivetingFormingHeight = $"{rng.NextDouble() * 10:F3}",
                    SpinRivetingTime = now,
                });

                all.Invoke("ClearanceStation1Data", new Factory4Workshop4_10Line_Clearance_Station1Dto
                {
                    SN = sn,
                    Clearance1Result = results[rng.Next(2)],
                    PositiveGap = $"{rng.NextDouble() * 5:F2}",
                    LowerLoad = $"{rng.NextDouble() * 100:F1}",
                    UpperLoad = $"{rng.NextDouble() * 100:F1}",
                    Clearance1Time = now,
                });

                all.Invoke("ClearanceStation2Data", new Factory4Workshop4_10Line_Clearance_Station2Dto
                {
                    SN = sn,
                    Clearance2Result = results[rng.Next(2)],
                    Offset = $"{rng.NextDouble() * 2:F3}",
                    BeforePressIn = $"{rng.NextDouble() * 10:F2}",
                    AfterPressIn = $"{rng.NextDouble() * 10:F2}",
                    Gap = $"{rng.NextDouble() * 5:F3}",
                    Clearance2Time = now,
                });

                all.Invoke("ABSPressDownData", new Factory4Workshop4_10Line_ABS_PressDownDto
                {
                    SN = sn,
                    ABSPressDownResult = results[rng.Next(2)],
                    ABSPressDownTime = now,
                });

                all.Invoke("ABSCheckData", new Factory4Workshop4_10Line_ABS_CheckDto
                {
                    SN = sn,
                    ABSCheckResult = results[rng.Next(2)],
                    ABSCheckTime = now,
                });

                all.Invoke("IsOnlieClearance", true);
                all.Invoke("IsOnlieRivetAndCrack", true);
                all.Invoke("IsOnlieVib", true);
                all.Invoke("IsOnlieVibScan", true);
                all.Invoke("IsOnlieABS", true);

                Task.Delay(1000, token).Wait();
            }
        }

        Result<Expression<Func<Tb_Factory4Workshop4_10LineSummary, bool>>> GetExpression(Factory4Workshop4_10LineSummaryQueryRequestDto request)
        {
            var exp = SqlSugar.Expressionable.Create<Tb_Factory4Workshop4_10LineSummary>();
            if (request.StartTime > request.EndTime)
            {
                return Result<Expression<Func<Tb_Factory4Workshop4_10LineSummary, bool>>>.Fail("起始时间不能大于结束时间！");
            }
            exp.And(x => x.RecordTime >= request.StartTime && x.RecordTime <= request.EndTime);
            if (string.IsNullOrEmpty(request.SN) == false)
            {
                exp.And(x => x.SN.Contains(request.SN));
            }
            return Result<Expression<Func<Tb_Factory4Workshop4_10LineSummary, bool>>>.Success(exp.ToExpression());
        }
    }
}
