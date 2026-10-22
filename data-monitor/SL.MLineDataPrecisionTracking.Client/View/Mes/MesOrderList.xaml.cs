using System.Windows.Controls;
using Autofac;
using SL.MLineDataPrecisionTracking.Client.ViewModel.Control;

namespace SL.MLineDataPrecisionTracking.Client.View.Mes
{
    /// <summary>
    /// MES 生产工单列表控件
    /// </summary>
    public partial class MesOrderList : UserControl
    {
        public MesOrderList()
        {
            DataContext = App.Container.Resolve<MesOrderListViewModel>();
            InitializeComponent();
        }
    }
}
