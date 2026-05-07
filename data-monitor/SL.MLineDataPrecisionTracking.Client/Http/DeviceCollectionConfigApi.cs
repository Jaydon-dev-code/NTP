using System.IO;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace SL.MLineDataPrecisionTracking.Client.Http
{
    public class DeviceCollectionConfigApi : BaseHttp
    {
        protected override string _controllerName => "DeviceCollectionConfig";

        /// <summary>
        /// 导入PLC点位信息
        /// </summary>
        /// <param name="filePath">Excel文件路径</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> ImportPlcPointsAsync(string filePath)
        {
            var extension = Path.GetExtension(filePath)?.ToLower();
            if (extension != ".xlsx")
            {
                return ApiResult.Fail("无效的文件格式，只支持.xlsx文件");
            }

            return await PostFileAsync("ImportPlcPoints", filePath);
        }

        /// <summary>
        /// 导入PLC点位信息（直接传递文件流）
        /// </summary>
        /// <param name="stream">Excel文件流</param>
        /// <param name="fileName">文件名</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> ImportPlcPointsAsync(Stream stream, string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLower();
            if (extension != ".xlsx")
            {
                return ApiResult.Fail("无效的文件格式，只支持.xlsx文件");
            }

            return await PostFileAsync("ImportPlcPoints", stream, fileName);
        }
    }
}