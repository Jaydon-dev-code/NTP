using System.Windows.Controls;
using SL.MLineDataPrecisionTracking.Client.Common;
using SL.MLineDataPrecisionTracking.Client.ViewModel.Control;

namespace SL.MLineDataPrecisionTracking.Client.View.Control
{
    /// <summary>
    /// 6分厂2号线（承载 MES 工单控件 + 型号不一致告警订阅）
    /// </summary>
    [ViewLinkServerInfoAttribute(
        serverNames: new string[] { "Factory6Line2MeasurePushService", "Factory6Line2ModelCheckService" },
        header: "订单",
        icon: "Resources\\Image\\ProductionRecord.png",
        MatchMode = ViewMatchMode.Any
    )]
    public partial class Factory6Line2 : UserControl
    {
        public Factory6Line2(Factory6Line2ViewModel viewModel)
        {
            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}
