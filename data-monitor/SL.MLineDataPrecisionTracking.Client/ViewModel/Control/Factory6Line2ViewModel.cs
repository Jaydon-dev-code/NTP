using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Client.View.Control;
using SL.MLineDataPrecisionTracking.Core.Services.DataCollection.Factory6Line2;
using SL.MLineDataPrecisionTracking.Models.Dtos.Mes;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    /// <summary>
    /// 6分厂2号线 ViewModel（全自动，无按钮）：
    /// 订阅 SignalR 型号不一致告警（MesModelMismatch）→ 弹置顶告警窗；
    /// 后台告警期间每轮重推（前端离线错过也能收到）；
    /// 启动时调 modelCheck/status 查询当前告警状态补显窗口；
    /// 无执行中订单或型号恢复一致推送 MesModelMatched → 自动关闭窗口。
    /// </summary>
    public partial class Factory6Line2ViewModel : ObservableObject
    {
        HubClien _hubClien;
        ModelCheckApi _modelCheckApi;
        Factory6Line2AlertWindow _alertWindow;

        /// <summary>当前告警窗对应的记录ID（重推幂等：同记录不重复处理）</summary>
        private int _currentRecordId;

        public Factory6Line2ViewModel(HubClien hubClien, ModelCheckApi modelCheckApi)
        {
            _hubClien = hubClien;
            _modelCheckApi = modelCheckApi;

            // 订阅后台型号校验推送（事件名与服务端 Factory6Line2ModelCheckService 常量一致）
            _hubClien.Start<MesModelMismatchPushDto>(
                Factory6Line2ModelCheckService.MismatchEventName,
                OnModelMismatch
            );
            _hubClien.Start<MesModelMatchedPushDto>(
                Factory6Line2ModelCheckService.MatchedEventName,
                OnModelMatched
            );

            // 启动补显：客户端离线期间的告警，上线后查状态恢复窗口
            _ = RestoreAlertFromStatusAsync();
        }

        #region 告警窗绑定属性

        private string _alertMessage = "检测当前生产和执行中的型号不一致";
        public string AlertMessage
        {
            get => _alertMessage;
            set => SetProperty(ref _alertMessage, value);
        }

        private string _thirdPartyModel;
        public string ThirdPartyModel
        {
            get => _thirdPartyModel;
            set => SetProperty(ref _thirdPartyModel, value);
        }

        private string _orderModel;
        public string OrderModel
        {
            get => _orderModel;
            set => SetProperty(ref _orderModel, value);
        }

        private string _workOrderNo;
        public string WorkOrderNo
        {
            get => _workOrderNo;
            set => SetProperty(ref _workOrderNo, value);
        }

        #endregion

        #region SignalR 回调

        /// <summary>
        /// 型号不一致 → 弹告警窗。
        /// 后台告警期间每轮重推：同记录ID且窗口已开时仅刷新信息。
        /// </summary>
        private void OnModelMismatch(MesModelMismatchPushDto push)
        {
            // SignalR 回调在后台线程，切 UI 线程
            Application.Current?.Dispatcher.Invoke(() =>
            {
                ThirdPartyModel = push?.ThirdPartyModel;
                OrderModel = push?.OrderModel;
                WorkOrderNo = push?.WorkOrderNo;
                AlertMessage = "检测当前生产和执行中的型号不一致";
                _currentRecordId = push?.RecordId ?? 0;

                ShowAlertWindow();
            });
        }

        /// <summary>型号恢复一致或无执行中订单 → 自动关闭告警窗</summary>
        private void OnModelMatched(MesModelMatchedPushDto matched)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                CloseAlertWindow();
            });
        }

        #endregion

        #region 启动补显

        /// <summary>
        /// 启动时查询后台当前告警状态：有进行中的告警 → 恢复告警窗。
        /// </summary>
        private async Task RestoreAlertFromStatusAsync()
        {
            try
            {
                var re = await _modelCheckApi.GetStatusAsync();
                var status = re?.Data;
                if (status?.Alert == null || status.State == 0)
                {
                    return; // 无进行中告警
                }

                Application.Current?.Dispatcher.Invoke(() =>
                {
                    _currentRecordId = status.Alert.RecordId;
                    ThirdPartyModel = status.Alert.ThirdPartyModel;
                    OrderModel = status.Alert.OrderModel;
                    WorkOrderNo = status.Alert.WorkOrderNo;
                    AlertMessage = "检测当前生产和执行中的型号不一致";

                    ShowAlertWindow();
                });
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"启动查询型号告警状态失败：{ex.Message}");
            }
        }

        #endregion

        #region 告警窗开关

        private void ShowAlertWindow()
        {
            if (_alertWindow == null)
            {
                _alertWindow = new Factory6Line2AlertWindow { DataContext = this };
                _alertWindow.Closed += (s, e) => _alertWindow = null;
            }

            if (!_alertWindow.IsVisible)
            {
                // 非模态 Show：不阻塞主窗体操作
                _alertWindow.Show();
            }
            _alertWindow.Activate();
        }

        private void CloseAlertWindow()
        {
            if (_alertWindow != null)
            {
                _alertWindow.Close();
                _alertWindow = null;
            }

            _currentRecordId = 0;
        }

        #endregion
    }
}
