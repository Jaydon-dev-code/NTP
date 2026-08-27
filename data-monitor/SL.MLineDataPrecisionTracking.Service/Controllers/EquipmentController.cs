using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection;
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
        private readonly StationCollectionManager _stationCollectionManager;

        public EquipmentController(EquipmentManagementService equipmentService, StationCollectionManager stationCollectionManager)
        {
            _equipmentService = equipmentService;
            _stationCollectionManager = stationCollectionManager;
        }

        #region 采集注册状态检查辅助方法

        /// <summary>
        /// 检查单个设备是否已注册采集
        /// </summary>
        private async Task<bool> IsEquipmentRegisteredAsync(string equipmentId)
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
                return false;

            var result = await _stationCollectionManager.GetRegisteredAsync(equipmentId);
            var isRegistered = result.IsSuccess && result.Data != null;
            
            if (isRegistered)
            {
                Serilog.Log.Warning("设备 {EquipmentId} 已注册采集，请先注销后再操作", equipmentId);
            }
            
            return isRegistered;
        }

        /// <summary>
        /// 检查指定厂下是否有已注册采集的设备
        /// </summary>
        private async Task<bool> HasRegisteredEquipmentInFactoryAsync(int factoryId)
        {
            var allStations = await _stationCollectionManager.GetAllRegisteredAsync();
            if (!allStations.IsSuccess || allStations.Data == null)
                return false;

            var hasRegistered = allStations.Data.Any(s => s.FactoryId == factoryId);
            
            if (hasRegistered)
            {
                Serilog.Log.Warning("厂 {FactoryId} 下存在已注册采集的设备，请先注销后再操作", factoryId);
            }
            
            return hasRegistered;
        }

        /// <summary>
        /// 检查指定产线下是否有已注册采集的设备
        /// </summary>
        private async Task<bool> HasRegisteredEquipmentInLineAsync(int lineId)
        {
            var allStations = await _stationCollectionManager.GetAllRegisteredAsync();
            if (!allStations.IsSuccess || allStations.Data == null)
                return false;

            var hasRegistered = allStations.Data.Any(s => s.LineId == lineId);
            
            if (hasRegistered)
            {
                Serilog.Log.Warning("产线 {LineId} 下存在已注册采集的设备，请先注销后再操作", lineId);
            }
            
            return hasRegistered;
        }

        #endregion

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
                if (await HasRegisteredEquipmentInFactoryAsync(factory.Id))
                    return ApiResult.Fail("该厂下存在已注册采集的设备，请先注销后再操作");

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
                if (await HasRegisteredEquipmentInFactoryAsync(factoryId))
                    return ApiResult.Fail("该厂下存在已注册采集的设备，请先注销后再操作");

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
                if (await HasRegisteredEquipmentInLineAsync(line.Id))
                    return ApiResult.Fail("该产线下存在已注册采集的设备，请先注销后再操作");

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
                if (await HasRegisteredEquipmentInLineAsync(lineId))
                    return ApiResult.Fail("该产线下存在已注册采集的设备，请先注销后再操作");

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
                if (await IsEquipmentRegisteredAsync(equipment.EquipmentId))
                    return ApiResult.Fail("设备已注册采集，请先注销后再操作");

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
                if (await IsEquipmentRegisteredAsync(equipmentId))
                    return ApiResult.Fail("设备已注册采集，请先注销后再操作");

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
                if (await IsEquipmentRegisteredAsync(dto.EquipmentId))
                    return ApiResult.Fail("设备已注册采集，请先注销后再操作");

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
                if (await IsEquipmentRegisteredAsync(equipmentId))
                    return ApiResult.Fail("设备已注册采集，请先注销后再操作");

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
