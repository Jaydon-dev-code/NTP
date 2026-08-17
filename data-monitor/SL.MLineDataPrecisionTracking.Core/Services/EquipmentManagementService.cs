using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    /// <summary>
    /// 设备管理服务：管理 厂 → 产线 → 设备 三级归属关系。
    /// 设备（Tb_Equipment）与产线相互独立，可先添加设备，再创建产线，随后将设备加入产线。
    /// </summary>
    public class EquipmentManagementService
    {
        private readonly Tb_FactoryRepository _factoryRepository;
        private readonly Tb_ProductionLineRepository _productionLineRepository;
        private readonly Tb_EquipmentRepository _equipmentRepository;
        private readonly Tb_PlcConnectionRepository _plcConnectionRepository;
        private readonly Tb_PlcPointRepository _plcPointRepository;

        public EquipmentManagementService(
            Tb_FactoryRepository factoryRepository,
            Tb_ProductionLineRepository productionLineRepository,
            Tb_EquipmentRepository equipmentRepository,
            Tb_PlcConnectionRepository plcConnectionRepository,
            Tb_PlcPointRepository plcPointRepository
        )
        {
            _factoryRepository = factoryRepository;
            _productionLineRepository = productionLineRepository;
            _equipmentRepository = equipmentRepository;
            _plcConnectionRepository = plcConnectionRepository;
            _plcPointRepository = plcPointRepository;
        }

        #region 厂（Factory）

        /// <summary>
        /// 新增厂
        /// </summary>
        public async Task<ApiResult> AddFactoryAsync(Tb_Factory factory)
        {
            if (factory == null || string.IsNullOrWhiteSpace(factory.FactoryCode))
                return ApiResult.Fail("厂编码不能为空");

            if (string.IsNullOrWhiteSpace(factory.FactoryName))
                return ApiResult.Fail("厂名称不能为空");

            var exists = await _factoryRepository.AnyAsync(x =>
                x.FactoryCode == factory.FactoryCode
            );
            if (exists)
                return ApiResult.Fail($"厂编码 \"{factory.FactoryCode}\" 已存在");

            factory.CreateTime = DateTime.Now;
            await _factoryRepository.InsertAsync(factory);
            return ApiResult.Success($"厂 \"{factory.FactoryName}\" 添加成功");
        }

        /// <summary>
        /// 修改厂
        /// </summary>
        public async Task<ApiResult> UpdateFactoryAsync(Tb_Factory factory)
        {
            if (factory == null || factory.Id <= 0)
                return ApiResult.Fail("厂Id无效");

            if (string.IsNullOrWhiteSpace(factory.FactoryCode))
                return ApiResult.Fail("厂编码不能为空");

            if (string.IsNullOrWhiteSpace(factory.FactoryName))
                return ApiResult.Fail("厂名称不能为空");

            var exists = await _factoryRepository.AnyAsync(x =>
                x.FactoryCode == factory.FactoryCode && x.Id != factory.Id
            );
            if (exists)
                return ApiResult.Fail($"厂编码 \"{factory.FactoryCode}\" 已存在");

            await _factoryRepository.UpdateAsync(factory);
            return ApiResult.Success("修改成功");
        }

        /// <summary>
        /// 删除厂（存在产线时不允许删除）
        /// </summary>
        public async Task<ApiResult> DeleteFactoryAsync(int factoryId)
        {
            var hasLine = await _productionLineRepository.AnyAsync(x =>
                x.FactoryId == factoryId
            );
            if (hasLine)
                return ApiResult.Fail("该厂下存在产线，无法删除");

            await _factoryRepository.DeleteAsync(x => x.Id == factoryId);
            return ApiResult.Success("删除成功");
        }

        /// <summary>
        /// 查询全部厂（含产线）
        /// </summary>
        public async Task<ApiResult<List<Tb_Factory>>> GetFactoriesAsync()
        {
            var list = await _factoryRepository.GetAllWithLinesAsync();
            return ApiResult<List<Tb_Factory>>.Success(list);
        }

        #endregion

        #region 产线（ProductionLine）

        /// <summary>
        /// 新增产线
        /// </summary>
        public async Task<ApiResult> AddProductionLineAsync(Tb_ProductionLine line)
        {
            if (line == null || string.IsNullOrWhiteSpace(line.LineCode))
                return ApiResult.Fail("产线编码不能为空");

            if (string.IsNullOrWhiteSpace(line.LineName))
                return ApiResult.Fail("产线名称不能为空");

            var factory = await _factoryRepository.GetFirstAsync(x => x.Id == line.FactoryId);
            if (factory == null)
                return ApiResult.Fail("所属厂不存在");

            var exists = await _productionLineRepository.AnyAsync(x =>
                x.FactoryId == line.FactoryId && x.LineCode == line.LineCode
            );
            if (exists)
                return ApiResult.Fail(
                    $"厂 \"{factory.FactoryName}\" 下产线编码 \"{line.LineCode}\" 已存在"
                );

            line.CreateTime = DateTime.Now;
            await _productionLineRepository.InsertAsync(line);
            return ApiResult.Success($"产线 \"{line.LineName}\" 添加成功");
        }

        /// <summary>
        /// 修改产线
        /// </summary>
        public async Task<ApiResult> UpdateProductionLineAsync(Tb_ProductionLine line)
        {
            if (line == null || line.Id <= 0)
                return ApiResult.Fail("产线Id无效");

            if (string.IsNullOrWhiteSpace(line.LineCode))
                return ApiResult.Fail("产线编码不能为空");

            if (string.IsNullOrWhiteSpace(line.LineName))
                return ApiResult.Fail("产线名称不能为空");

            var factory = await _factoryRepository.GetFirstAsync(x => x.Id == line.FactoryId);
            if (factory == null)
                return ApiResult.Fail("所属厂不存在");

            var exists = await _productionLineRepository.AnyAsync(x =>
                x.FactoryId == line.FactoryId && x.LineCode == line.LineCode && x.Id != line.Id
            );
            if (exists)
                return ApiResult.Fail(
                    $"厂 \"{factory.FactoryName}\" 下产线编码 \"{line.LineCode}\" 已存在"
                );

            await _productionLineRepository.UpdateAsync(line);
            return ApiResult.Success("修改成功");
        }

        /// <summary>
        /// 删除产线（先解除产线下设备归属）
        /// </summary>
        public async Task<ApiResult> DeleteProductionLineAsync(int lineId)
        {
            var equipments = await _equipmentRepository.GetListWithLineAsync();
            foreach (var equipment in equipments.Where(e => e.LineId == lineId))
            {
                await _equipmentRepository.UpdateLineAsync(equipment.EquipmentId, null);
            }

            await _productionLineRepository.DeleteAsync(x => x.Id == lineId);
            return ApiResult.Success("删除成功");
        }

        /// <summary>
        /// 查询全部产线（含厂、设备）
        /// </summary>
        public async Task<ApiResult<List<Tb_ProductionLine>>> GetProductionLinesAsync(
            int? factoryId = null
        )
        {
            var list = await _productionLineRepository.GetListWithFactoryAsync();
            if (factoryId.HasValue)
                list = list.Where(l => l.FactoryId == factoryId.Value).ToList();
            return ApiResult<List<Tb_ProductionLine>>.Success(list);
        }

        #endregion

        #region 设备（Equipment）

        /// <summary>
        /// 新增设备（独立存在，可不立即挂产线）
        /// </summary>
        public async Task<ApiResult> AddEquipmentAsync(Tb_Equipment equipment)
        {
            if (equipment == null || string.IsNullOrWhiteSpace(equipment.DeviceName))
                return ApiResult.Fail("设备名称不能为空");

            if (string.IsNullOrWhiteSpace(equipment.EquipmentId))
                return ApiResult.Fail("设备编号不能为空");

            var exists = await _equipmentRepository.QueryableFirstAsync(x =>
                x.EquipmentId == equipment.EquipmentId
            );
            if (exists != null)
                return ApiResult.Fail($"设备编号 \"{equipment.EquipmentId}\" 已存在");

            if (equipment.LineId.HasValue)
            {
                var line = await _productionLineRepository.GetFirstAsync(x =>
                    x.Id == equipment.LineId.Value
                );
                if (line == null)
                    return ApiResult.Fail("归属产线不存在");
            }

            equipment.CreateTime = DateTime.Now;
            await _equipmentRepository.ExecuteReturnIdentityAsync(equipment);
            return ApiResult.Success($"设备 \"{equipment.DeviceName}\" 添加成功");
        }

        /// <summary>
        /// 修改设备
        /// </summary>
        public async Task<ApiResult> UpdateEquipmentAsync(Tb_Equipment equipment)
        {
            if (equipment == null || equipment.Id <= 0)
                return ApiResult.Fail("设备Id无效");

            if (string.IsNullOrWhiteSpace(equipment.DeviceName))
                return ApiResult.Fail("设备名称不能为空");

            if (string.IsNullOrWhiteSpace(equipment.EquipmentId))
                return ApiResult.Fail("设备编号不能为空");

            var exists = await _equipmentRepository.QueryableFirstAsync(x =>
                x.EquipmentId == equipment.EquipmentId && x.Id != equipment.Id
            );
            if (exists != null)
                return ApiResult.Fail($"设备编号 \"{equipment.EquipmentId}\" 已存在");

            if (equipment.LineId.HasValue)
            {
                var line = await _productionLineRepository.GetFirstAsync(x =>
                    x.Id == equipment.LineId.Value
                );
                if (line == null)
                    return ApiResult.Fail("归属产线不存在");
            }

            await _equipmentRepository.UpdateAsync(equipment);
            return ApiResult.Success("修改成功");
        }

        /// <summary>
        /// 删除设备（连带删除其PLC连接与点位），按设备编号
        /// </summary>
        public async Task<ApiResult> DeleteEquipmentAsync(string equipmentId)
        {
            var equipment = await _equipmentRepository.GetEquipmentAllAsync(x =>
                x.EquipmentId == equipmentId
            );
            if (equipment == null)
                return ApiResult.Fail("设备不存在");

            if (equipment.PlcConnections != null)
            {
                foreach (var plc in equipment.PlcConnections)
                {
                    await _plcPointRepository.DeleteableAsync(p => p.PlcConnectionId == plc.Id);
                }
                await _plcConnectionRepository.DeleteableAsync(c => c.EquipmentId == equipment.Id);
            }

            await _equipmentRepository.DeleteableAsync(x => x.EquipmentId == equipmentId);
            return ApiResult.Success("删除成功");
        }

        /// <summary>
        /// 查询全部设备（含归属产线、厂）
        /// </summary>
        public async Task<ApiResult<List<Tb_Equipment>>> GetEquipmentsAsync(
            int? factoryId = null,
            int? lineId = null
        )
        {
            var list = await _equipmentRepository.GetListWithLineAsync();
            if (factoryId.HasValue)
                list = list
                    .Where(e =>
                        e.ProductionLine != null && e.ProductionLine.FactoryId == factoryId.Value
                    )
                    .ToList();
            if (lineId.HasValue)
                list = list.Where(e => e.LineId == lineId.Value).ToList();
            return ApiResult<List<Tb_Equipment>>.Success(list);
        }

        /// <summary>
        /// 将设备加入产线（先创建产线，再把设备加进去），按设备编号
        /// </summary>
        public async Task<ApiResult> AddEquipmentToLineAsync(string equipmentId, int lineId)
        {
            var equipment = await _equipmentRepository.QueryableFirstAsync(x =>
                x.EquipmentId == equipmentId
            );
            if (equipment == null)
                return ApiResult.Fail("设备不存在");

            var line = await _productionLineRepository.GetFirstAsync(x => x.Id == lineId);
            if (line == null)
                return ApiResult.Fail("产线不存在");

            await _equipmentRepository.UpdateLineAsync(equipmentId, lineId);
            return ApiResult.Success($"设备 \"{equipment.DeviceName}\" 已加入产线 \"{line.LineName}\"");
        }

        /// <summary>
        /// 将设备移出产线，按设备编号
        /// </summary>
        public async Task<ApiResult> RemoveEquipmentFromLineAsync(string equipmentId)
        {
            var equipment = await _equipmentRepository.QueryableFirstAsync(x =>
                x.EquipmentId == equipmentId
            );
            if (equipment == null)
                return ApiResult.Fail("设备不存在");

            await _equipmentRepository.UpdateLineAsync(equipmentId, null);
            return ApiResult.Success($"设备 \"{equipment.DeviceName}\" 已移出产线");
        }

        #endregion
    }
}
