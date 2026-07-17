using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    public class EnergyRangeExcelImportService
    {
        private readonly Tb_EnergyRangeRepository _energyRangeRepository;
        private readonly Tb_EnergyRangeDetailRepository _energyRangeDetailRepository;

        public EnergyRangeExcelImportService(
            Tb_EnergyRangeRepository energyRangeRepository,
            Tb_EnergyRangeDetailRepository energyRangeDetailRepository)
        {
            _energyRangeRepository = energyRangeRepository;
            _energyRangeDetailRepository = energyRangeDetailRepository;
        }

        public async Task<ApiResult> ImportAsync(Stream excelStream, bool overwrite = false)
        {
            try
            {
                IWorkbook workbook = new XSSFWorkbook(excelStream);
                ISheet sheet = workbook.GetSheetAt(0);

                IRow productModelRow = sheet.GetRow(0);
                if (productModelRow == null)
                    return ApiResult.Fail("Excel文件为空");

                string productModel = productModelRow.GetCell(1)?.ToString()?.Trim();
                if (string.IsNullOrEmpty(productModel))
                    return ApiResult.Fail("未找到产品型号（B1单元格）");

                var details = new List<Tb_EnergyRangeDetail>();
                for (int i = 2; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue;

                    var timeCell = row.GetCell(0)?.ToString()?.Trim();
                    var lowerCell = row.GetCell(1)?.ToString()?.Trim();
                    var upperCell = row.GetCell(2)?.ToString()?.Trim();

                    if (string.IsNullOrEmpty(timeCell) && string.IsNullOrEmpty(lowerCell) && string.IsNullOrEmpty(upperCell))
                        continue;

                    if (!int.TryParse(timeCell, out int time) || time < 0)
                        return ApiResult.Fail($"第{i + 1}行：时间必须为大于等于0的整数");

                    if (!decimal.TryParse(lowerCell, out decimal lowerLimit) || lowerLimit <= 0)
                        return ApiResult.Fail($"第{i + 1}行：下限值必须为正数");

                    if (Math.Round(lowerLimit, 2) != lowerLimit)
                        return ApiResult.Fail($"第{i + 1}行：下限值最多两位小数");

                    if (!decimal.TryParse(upperCell, out decimal upperLimit) || upperLimit <= 0)
                        return ApiResult.Fail($"第{i + 1}行：上限值必须为正数");

                    if (Math.Round(upperLimit, 2) != upperLimit)
                        return ApiResult.Fail($"第{i + 1}行：上限值最多两位小数");

                    details.Add(new Tb_EnergyRangeDetail
                    {
                        Time = time,
                        LowerLimit = lowerLimit,
                        UpperLimit = upperLimit
                    });
                }

                if (details.Count == 0)
                    return ApiResult.Fail("未找到有效数据行");

                var existing = await _energyRangeRepository.GetFirstAsync(x => x.ProductModel == productModel);
                if (existing != null)
                {
                    if (!overwrite)
                        return ApiResult.Fail($"产品型号 \"{productModel}\" 已存在，是否覆盖？");

                    await _energyRangeDetailRepository.DeleteAsync(x => x.EnergyRangeId == existing.Id);
         
                    existing.CreateTime = DateTime.Now;
                    await _energyRangeRepository.UpdateAsync(existing);

                    foreach (var d in details)
                        d.EnergyRangeId = existing.Id;
                    await _energyRangeDetailRepository.InsertAsync(details);
                }
                else
                {
                    var entity = new Tb_EnergyRange
                    {
                        ProductModel = productModel,
                        CreateTime = DateTime.Now
                    };
                    int id = await _energyRangeRepository.InsertAsync(entity);

                    foreach (var d in details)
                        d.EnergyRangeId = id;
                    await _energyRangeDetailRepository.InsertAsync(details);
                }

                return ApiResult.Success($"产品型号 \"{productModel}\" 导入成功，共 {details.Count} 条数据");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"导入失败：{ex.Message}");
            }
        }

        public async Task<List<Tb_EnergyRange>> GetAllAsync()
        {
            return await _energyRangeRepository.GetAllWithDetailsAsync();
        }

  

        public async Task DeleteNavAsync(Expression<Func<Tb_EnergyRange, bool>> whereExpression)
        {
            await _energyRangeRepository.DeleteNavAsync(whereExpression);
        }

    

        public async Task SetCurrentStationModelAsync(int id, string station)
        {
            var all = await _energyRangeRepository.GetListAsync();
            foreach (var item in all)
            {
                if (item.Id == id)
                    item.Station = station;
                else if (item.Station == station)
                    item.Station = null;
                await _energyRangeRepository.UpdateAsync(item);
            }
        }
    }
}
