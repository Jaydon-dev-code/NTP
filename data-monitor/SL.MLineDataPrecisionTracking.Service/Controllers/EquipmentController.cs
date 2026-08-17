using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    /// <summary>
    /// 设备管理控制器 — 厂 / 产线 / 设备 三级归属关系管理
    /// </summary>
    public class EquipmentController : ApiController
    {
        private readonly EquipmentManagementService _equipmentService;

        public EquipmentController(EquipmentManagementService equipmentService)
        {
            _equipmentService = equipmentService;
        }

        #region 厂（Factory）

        [HttpPost]
        public async Task<ApiResult> AddFactory([FromBody] Tb_Factory factory)
        {
            try
            {
                return await _equipmentService.AddFactoryAsync(factory);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"添加厂失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> UpdateFactory([FromBody] Tb_Factory factory)
        {
            try
            {
                return await _equipmentService.UpdateFactoryAsync(factory);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"修改厂失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> DeleteFactory([FromBody] int factoryId)
        {
            try
            {
                return await _equipmentService.DeleteFactoryAsync(factoryId);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"删除厂失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult<List<Tb_Factory>>> GetFactories()
        {
            try
            {
                return await _equipmentService.GetFactoriesAsync();
            }
            catch (Exception ex)
            {
                return ApiResult<List<Tb_Factory>>.Fail($"查询厂失败: {ex.Message}");
            }
        }

        #endregion

        #region 产线（ProductionLine）

        [HttpPost]
        public async Task<ApiResult> AddProductionLine([FromBody] Tb_ProductionLine line)
        {
            try
            {
                return await _equipmentService.AddProductionLineAsync(line);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"添加产线失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> UpdateProductionLine([FromBody] Tb_ProductionLine line)
        {
            try
            {
                return await _equipmentService.UpdateProductionLineAsync(line);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"修改产线失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> DeleteProductionLine([FromBody] int lineId)
        {
            try
            {
                return await _equipmentService.DeleteProductionLineAsync(lineId);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"删除产线失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult<List<Tb_ProductionLine>>> GetProductionLines(
            [FromBody] int? factoryId
        )
        {
            try
            {
                return await _equipmentService.GetProductionLinesAsync(factoryId);
            }
            catch (Exception ex)
            {
                return ApiResult<List<Tb_ProductionLine>>.Fail($"查询产线失败: {ex.Message}");
            }
        }

        #endregion

        #region 设备（Equipment）

        [HttpPost]
        public async Task<ApiResult> AddEquipment([FromBody] Tb_Equipment equipment)
        {
            try
            {
                return await _equipmentService.AddEquipmentAsync(equipment);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"添加设备失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> UpdateEquipment([FromBody] Tb_Equipment equipment)
        {
            try
            {
                return await _equipmentService.UpdateEquipmentAsync(equipment);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"修改设备失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult> DeleteEquipment([FromBody] string equipmentId)
        {
            try
            {
                return await _equipmentService.DeleteEquipmentAsync(equipmentId);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"删除设备失败: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ApiResult<List<Tb_Equipment>>> GetEquipments(
            [FromBody] EquipmentQueryDto query
        )
        {
            try
            {
                return await _equipmentService.GetEquipmentsAsync(query?.FactoryId, query?.LineId);
            }
            catch (Exception ex)
            {
                return ApiResult<List<Tb_Equipment>>.Fail($"查询设备失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 将设备加入产线
        /// </summary>
        [HttpPost]
        public async Task<ApiResult> AddEquipmentToLine([FromBody] EquipmentLineDto dto)
        {
            try
            {
                return await _equipmentService.AddEquipmentToLineAsync(
                    dto.EquipmentId,
                    dto.LineId
                );
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"设备入线失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 将设备移出产线
        /// </summary>
        [HttpPost]
        public async Task<ApiResult> RemoveEquipmentFromLine([FromBody] string equipmentId)
        {
            try
            {
                return await _equipmentService.RemoveEquipmentFromLineAsync(equipmentId);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"设备移出产线失败: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// 设备查询条件
    /// </summary>
    public class EquipmentQueryDto
    {
        public int? FactoryId { get; set; }
        public int? LineId { get; set; }
    }

    /// <summary>
    /// 设备入线参数
    /// </summary>
    public class EquipmentLineDto
    {
        public string EquipmentId { get; set; }
        public int LineId { get; set; }
    }
}
