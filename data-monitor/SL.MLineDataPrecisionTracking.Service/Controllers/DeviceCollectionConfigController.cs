using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Models.Domain;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    public class DeviceCollectionConfigController : ApiController
    {
        PlcAddressExcelImportService _plcImport;

        public DeviceCollectionConfigController(PlcAddressExcelImportService plcImport)
        {
            _plcImport = plcImport;
        }

        /// <summary>
        /// 导入PLC点位信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> ImportPlcPoints()
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
                    
                    if (!IsValidExcelFile(fileName))
                    {
                        return ApiResult.Fail("无效的文件格式，只支持.xlsx文件");
                    }

                    using (var stream = await content.ReadAsStreamAsync())
                    {
                        var result = await _plcImport.ImportPlcPointsAsync(stream);
                        if (result)
                        {
                            return ApiResult.Success("导入成功");
                        }
                        else
                        {
                            return ApiResult.Fail("导入失败");
                        }
                    }
                }

                return ApiResult.Fail("未找到上传的文件");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"导入失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 验证Excel文件
        /// </summary>
        private bool IsValidExcelFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var extension = Path.GetExtension(fileName)?.ToLower();
            return extension == ".xlsx";
        }
    }
}