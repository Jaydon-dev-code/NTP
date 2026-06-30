using SL.MLineDataPrecisionTracking.Client.Common;
using SL.MLineDataPrecisionTracking.Client.ViewModel.Control;
using SL.MLineDataPrecisionTracking.Core.Services;
using System.Windows.Controls;

namespace SL.MLineDataPrecisionTracking.Client.View.Control
{
    [ViewLinkServerInfoAttribute(serverName: new string[] {}, heard: "加热数据", icon: "Resources\\Image\\ProductionRecord.png")]
    public partial class FrmBr8Sec63HeatEnergyChart : UserControl
    {
        public FrmBr8Sec63HeatEnergyChart(
            FrmBr8Sec63HeatEnergyChartViewModel heatDataViewModell)
        {
            this.DataContext = heatDataViewModell;
            InitializeComponent();
        }
    }
}
