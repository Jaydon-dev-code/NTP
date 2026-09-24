using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection;
using SL.MLineDataPrecisionTracking.Core.Services.SPCDataCollection.DataCollection;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage.SPC;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.SPC;

namespace SL.MLineDataPrecisionTracking.Service.Controllers
{
    /// <summary>
    /// SPC 特性库管理 — 产品、零件、设备、特性的增删改查
    /// </summary>
    public class SpcController : ApiController
    {
        private readonly Tb_SPC_ProductRepository _productRepo;
        private readonly Tb_SPC_PartRepository _partRepo;
        private readonly Tb_SPC_EquipmentRepository _equipmentRepo;
        private readonly Tb_SPC_CharacteristicRepository _characteristicRepo;
        private readonly Tb_SPC_CollectionSourceRepository _collectionSourceRepo;
        private readonly Tb_SPC_PlcPointRepository _plcPointRepo;
        private readonly Tb_PlcConnectionRepository _plcConnectionRepo;
        private readonly Tb_PlcPointRepository _plcPointRepoOrig;
        private readonly CurrentModelSyncService _modelSyncService;
        private readonly CurrentModelResolver _modelResolver;

        public SpcController(
            Tb_SPC_ProductRepository productRepo,
            Tb_SPC_PartRepository partRepo,
            Tb_SPC_EquipmentRepository equipmentRepo,
            Tb_SPC_CharacteristicRepository characteristicRepo,
            Tb_SPC_CollectionSourceRepository collectionSourceRepo,
            Tb_SPC_PlcPointRepository plcPointRepo,
            Tb_PlcConnectionRepository plcConnectionRepo,
            Tb_PlcPointRepository plcPointRepoOrig,
            CurrentModelSyncService modelSyncService,
            CurrentModelResolver modelResolver)
        {
            _productRepo = productRepo;
            _partRepo = partRepo;
            _equipmentRepo = equipmentRepo;
            _characteristicRepo = characteristicRepo;
            _collectionSourceRepo = collectionSourceRepo;
            _plcPointRepo = plcPointRepo;
            _plcConnectionRepo = plcConnectionRepo;
            _plcPointRepoOrig = plcPointRepoOrig;
            _modelSyncService = modelSyncService;
            _modelResolver = modelResolver;
        }

        #region 产品

        [HttpPost]
        public async Task<ApiResult<List<Tb_SPC_Product>>> GetProducts()
        {
            return ApiResult<List<Tb_SPC_Product>>.Success(await _productRepo.GetListAsync());
        }

        [HttpPost]
        public async Task<ApiResult<Tb_SPC_Product>> GetProduct([FromBody] int id)
        {
            var entity = await _productRepo.GetByIdAsync(id);
            return entity != null
                ? ApiResult<Tb_SPC_Product>.Success(entity)
                : ApiResult<Tb_SPC_Product>.Fail("产品不存在");
        }

