using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Client.Http
{
    public class EnergyRangeApi : BaseHttp
    {
        protected override string _controllerName => "EnergyRange";

        public async Task<ApiResult> ImportAsync(string filePath, bool overwrite = false)
        {
            var extension = Path.GetExtension(filePath)?.ToLower();
            if (extension != ".xlsx")
            {
                return ApiResult.Fail("无效的文件格式，只支持.xlsx文件");
            }

            return await PostFileAsync($"Import?overwrite={overwrite}", filePath);
        }

        public async Task<ApiResult> ImportAsync(Stream stream, string fileName, bool overwrite = false)
        {
            var extension = Path.GetExtension(fileName)?.ToLower();
            if (extension != ".xlsx")
            {
                return ApiResult.Fail("无效的文件格式，只支持.xlsx文件");
            }

            return await PostFileAsync($"Import?overwrite={overwrite}", stream, fileName);
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

        public async Task<ApiResult> StartSimulationAsync()
        {
            return await PostAsync("StartSimulation", null);
        }

        public async Task<ApiResult> StopSimulationAsync()
        {
            return await PostAsync("StopSimulation", null);
        }
    }
}
