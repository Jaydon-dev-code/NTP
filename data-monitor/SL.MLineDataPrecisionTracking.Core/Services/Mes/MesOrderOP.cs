using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;
using SL.MLineDataPrecisionTracking.Models.Entities.Mes;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Core.Services.Mes
{
    /// <summary>
    /// MES 工单操作：接口一/二落库、状态切换、执行中工单的内存快照。
    /// 内存快照采用不可变对象整体替换（线程安全），
    /// 所有影响执行中工单的变更路径（SetStatus/Update删除）统一刷新快照，
    /// 供推送服务免查库直接取工单号。
    /// 订单型号（OrderPieceType）来自型号专用库（ModelDb，非本库订单字段），内存缓存+TTL刷新。
    /// </summary>
    public class MesOrderOP
    {
        private readonly Tb_MesOrderRepository _mesOrderRepository;
        private readonly ModelDb _modelDb;

        /// <summary>型号库当前型号缓存有效期</summary>
        static readonly TimeSpan ModelCacheTtl = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 执行中工单内存快照（不可变，整体替换保证线程安全）
        /// </summary>
        private sealed class ExecutingOrderSnapshot
        {
            public ExecutingOrderSnapshot(string? orderNo, string materialCode, string materialName)
            {
                OrderNo = orderNo;
                MaterialCode = materialCode ?? string.Empty;
                MaterialName = materialName ?? string.Empty;
            }

            public string OrderNo { get; }
            public string MaterialCode { get; }
            public string MaterialName { get; }
        }

        /// <summary>当前快照（null=无执行中工单；引用赋值原子，读侧无锁）</summary>
        private volatile ExecutingOrderSnapshot _snapshot;

        /// <summary>型号库当前型号缓存（volatile 整体替换）</summary>
        private volatile string _thirdPartyModel = string.Empty;

        /// <summary>型号缓存过期时间（ticks；volatile 不支持 DateTime，用 long）</summary>
        private long _modelCacheExpiresAtTicks;

        /// <summary>后台刷新型号中标志（避免并发刷新）</summary>
        private int _modelRefreshing;

        /// <summary>执行中工单号（无执行中工单为空串）</summary>
        public string? OrderNo => _snapshot?.OrderNo;

        /// <summary>
        /// 当前生产型号（型号库「产品型号_当前产品型号表」最新型号名称；带10秒内存缓存）。
        /// 注意：不是订单的成品料号，型号校验服务直接查库对比，此处供推送/展示取用。
        /// </summary>
        public string OrderPieceType
        {
            get
            {
                if (DateTime.UtcNow.Ticks >= Interlocked.Read(ref _modelCacheExpiresAtTicks))
                {
                    // 过期 → 后台异步刷新，本次先返回旧值（无锁、不阻塞推送线程）
                    if (Interlocked.CompareExchange(ref _modelRefreshing, 1, 0) == 0)
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                await RefreshThirdPartyModelAsync();
                            }
                            finally
                            {
                                Interlocked.Exchange(ref _modelRefreshing, 0);
                            }
                        });
                    }
                }
                return _thirdPartyModel;
            }
        }

        /// <summary>执行中订单物料描述（MaterialName）</summary>
        public string OrderMaterialName => _snapshot?.MaterialName ?? string.Empty;

        public MesOrderOP(
            Tb_MesOrderRepository mesOrderRepository,
            ModelDb modelDb
        )
        {
            _mesOrderRepository = mesOrderRepository;
            _modelDb = modelDb;
            try
            {
                RefreshExecutingSnapshotAsync().GetAwaiter().GetResult();
                RefreshThirdPartyModelAsync().GetAwaiter().GetResult();
            }
            catch
            {
                _snapshot = null;
            }
        }

        /// <summary>
        /// 从型号库刷新当前型号缓存（TTL 过期后由 OrderPieceType 读取触发）。
        /// 查询失败保留旧值，仅延长缓存期避免刷屏重试。
        /// </summary>
        private async Task RefreshThirdPartyModelAsync()
        {
            var model = await _modelDb.GetCurrentModelAsync();
            if (!string.IsNullOrEmpty(model))
            {
                _thirdPartyModel = model;
            }
            Interlocked.Exchange(
                ref _modelCacheExpiresAtTicks,
                DateTime.UtcNow.Add(ModelCacheTtl).Ticks
            );
        }

        /// <summary>
        /// 从数据库刷新执行中工单快照（所有变更路径统一调用）。
        /// 无执行中工单时快照置 null，避免残留旧值。
        /// </summary>
        private async Task RefreshExecutingSnapshotAsync()
        {
            var executing = await _mesOrderRepository.GetExecutingAsync();
            _snapshot =
                executing == null
                    ? null
                    : new ExecutingOrderSnapshot(
                        executing.WorkOrderNo,
                        executing.MaterialCode,
                        executing.MaterialName
                    );
        }

        /// <summary>
        /// 接口一：生产工单新增
        /// </summary>
        public async Task<ApiResult> Release(MesOrderReleaseDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.WorkOrderNo))
            {
                return new ApiResult
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = "工单号不能为空",
                };
            }

            var exists = await _mesOrderRepository.GetByWorkOrderNoAsync(dto.WorkOrderNo);
            if (exists != null)
            {
                return new ApiResult
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = "工单已存在",
                };
            }

            var order = new Tb_MesOrder
            {
                WorkOrderNo = dto.WorkOrderNo,
                MaterialCode = dto.MaterialCode,
                MaterialName = dto.MaterialName,
                PlanQty = dto.PlanQty,
                PlanDate = dto.PlanDate,
                Status = (int)MesOrderStatusEnum.Pending,
                CreateTime = DateTime.Now,
            };

            var orderId = await _mesOrderRepository.InsertAsync(order);

            if (dto.OrderBoms != null && dto.OrderBoms.Any())
            {
                var boms = dto
                    .OrderBoms.Select(b => new Tb_MesOrderBom
                    {
                        MesOrderId = orderId,
                        ComponentCode = b.ComponentCode,
                        ComponentName = b.ComponentName,
                        Qty = b.Qty,
                        CreateTime = DateTime.Now,
                    })
                    .ToList();
                await _mesOrderRepository.InsertBomsAsync(boms);
            }

            return new ApiResult
            {
                Code = 200,
                IsSuccess = true,
                Message = "OK",
            };
        }

        /// <summary>
        /// 接口二：生产工单更新（修改计划数量或删除工单）
        /// </summary>
        public async Task<ApiResult> Update(MesOrderUpdateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.WorkOrderNo))
            {
                return new ApiResult
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = "工单号不能为空",
                };
            }

            var exists = await _mesOrderRepository.GetByWorkOrderNoAsync(dto.WorkOrderNo);
            if (exists == null)
            {
                return new ApiResult
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = "工单不存在",
                };
            }

            if (dto.Deleted)
            {
                await _mesOrderRepository.DeleteByWorkOrderNoAsync(dto.WorkOrderNo);
                // 删除的可能是执行中工单 → 刷新快照防止残留
                await RefreshExecutingSnapshotAsync();
            }
            else
            {
                await _mesOrderRepository.UpdatePlanQtyAsync(dto.WorkOrderNo, dto.PlanQty);
            }

            return new ApiResult
            {
                Code = 200,
                IsSuccess = true,
                Message = "OK",
            };
        }

        /// <summary>
        /// 查询工单列表（纯查询，无副作用）
        /// </summary>
        public async Task<ApiResult<List<Tb_MesOrder>>> Query()
        {
            var re = await _mesOrderRepository.GetListAsync();
            return ApiResult<List<Tb_MesOrder>>.Success(re);
        }

        /// <summary>
        /// 订单状态切换（0待执行 / 1执行中）。
        /// 切换后统一刷新内存快照，保证推送服务立即读到新工单。
        /// </summary>
        public async Task<ApiResult> SetStatus(MesOrderStatusDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.WorkOrderNo))
            {
                return new ApiResult
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = "工单号不能为空",
                };
            }

            if (
                dto.Status != (int)MesOrderStatusEnum.Executing
                && dto.Status != (int)MesOrderStatusEnum.Pending
            )
            {
                return new ApiResult
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = "状态值无效",
                };
            }

            var exists = await _mesOrderRepository.GetByWorkOrderNoAsync(dto.WorkOrderNo);
            if (exists == null)
            {
                return new ApiResult
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = "工单不存在",
                };
            }

            // 设为执行中时，先把其他执行中的工单改回待执行（同一时间只允许一个执行中）
            if (dto.Status == (int)MesOrderStatusEnum.Executing)
            {
                await _mesOrderRepository.ResetOtherExecutingAsync(dto.WorkOrderNo);
            }

            await _mesOrderRepository.SetStatusAsync(
                dto.WorkOrderNo,
                (MesOrderStatusEnum)dto.Status
            );

            // 状态变更后刷新内存快照（切为待执行时快照置 null）
            await RefreshExecutingSnapshotAsync();

            return new ApiResult
            {
                Code = 200,
                IsSuccess = true,
                Message = "OK",
            };
        }
    }
}