        [HttpPost]
        public async Task<ApiResult> AddProduct([FromBody] Tb_SPC_Product entity)
        {
            try
            {
                entity.CreateTime = DateTime.Now;
                var id = await _productRepo.InsertAsync(entity);
                entity.Id = id;
                return ApiResult.Success("新增成功");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ApiResult> UpdateProduct([FromBody] Tb_SPC_Product entity)
        {
            try
            {
                return await _productRepo.UpdateAsync(entity)
                    ? ApiResult.Success("更新成功")
                    : ApiResult.Fail("更新失败");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ApiResult> DeleteProduct([FromBody] int id)
        {
            return await _productRepo.DeleteAsync(id)
                ? ApiResult.Success("删除成功")
                : ApiResult.Fail("删除失败");
        }

        #endregion

        #region 零件

        [HttpPost]
        public async Task<ApiResult<List<Tb_SPC_Part>>> GetParts()
        {
            return ApiResult<List<Tb_SPC_Part>>.Success(await _partRepo.GetListAsync());
        }

        [HttpPost]
        public async Task<ApiResult<List<Tb_SPC_Part>>> GetPartsByProduct([FromBody] int productId)
        {
            return ApiResult<List<Tb_SPC_Part>>.Success(await _partRepo.GetByProductIdAsync(productId));
        }

        [HttpPost]
        public async Task<ApiResult> AddPart([FromBody] Tb_SPC_Part entity)
        {
            try
            {
                entity.CreateTime = DateTime.Now;
                await _partRepo.InsertAsync(entity);
                return ApiResult.Success("新增成功");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ApiResult> UpdatePart([FromBody] Tb_SPC_Part entity)
        {
            try
            {
                return await _partRepo.UpdateAsync(entity)
                    ? ApiResult.Success("更新成功")
                    : ApiResult.Fail("更新失败");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ApiResult> DeletePart([FromBody] int id)
        {
            return await _partRepo.DeleteAsync(id)
                ? ApiResult.Success("删除成功")
                : ApiResult.Fail("删除失败");
        }

        #endregion

        #region 设备

        [HttpPost]
        public async Task<ApiResult<List<Tb_SPC_Equipment>>> GetEquipments()
        {
            return ApiResult<List<Tb_SPC_Equipment>>.Success(await _equipmentRepo.GetListAsync());
        }

        [HttpPost]
        public async Task<ApiResult> AddEquipment([FromBody] Tb_SPC_Equipment entity)
        {
            try
            {
                entity.CreateTime = DateTime.Now;
                await _equipmentRepo.InsertAsync(entity);
                return ApiResult.Success("新增成功");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ApiResult> UpdateEquipment([FromBody] Tb_SPC_Equipment entity)
        {
            try
            {
                return await _equipmentRepo.UpdateAsync(entity)
                    ? ApiResult.Success("更新成功")
                    : ApiResult.Fail("更新失败");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ApiResult> DeleteEquipment([FromBody] int id)
        {
            return await _equipmentRepo.DeleteAsync(id)
                ? ApiResult.Success("删除成功")
                : ApiResult.Fail("删除失败");
        }

        #endregion

        #region 特性

        [HttpPost]
        public async Task<ApiResult<List<Tb_SPC_Characteristic>>> GetCharacteristics()
        {
            return ApiResult<List<Tb_SPC_Characteristic>>.Success(await _characteristicRepo.GetListAsync());
        }

        [HttpPost]
        public async Task<ApiResult> AddCharacteristic([FromBody] Tb_SPC_Characteristic entity)
        {
            try
            {
                entity.CreateTime = DateTime.Now;
                var id = await _characteristicRepo.InsertAsync(entity);

                // 根据数据来源自动创建对应的采集源记录
                if (entity.DataSource == Models.Enum.SPCDataSource.Database)
                {
                    await _collectionSourceRepo.InsertAsync(new Tb_SPC_CollectionSource
                    {
                        CharacteristicId = id,
                        CreateTime = DateTime.Now,
                    });
                }
                else if (entity.DataSource == Models.Enum.SPCDataSource.PLC)
                {
                    await _plcPointRepo.InsertAsync(new Tb_SPC_PlcPoint
                    {
                        CharacteristicId = id,
                        CreateTime = DateTime.Now,
                    });
                }

                return ApiResult.Success("新增成功");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ApiResult> UpdateCharacteristic([FromBody] Tb_SPC_Characteristic entity)
        {
            try
            {
                return await _characteristicRepo.UpdateAsync(entity)
                    ? ApiResult.Success("更新成功")
                    : ApiResult.Fail("更新失败");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ApiResult> DeleteCharacteristic([FromBody] int id)
        {
            // 先删除关联的采集源记录
            await _collectionSourceRepo.DeleteByCharacteristicIdAsync(id);
            await _plcPointRepo.DeleteByCharacteristicIdAsync(id);

            return await _characteristicRepo.DeleteAsync(id)
                ? ApiResult.Success("删除成功")
                : ApiResult.Fail("删除失败");
        }

        #endregion

        #region 数据库采集源

        [HttpPost]
        public async Task<ApiResult<List<Tb_SPC_CollectionSource>>> GetCollectionSources()
        {
            return ApiResult<List<Tb_SPC_CollectionSource>>.Success(await _collectionSourceRepo.GetListAsync());
        }

        [HttpPost]
        public async Task<ApiResult<Tb_SPC_CollectionSource>> GetCollectionSource([FromBody] int characteristicId)
        {
            var entity = await _collectionSourceRepo.GetByCharacteristicIdAsync(characteristicId);
            return entity != null
                ? ApiResult<Tb_SPC_CollectionSource>.Success(entity)
                : ApiResult<Tb_SPC_CollectionSource>.Fail("未找到");
        }

        [HttpPost]
        public async Task<ApiResult> UpdateCollectionSource([FromBody] Tb_SPC_CollectionSource entity)
        {
            try
            {
                return await _collectionSourceRepo.UpdateAsync(entity)
                    ? ApiResult.Success("更新成功")
                    : ApiResult.Fail("更新失败");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        #endregion

        #region PLC 点位

        [HttpPost]
        public async Task<ApiResult<List<Tb_SPC_PlcPoint>>> GetPlcPoints()
        {
            return ApiResult<List<Tb_SPC_PlcPoint>>.Success(await _plcPointRepo.GetListAsync());
        }

        [HttpPost]
        public async Task<ApiResult<Tb_SPC_PlcPoint>> GetPlcPoint([FromBody] int characteristicId)
        {
            var entity = await _plcPointRepo.GetByCharacteristicIdAsync(characteristicId);
            return entity != null
                ? ApiResult<Tb_SPC_PlcPoint>.Success(entity)
                : ApiResult<Tb_SPC_PlcPoint>.Fail("未找到");
        }

        [HttpPost]
        public async Task<ApiResult> UpdatePlcPoint([FromBody] Tb_SPC_PlcPoint entity)
        {
            try
            {
                return await _plcPointRepo.UpdateAsync(entity)
                    ? ApiResult.Success("更新成功")
                    : ApiResult.Fail("更新失败");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 按设备ID查询其下所有 PLC 点位（用于导入选择）。
        /// 复用 Tb_Equipment → Tb_PlcConnection → Tb_PlcPoint。
        /// </summary>
        [HttpPost]
        public async Task<ApiResult<List<Tb_PlcPoint>>> GetEquipmentPlcPoints([FromBody] int equipmentId)
        {
            var connections = await _plcConnectionRepo.QueryableAsync(c => c.EquipmentId == equipmentId);
            if (connections == null || connections.Count == 0)
                return ApiResult<List<Tb_PlcPoint>>.Success(new List<Tb_PlcPoint>());

            var connIds = connections.Select(c => c.Id).ToList();
            var points = await _plcPointRepoOrig.QueryableAsync(p => connIds.Contains(p.PlcConnectionId));
            return ApiResult<List<Tb_PlcPoint>>.Success(points);
        }

        /// <summary>查该特性下所有 PLC 点位</summary>
        [HttpPost]
        public async Task<ApiResult<List<Tb_SPC_PlcPoint>>> GetPlcPointsByCharacteristic([FromBody] int characteristicId)
        {
            return ApiResult<List<Tb_SPC_PlcPoint>>.Success(
                await _plcPointRepo.GetAllByCharacteristicIdAsync(characteristicId));
        }

        /// <summary>批量保存手动录入的 PLC 点位</summary>
        [HttpPost]
        public async Task<ApiResult> BatchInsertPlcPoints([FromBody] List<Tb_SPC_PlcPoint> list)
        {
            try
            {
                if (list == null || list.Count == 0)
                    return ApiResult.Fail("点位列表为空");
                var count = await _plcPointRepo.InsertBatchAsync(list);
                return ApiResult.Success($"成功保存 {count} 个点位");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        /// <summary>删除单个 PLC 点位</summary>
        [HttpPost]
        public async Task<ApiResult> DeletePlcPoint([FromBody] int id)
        {
            try
            {
                return await _plcPointRepo.DeleteAsync(id)
                    ? ApiResult.Success("删除成功")
                    : ApiResult.Fail("删除失败");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Excel 导入 PLC 点位（multipart 上传 .xlsx）。
        /// 列：IP | 端口 | PLC类型 | 变量名 | 区块 | 地址 | 数据类型 | 长度
        /// 解析后批量插入 Tb_SPC_PlcPoint（PlcPointId=0 手动模式）。
        /// </summary>
        [HttpPost]
        public async Task<ApiResult> ImportPlcPoints([FromBody] SpcPlcImportDto dto)
        {
            try
            {
                if (dto?.FileBytes == null || dto.FileBytes.Length == 0)
                    return ApiResult.Fail("未接收到文件");

                if (dto.CharacteristicId <= 0)
                    return ApiResult.Fail("缺少特性ID");

                var bytes = Convert.FromBase64String(dto.FileBytes);
                using (var stream = new System.IO.MemoryStream(bytes))
                {
                    var points = ReadSpcPlcExcel(stream, dto.CharacteristicId);
                    if (points.Count == 0)
                        return ApiResult.Fail("Excel 中未解析到有效点位（需含变量名和区块）");

                    await _plcPointRepo.DeleteByCharacteristicIdAsync(dto.CharacteristicId);
                    var count = await _plcPointRepo.InsertBatchAsync(points);
                    return ApiResult.Success($"成功导入 {count} 个点位");
                }
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"导入失败: {ex.Message}");
            }
        }

        /// <summary>解析 SPC PLC 点位 Excel（8列）</summary>
        private List<Tb_SPC_PlcPoint> ReadSpcPlcExcel(System.IO.Stream stream, int characteristicId)
        {
            var list = new List<Tb_SPC_PlcPoint>();
            var workbook = new NPOI.XSSF.UserModel.XSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;
                int c = 0;
                string ip = row.GetCell(c++)?.ToString()?.Trim();
                int port = int.TryParse(row.GetCell(c++)?.ToString(), out int p) ? p : 8000;
                string plcType = row.GetCell(c++)?.ToString()?.Trim() ?? "三菱";
                string pointName = row.GetCell(c++)?.ToString()?.Trim();
                string area = row.GetCell(c++)?.ToString()?.Trim();
                string address = row.GetCell(c++)?.ToString()?.Trim();
                string dataType = row.GetCell(c++)?.ToString()?.Trim() ?? "Int16";
                int length = int.TryParse(row.GetCell(c++)?.ToString(), out int len) ? len : 1;

                if (string.IsNullOrEmpty(pointName) || string.IsNullOrEmpty(area))
                    continue;

                list.Add(new Tb_SPC_PlcPoint
                {
                    CharacteristicId = characteristicId,
                    PlcPointId = 0,
                    IpAddress = ip,
                    Port = port,
                    PlcType = plcType,
                    PointName = pointName,
                    Area = area,
                    Address = address,
                    DataType = dataType,
                    Length = length,
                });
            }
            return list;
        }

        #endregion

        #region 当前型号

        /// <summary>读取内存中的当前生产型号快照（由三方不间断轮询维护，无本地缓存表）</summary>
        [HttpPost]
        public ApiResult<SPCCurrentModelInfo> GetCurrentModel()
        {
            return ApiResult<SPCCurrentModelInfo>.Success(_modelSyncService.GetCurrentModel());
        }

        /// <summary>强制立即查一次三方库，更新内存型号</summary>
        [HttpPost]
        public async Task<ApiResult> SyncCurrentModelNow()
        {
            try
            {
                if (!ThirdPartyDbConfig.Enabled || !ThirdPartyDbConfig.IsConfigured)
                    return ApiResult.Fail("请先配置 ThirdPartyDb_Enabled、ConnectionString、CurrentModelSql");

                await _modelSyncService.SyncOnceAsync();
                var info = _modelSyncService.GetCurrentModel();
                if (info.SyncStatus == "Ok")
                    return ApiResult.Success($"同步成功: {info.ModelNo}");
                return ApiResult.Fail(info.LastError ?? "同步未生效（请检查连接串/型号 SQL）");
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        /// <summary>干跑解析：按特性 ID 演示 型号→产品→零件 结果</summary>
        [HttpPost]
        public async Task<ApiResult<SPCResolvedContext>> ResolveCharacteristic([FromBody] int characteristicId)
        {
            try
            {
                var ctx = await _modelResolver.ResolveByIdAsync(characteristicId);
                return ApiResult<SPCResolvedContext>.Success(ctx);
            }
            catch (Exception ex)
            {
                return ApiResult<SPCResolvedContext>.Fail(ex.Message);
            }
        }

        #endregion

        #region 采集方式

        /// <summary>
        /// 获取所有可用的采集方式（SPCDataCollectionAbstract 子类）。
        /// 反射扫描程序集，返回类名 + 描述 + 数据来源类型。
        /// </summary>
        [HttpPost]
        public ApiResult<List<CollectionMethodInfo>> GetCollectionMethods()
        {
            var types = typeof(SPCDataCollectionAbstract).Assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract
                    && typeof(SPCDataCollectionAbstract).IsAssignableFrom(t))
                .ToList();

            var list = new List<CollectionMethodInfo>();
            foreach (var t in types)
            {
                // 不调用构造函数创建实例，只读取 DataSource/Description 这两个固定属性
                var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(t) as SPCDataCollectionAbstract;
                if (instance == null) continue;

                list.Add(new CollectionMethodInfo
                {
                    ClassName = t.Name,
                    Description = instance.Description,
                    DataSource = instance.DataSource,
                });
            }
            return ApiResult<List<CollectionMethodInfo>>.Success(list);
        }

        #endregion
    }

    public class CollectionMethodInfo
    {
        public string ClassName { get; set; }
        public string Description { get; set; }
        public SL.MLineDataPrecisionTracking.Models.Enum.SPCDataSource DataSource { get; set; }
    }

    /// <summary>SPC PLC Excel 导入入参（文件以 Base64 传输）</summary>
    public class SpcPlcImportDto
    {
        public int CharacteristicId { get; set; }
        public string FileBytes { get; set; }
    }
}
