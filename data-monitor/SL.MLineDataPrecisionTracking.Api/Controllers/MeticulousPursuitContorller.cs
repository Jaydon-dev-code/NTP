using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Serilog;
using Serilog.Core;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Request;
using SL.MLineDataPrecisionTracking.Models.Dtos.Response;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Api.Controllers
{
    public class MeticulousPursuitController : ApiController
    {
        Tb_LineSummaryRepository _lineSummaryRepository;

        public MeticulousPursuitController(Tb_LineSummaryRepository lineSummaryRepository)
        {
            _lineSummaryRepository = lineSummaryRepository;
        }

        [HttpGet]
        public  DateTime GetTime()
        {
            return DateTime.Now;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="lineSummaryQueryRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<LineSummaryQueryResponseDto>> QueryablToPagee(
            [FromBody] LineSummaryQueryRequestDto lineSummaryQueryRequest
        )
        {
            try
            {
                var expression = GetExpression(lineSummaryQueryRequest.RefinedSearch);
                if (expression.IsSuccess is false)
                {
                    return ApiResult<LineSummaryQueryResponseDto>.Fail(expression.Message);
                }
                var re = await _lineSummaryRepository.QueryableAsync(
                    expression.Data,
                    x => x.RecordTime,
                    lineSummaryQueryRequest.PageNumber,
                    lineSummaryQueryRequest.PageSize
                );
                return ApiResult<LineSummaryQueryResponseDto>.Success(
                    new LineSummaryQueryResponseDto(re.List, re.TotalCount, re.TotalPage)
                );
            }
            catch (Exception ex)
            {
                Log.Warning("装配线查询数据失败！。\r\n{ex.Message}", ex.Message);
                return ApiResult<LineSummaryQueryResponseDto>.Fail(
                    $"装配线查询数据失败！。\r\n{ex.Message}"
                );
            }
        }

        /// <summary>
        /// 条码查询
        /// </summary>
        /// <param name="lineSummaryQueryRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<LineSummaryQueryResponseDto>> MarkingNoQuery(
            [FromBody] LineSummaryQueryRequestDto lineSummaryQueryRequest
        )
        {
            try
            {
                if (string.IsNullOrEmpty(lineSummaryQueryRequest?.RefinedSearch?.MarkingNo))
                {
                    return ApiResult<LineSummaryQueryResponseDto>.Success(
                        new LineSummaryQueryResponseDto(new List<Tb_LineSummary>(), 0, 0)
                    );
                }
                var re = await _lineSummaryRepository.QueryableFirstAsync(x =>
                    x.MarkingNo == lineSummaryQueryRequest.RefinedSearch.MarkingNo
                );
                return ApiResult<LineSummaryQueryResponseDto>.Success(
                    new LineSummaryQueryResponseDto(
                        new List<Models.Entities.Tb_LineSummary>() { re },
                        0,
                        0
                    )
                );
            }
            catch (Exception ex)
            {
                Log.Warning("装配线查询数据失败！。\r\n{ex.Message}", ex.Message);
                return ApiResult<LineSummaryQueryResponseDto>.Fail(
                    $"装配线查询数据失败！。\r\n{ex.Message}"
                );
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="lineSummaryQueryRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<LineSummaryQueryResponseDto>> SaveQuery(
            [FromBody] LineSummaryQueryRequestDto lineSummaryQueryRequest
        )
        {
            try
            {
                var expression = GetExpression(lineSummaryQueryRequest.RefinedSearch);
                if (expression.IsSuccess is false)
                {
                    return ApiResult<LineSummaryQueryResponseDto>.Fail(expression.Message);
                }
                var re = await _lineSummaryRepository.QueryableAsync(expression.Data);
                return ApiResult<LineSummaryQueryResponseDto>.Success(
                    new LineSummaryQueryResponseDto(re, 0, 0)
                );
            }
            catch (Exception ex)
            {
                Log.Warning("装配线查询数据失败！。\r\n{ex.Message}", ex.Message);
                return ApiResult<LineSummaryQueryResponseDto>.Fail(
                    $"装配线查询数据失败！。\r\n{ex.Message}"
                );
            }
        }

        Result<Expression<Func<Tb_LineSummary, bool>>> GetExpression(RefinedSearchCriteria search)
        {
            var exp = SqlSugar.Expressionable.Create<Tb_LineSummary>();
            var startTime = search.StartDate.Date + search.StartTime.TimeOfDay;
            var endTime = search.EndDate.Date + search.EndTime.TimeOfDay;
            if (startTime > endTime)
            {
                return Result<Expression<Func<Tb_LineSummary, bool>>>.Fail(
                    "起始时间不能大于结束时间！"
                );
            }
            else
            {
                exp.And(x => x.RecordTime >= startTime && x.RecordTime <= endTime);
            }
            if (string.IsNullOrEmpty(search.TrayNoA) is false)
            {
                exp.And(x => x.TrayNoA == search.TrayNoA);
            }
            if (string.IsNullOrEmpty(search.TrayNoB) is false)
            {
                exp.And(x => x.TrayNoB == search.TrayNoB);
            }
            if (string.IsNullOrEmpty(search.NgCodeA) is false)
            {
                exp.And(x => x.NgCodeA == search.NgCodeA);
            }
            if (string.IsNullOrEmpty(search.NgCodeB) is false)
            {
                exp.And(x => x.NgCodeB == search.NgCodeB);
            }
            if (string.IsNullOrEmpty(search.MarkingNo) is false)
            {
                exp.And(x => x.MarkingNo == search.MarkingNo);
            }
            if (string.IsNullOrEmpty(search.ModelName) is false)
            {
                exp.And(x => x.ModelName == search.ModelName);
            }
            if (search.Result != Models.Enum.ResultEnum.ALL)
            {
                exp.And(x => x.Result == search.Result);
            }
            return Result<Expression<Func<Tb_LineSummary, bool>>>.Success(exp.ToExpression());
        }
    }
}