using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using SL.MLineDataPrecisionTracking.Core.Hubs;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;
using SL.MLineDataPrecisionTracking.Models.Entities.Mes;
using SL.MLineDataPrecisionTracking.Models.Enum;
using IClientProxy = Microsoft.AspNet.SignalR.Hubs.IClientProxy;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Line2
{
    /// <summary>
    /// 6分厂2号线型号校验服务（全自动，无前台按钮、无冷却）：
    /// 每轮对比型号库当前产品型号与执行中订单成品料号；
    /// 不一致 → 落异常记录（开始时间=发现时刻）+ SignalR 推送前台告警（MesModelMismatch），
    ///           等待期间每轮重推（前端离线错过也能收到）；
    /// 无执行中订单 或 型号恢复一致 → 回写结束时间（结束时间=恢复时刻）+ 推送 MesModelMatched 关闭前台告警窗。
    /// </summary>
    public class Factory6Line2ModelCheckService : DataCollectionServiceAbstract
    {
        protected override string _serviceName => "6分厂2号线型号校验";

        /// <summary>SignalR 告警事件名（前台订阅：型号不一致）</summary>
        public const string MismatchEventName = "MesModelMismatch";

        /// <summary>SignalR 事件名（前台订阅：型号恢复一致/无执行中订单，用于关闭告警窗）</summary>
        public const string MatchedEventName = "MesModelMatched";

        /// <summary>
        /// 校验状态机：Normal 正常检测 → Alerted 告警中（持续重推）→ 恢复条件满足自动回 Normal
        /// </summary>
        private enum CheckState
        {
            Normal,
            Alerted,
        }

        private readonly ModelDb _modelDb;
        private readonly Tb_MesOrderRepository _orderRepository;
        private readonly Tb_MesModelAbnormalRecordRepository _recordRepository;
        private readonly IHubContext _chatHub;

        private readonly object _stateLock = new object();
        private CheckState _state = CheckState.Normal;

        /// <summary>当前告警对应的异常记录ID（_stateLock 保护）</summary>
        private int _currentRecordId;

        /// <summary>最近一次告警报文（_stateLock 保护；用于告警期间每轮重推）</summary>
        private MesModelMismatchPushDto _lastAlert;

        public Factory6Line2ModelCheckService(
            ModelDb modelDb,
            Tb_MesOrderRepository orderRepository,
            Tb_MesModelAbnormalRecordRepository recordRepository,
            IHubContext chatHub
        )
        {
            _modelDb = modelDb;
            _orderRepository = orderRepository;
            _recordRepository = recordRepository;
            _chatHub = chatHub;
        }

        protected override Task<Result> InitAsync()
        {
            var intervalStr = ConfigurationManager.AppSettings["ModelCheckIntervalSeconds"];
            if (int.TryParse(intervalStr, out var seconds) && seconds > 0)
            {
                _sleepTimeSpan = TimeSpan.FromSeconds(seconds);
            }
            else
            {
                _sleepTimeSpan = TimeSpan.FromSeconds(5);
            }

            if (_modelDb.Db == null)
            {
                return Task.FromResult(Result.Fail("未配置型号库连接（ModelDbConnection）"));
            }
            return Task.FromResult(Result.Success());
        }

        protected override Task<Result> HandshakeAsync()
        {
            return Task.FromResult(Result.Success());
        }

        protected override async Task<Result<object>> InteractAsync()
        {
            // 查型号库当前产品型号（当前产品型号表 关联 产品型号表 → 产品型号名称）
            var thirdPartyModel = await _modelDb.GetCurrentModelAsync();

            // 查执行中订单
            var order = await _orderRepository.GetExecutingAsync();

            // ── 恢复条件：无执行中订单 或 型号一致 ──
            bool noOrder = order == null||string.IsNullOrEmpty(thirdPartyModel);
            bool modelMatch =
                !noOrder
                && !string.IsNullOrEmpty(thirdPartyModel)
                && string.Equals(
                    thirdPartyModel,
                    order.MaterialCode?.Trim(),
                    StringComparison.OrdinalIgnoreCase
                );

            if (noOrder || modelMatch)
            {
                MesModelMismatchPushDto closed = null;
                lock (_stateLock)
                {
                    if (_state == CheckState.Alerted)
                    {
                        // 告警中且恢复条件满足 → 回写结束时间 + 通知前台关窗
                        _state = CheckState.Normal;
                        closed = _lastAlert;
                        _lastAlert = null;
                    }
                }

                if (closed != null)
                {
                    var now = DateTime.Now;
                    await _recordRepository.UpdateEndTimeAsync(closed.RecordId, now);
                    Serilog.Log.Information(
                        "[型号校验]【{_serverName}】告警结束（{Reason}），记录ID={Id} 回写结束时间",
                        _serviceName,
                        noOrder ? "无执行中订单" : "型号恢复一致",
                        closed.RecordId
                    );
                    try
                    {
                        await ((IClientProxy)_chatHub.Clients.All).Invoke(
                            MatchedEventName,
                            new MesModelMatchedPushDto
                            {
                                Model = thirdPartyModel,
                                MatchTime = now,
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Warning($"[型号校验] SignalR 推送异常：{ex.Message}");
                    }
                }
                return Result<object>.Fail(noOrder ? "无执行中订单" : "型号一致");
            }

            // ── 仍不一致 ──
            MesModelMismatchPushDto repush;
            bool isFirstAlert;
            lock (_stateLock)
            {
                isFirstAlert = _state == CheckState.Normal;
                repush = isFirstAlert ? null : _lastAlert;
            }

            if (isFirstAlert)
            {
                // 首次发现不一致：落异常记录（开始时间=发现时刻），锁外落库
                var now = DateTime.Now;
                var recordId = await _recordRepository.InsertAsync(
                    new Tb_MesModelAbnormalRecord
                    {
                        ThirdPartyModel = thirdPartyModel,
                        OrderModel = order.MaterialCode?.Trim(),
                        WorkOrderNo = order.WorkOrderNo,
                        StartTime = now, // 自动记录开始时间
                        Status = (int)MesModelAbnormalStatusEnum.Recording,
                        CreateTime = now,
                    }
                );
                repush = new MesModelMismatchPushDto
                {
                    RecordId = recordId,
                    ThirdPartyModel = thirdPartyModel,
                    OrderModel = order.MaterialCode?.Trim(),
                    WorkOrderNo = order.WorkOrderNo,
                    PushTime = now,
                };
                lock (_stateLock)
                {
                    _state = CheckState.Alerted;
                    _currentRecordId = recordId;
                    _lastAlert = repush;
                }
                Serilog.Log.Warning(
                    "[型号校验]【{_serverName}】型号不一致：三方[{Third}] 订单[{Order}] 工单[{No}]，记录ID={Id} 已推送前台",
                    _serviceName,
                    thirdPartyModel,
                    order.MaterialCode,
                    order.WorkOrderNo,
                    recordId
                );
            }

            try
            {
                await ((IClientProxy)_chatHub.Clients.All).Invoke(MismatchEventName, repush);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"[型号校验] SignalR 推送异常：{ex.Message}");
            }

            return Result<object>.Success(repush);
        }

        /// <summary>
        /// 查询当前告警状态（前台启动/重连时调用，补显告警窗）。
        /// </summary>
        public MesModelAlertStatusDto GetAlertStatus()
        {
            lock (_stateLock)
            {
                return new MesModelAlertStatusDto
                {
                    State = (int)_state,
                    Alert = _lastAlert,
                };
            }
        }

        protected override Task NotifyAsync(Result<object> interact)
        {
            // 告警已在 InteractAsync 中完成
            return Task.CompletedTask;
        }
    }
}
