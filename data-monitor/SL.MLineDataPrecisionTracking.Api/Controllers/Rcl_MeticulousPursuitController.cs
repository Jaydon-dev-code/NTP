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
    public class Rcl_MeticulousPursuitController : ApiController
    {
        Tb_HeatTreatmentDataRepository _heatTreatmentDataRepository;

        public Rcl_MeticulousPursuitController(
            Tb_HeatTreatmentDataRepository heatTreatmentDataRepository
        )
        {
            _heatTreatmentDataRepository = heatTreatmentDataRepository;
        }

        [HttpGet]
        public DateTime GetTime()
        {
            return DateTime.Now;
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
                    heatTreatmentDataQueryRequest.PageNumber,
                    heatTreatmentDataQueryRequest.PageSize
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
                        new List<Tb_HeatTreatmentData>() { re },
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
                exp.And(x => x.MarkingNo == search.MarkingNo);
            }

            return Result<Expression<Func<Tb_HeatTreatmentData, bool>>>.Success(exp.ToExpression());
        }
    }
}
