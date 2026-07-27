using Serilog;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Request;
using SL.MLineDataPrecisionTracking.Models.Dtos.Response;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory4Workshop4_10Line;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    public class Factory4Workshop4_10LineController : ApiController
    {
        Tb_Factory4Workshop4_10LineSummaryRepository _summaryRepository;

        public Factory4Workshop4_10LineController(Tb_Factory4Workshop4_10LineSummaryRepository summaryRepository)
        {
            _summaryRepository = summaryRepository;
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
