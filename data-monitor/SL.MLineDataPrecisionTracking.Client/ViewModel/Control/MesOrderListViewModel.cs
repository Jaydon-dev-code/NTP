using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Models.Entities.Mes;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    /// <summary>
    /// MES 工单列表 ViewModel：查询展示工单，状态切换（待执行↔执行中）通过 API 落库
    /// </summary>
    public partial class MesOrderListViewModel : ObservableObject
    {
        private readonly MesOrderApi _mesOrderApi;

        /// <summary>
        /// 工单集合
        /// </summary>
        public ObservableCollection<Tb_MesOrder> Orders { get; } =
            new ObservableCollection<Tb_MesOrder>();

        public MesOrderListViewModel(MesOrderApi mesOrderApi)
        {
            _mesOrderApi = mesOrderApi;
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            var result = await _mesOrderApi.QueryAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                return;
            }

            Orders.Clear();
            foreach (var order in result.Data)
            {
                Orders.Add(order);
            }
        }

        private RelayCommand _refreshCommand;

        /// <summary>
        /// 刷新工单列表
        /// </summary>
        public IRelayCommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    _refreshCommand = new RelayCommand(async () => await LoadAsync());
                }
                return _refreshCommand;
            }
        }

        private RelayCommand<Tb_MesOrder> _toggleStatusCommand;

        /// <summary>
        /// 切换订单状态（待执行 ↔ 执行中），先调 API 落库，成功后再刷新
        /// </summary>
        public IRelayCommand<Tb_MesOrder> ToggleStatusCommand
        {
            get
            {
                if (_toggleStatusCommand == null)
                {
                    _toggleStatusCommand = new RelayCommand<Tb_MesOrder>(async order =>
                        await ToggleStatusAsync(order));
                }
                return _toggleStatusCommand;
            }
        }

        private async Task ToggleStatusAsync(Tb_MesOrder order)
        {
            if (order == null)
            {
                return;
            }

            var targetStatus = order.Status == 0 ? 1 : 0;
            var result = await _mesOrderApi.SetStatusAsync(order.WorkOrderNo, targetStatus);
            if (!result.IsSuccess)
            {
                HandyControl.Controls.MessageBox.Show(
                    $"状态切换失败：{result.Message}",
                    "错误"
                );
                return;
            }

            await LoadAsync();
        }
    }
}
