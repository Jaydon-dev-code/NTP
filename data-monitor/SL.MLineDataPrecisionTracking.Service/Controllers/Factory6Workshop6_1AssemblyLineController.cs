using Serilog;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Request;
using SL.MLineDataPrecisionTracking.Models.Dtos.Response;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_1AssemblyLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    public class Factory6Workshop6_1AssemblyLineController : ApiController
    {
        Tb_Factory6Workshop6_1AssemblyLineABSummaryRepository _lineSummaryRepository;

        public Factory6Workshop6_1AssemblyLineController(
            Tb_Factory6Workshop6_1AssemblyLineABSummaryRepository lineSummaryRepository
        )
        {
            _lineSummaryRepository = lineSummaryRepository;
        }

        [HttpPost]
        public async Task<ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>> QueryablToPagee(
            [FromBody] LineSummaryQueryRequestDto lineSummaryQueryRequest
        )
        {
            try
            {
                var expression = GetExpression(lineSummaryQueryRequest.RefinedSearch);
                if (expression.IsSuccess is false)
                {
                    return ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>.Fail(
                        expression.Message
                    );
                }
                var re = await _lineSummaryRepository.QueryableAsync(
                    expression.Data,
                    x => x.RecordTime,
                    lineSummaryQueryRequest.PageNumber,
                    lineSummaryQueryRequest.PageSize
                );
                return ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>.Success(
                    new Factory6Workshop6_1AssemblyLineResponseDto(
                        re.List,
                        re.TotalCount,
                        re.TotalPage
                    )
                );
            }
            catch (Exception ex)
            {
                Log.Warning("装配线查询数据失败！。\r\n{ex.Message}", ex.Message);
                return ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>.Fail(
                    $"装配线查询数据失败！。\r\n{ex.Message}"
                );
            }
        }

        [HttpPost]
        public async Task<ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>> MarkingNoQuery(
            [FromBody] LineSummaryQueryRequestDto lineSummaryQueryRequest
        )
        {
            try
            {
                if (string.IsNullOrEmpty(lineSummaryQueryRequest?.RefinedSearch?.MarkingNo))
                {
                    return ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>.Success(
                        new Factory6Workshop6_1AssemblyLineResponseDto(
                            new List<Tb_Factory6Workshop6_1AssemblyLineABSummary>(),
                            0,
                            0
                        )
                    );
                }
                var re = await _lineSummaryRepository.QueryableFirstAsync(x =>
                    x.MarkingNo == lineSummaryQueryRequest.RefinedSearch.MarkingNo
                );
                return ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>.Success(
                    new Factory6Workshop6_1AssemblyLineResponseDto(
                        re == null
                            ? null
                            : new List<Tb_Factory6Workshop6_1AssemblyLineABSummary> { re },
                        0,
                        0
                    )
                );
            }
            catch (Exception ex)
            {
                Log.Warning("装配线查询数据失败！。\r\n{ex.Message}", ex.Message);
                return ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>.Fail(
                    $"装配线查询数据失败！。\r\n{ex.Message}"
                );
            }
        }

        [HttpPost]
        public async Task<ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>> SaveQuery(
            [FromBody] LineSummaryQueryRequestDto lineSummaryQueryRequest
        )
        {
            try
            {
                var expression = GetExpression(lineSummaryQueryRequest.RefinedSearch);
                if (expression.IsSuccess is false)
                {
                    return ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>.Fail(
                        expression.Message
                    );
                }
                var re = await _lineSummaryRepository.QueryableAsync(expression.Data);
                return ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>.Success(
                    new Factory6Workshop6_1AssemblyLineResponseDto(re, 0, 0)
                );
            }
            catch (Exception ex)
            {
                Log.Warning("装配线查询数据失败！。\r\n{ex.Message}", ex.Message);
                return ApiResult<Factory6Workshop6_1AssemblyLineResponseDto>.Fail(
                    $"装配线查询数据失败！。\r\n{ex.Message}"
                );
            }
        }

        Result<Expression<Func<Tb_Factory6Workshop6_1AssemblyLineABSummary, bool>>> GetExpression(
            RefinedSearchCriteria search
        )
        {
            var exp = SqlSugar.Expressionable.Create<Tb_Factory6Workshop6_1AssemblyLineABSummary>();
            var startTime = search.StartDate.Date + search.StartTime.TimeOfDay;
            var endTime = search.EndDate.Date + search.EndTime.TimeOfDay;
            if (startTime > endTime)
            {
                return Result<Expression<Func<Tb_Factory6Workshop6_1AssemblyLineABSummary, bool>>>.Fail(
                    "起始时间不能大于结束时间！"
                );
            }
            else
            {
                exp.And(x => x.RecordTime >= startTime && x.RecordTime <= endTime);
            }
            if (string.IsNullOrEmpty(search.TrayNoA) is false)
            {
                exp.And(x => x.LineATrayNo == search.TrayNoA);
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
                exp.And(x => x.MarkingNo.Contains(search.MarkingNo));
            }
            if (string.IsNullOrEmpty(search.ModelName) is false)
            {
                exp.And(x => x.ModelName == search.ModelName);
            }
            if (search.Result != Models.Enum.ResultEnum.ALL)
            {
                exp.And(x => x.Result == search.Result);
            }
            return Result<Expression<Func<Tb_Factory6Workshop6_1AssemblyLineABSummary, bool>>>.Success(
                exp.ToExpression()
            );
        }
    }
}
