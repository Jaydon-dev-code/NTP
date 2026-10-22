using System.Windows.Controls;
using SL.MLineDataPrecisionTracking.Client.Common;

namespace SL.MLineDataPrecisionTracking.Client.View.Control
{
    /// <summary>
    /// 6分厂2号线（承载 MES 工单控件）
    /// </summary>
    [ViewLinkServerInfoAttribute(
        serverNames: new string[] { },
        header: "6分厂2号线",
        icon: "Resources\\Image\\ProductionRecord.png",
        MatchMode = ViewMatchMode.Any
    )]
    public partial class Factory6Line2 : UserControl
    {
        public Factory6Line2()
        {
            InitializeComponent();
        }
    }
}
