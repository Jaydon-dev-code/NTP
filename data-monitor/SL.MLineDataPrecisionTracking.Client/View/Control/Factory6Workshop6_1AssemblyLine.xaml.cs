using SL.MLineDataPrecisionTracking.Client.Common;
using SL.MLineDataPrecisionTracking.Client.ViewModel.Control;
using System.Windows.Controls;

namespace SL.MLineDataPrecisionTracking.Client.View.Control
{
    [ViewLinkServerInfoAttribute(serverNames: new[] { "Factory6Workshop6_1AssemblyLineA", "Factory6Workshop6_1AssemblyLineB" }, header: "装配线数据查询", icon: "Resources\\Image\\ProductionRecord.png", MatchMode = ViewMatchMode.Any)]
    public partial class Factory6Workshop6_1AssemblyLine : UserControl
    {
        public Factory6Workshop6_1AssemblyLine(Factory6Workshop6_1AssemblyLineViewModel viewModel)
        {
            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}
