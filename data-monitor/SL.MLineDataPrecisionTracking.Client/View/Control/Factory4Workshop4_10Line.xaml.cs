using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SL.MLineDataPrecisionTracking.Client.Common;
using SL.MLineDataPrecisionTracking.Client.ViewModel.Control;

namespace SL.MLineDataPrecisionTracking.Client.View.Control
{
    /// <summary>
    /// Factory4Workshop4_10Line.xaml 的交互逻辑
    /// </summary>
    [ViewLinkServerInfoAttribute(
        serverNames: new[]
        {
            "Factory4Workshop4_10Line_ABS",
            "Factory4Workshop4_10Line_Clearance",
            "Factory4Workshop4_10Line_RivetAndCrack",
            "Factory4Workshop4_10Line_Vib",
        },
        header: "装配线数据查询",
        icon: "Resources\\Image\\ProductionRecord.png",
        MatchMode = ViewMatchMode.Any
    )]
    public partial class Factory4Workshop4_10Line : UserControl
    {
        public Factory4Workshop4_10Line(
            Factory4Workshop4_10LineViewModel factory4Workshop4_10LineViewModel
        )
        {
            this.DataContext = factory4Workshop4_10LineViewModel;
            InitializeComponent();
        }
    }
}
