using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Request;
using SL.MLineDataPrecisionTracking.Models.Dtos.Response;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Client.Http
{
    public class Rcl_MeticulousPursuitApi : BaseHttp
    {
        protected override string _controllerName => "Rcl_MeticulousPursuit";

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="heatTreatmentDataQueryRequest">查询请求参数</param>
        /// <returns>分页查询结果</returns>
        public async Task<ApiResult<HeatTreatmentDataQueryResponseDto>> QueryablToPagee(
            HeatTreatmentDataQueryRequestDto heatTreatmentDataQueryRequest
        )
        {
            return await PostAsync<HeatTreatmentDataQueryResponseDto>(
                "QueryablToPagee",
                heatTreatmentDataQueryRequest
            );
        }

        /// <summary>
        /// 条码查询
        /// </summary>
        /// <param name="heatTreatmentDataQueryRequest">查询请求参数</param>
        /// <returns>条码查询结果</returns>
        public async Task<ApiResult<HeatTreatmentDataQueryResponseDto>> MarkingNoQuery(
            HeatTreatmentDataQueryRequestDto heatTreatmentDataQueryRequest
        )
        {
            return await PostAsync<HeatTreatmentDataQueryResponseDto>(
                "MarkingNoQuery",
                heatTreatmentDataQueryRequest
            );
        }

        /// <summary>
        /// 导出查询
        /// </summary>
        /// <param name="heatTreatmentDataQueryRequest">查询请求参数</param>
        /// <returns>导出查询结果</returns>
        public async Task<ApiResult<HeatTreatmentDataQueryResponseDto>> SaveQuery(
            HeatTreatmentDataQueryRequestDto heatTreatmentDataQueryRequest
        )
        {
            return await PostAsync<HeatTreatmentDataQueryResponseDto>(
                "SaveQuery",
                heatTreatmentDataQueryRequest
            );
        }

        public async Task<ApiResult> ImportAsync(string filePath, bool overwrite = false)
        {
            var extension = Path.GetExtension(filePath)?.ToLower();
            if (extension != ".xlsx")
            {
                return ApiResult.Fail("无效的文件格式，只支持.xlsx文件");
            }

            return await PostFileAsync($"Import?overwrite={overwrite}", filePath);
        }

        public async Task<ApiResult<List<Tb_EnergyRange>>> GetAllAsync()
        {
            return await PostAsync<List<Tb_EnergyRange>>("GetAll", null);
        }

        public async Task<ApiResult> DeleteNavAsync(int id)
        {
            return await PostAsync("DeleteNav", id);
        }

        public async Task<ApiResult> SetCurrentStationModelAsync(int id, string station)
        {
            return await PostAsync($"SetCurrentStationModel?id={id}&station={station}", null);
        }

        public async Task<ApiResult<EnergyRangeQueryResponseDto>> GetEnergyRangesAsync(
            EnergyRangeQueryRequestDto request
        )
        {
            return await PostAsync<EnergyRangeQueryResponseDto>("GetEnergyRanges", request);
        }

        public async Task<ApiResult<EnergyRangePointQueryResponseDto>> GetEnergyRangePointsAsync(
            EnergyRangePointQueryRequestDto request
        )
        {
            return await PostAsync<EnergyRangePointQueryResponseDto>(
                "GetEnergyRangePoints",
                request
            );
        }
    }
}