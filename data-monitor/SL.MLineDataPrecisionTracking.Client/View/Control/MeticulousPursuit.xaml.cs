using SL.MLineDataPrecisionTracking.Client.Common;
using SL.MLineDataPrecisionTracking.Client.ViewModel.Control;
using SL.MLineDataPrecisionTracking.Core.Services;
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

namespace SL.MLineDataPrecisionTracking.Client.View.Control
{
    /// <summary>
    /// MeticulousPursuit.xaml 的交互逻辑
    /// </summary>
    [ViewLinkServerInfoAttribute(serverNames: new[] { "Factory6Workshop6_3AssemblyLineA", "Factory6Workshop6_3AssemblyLineB", "Factory6Workshop6_1AssemblyLineA", "Factory6Workshop6_1AssemblyLineB" }, header: "装配线数据查询", icon: "Resources\\Image\\ProductionRecord.png", MatchMode = ViewMatchMode.Any)]
    public partial class MeticulousPursuit : UserControl
    {
       
        public MeticulousPursuit(MeticulousPursuitViewModel meticulousPursuitViewModel)
        {
            this.DataContext = meticulousPursuitViewModel;
            InitializeComponent();
            
        }

     
    }
}
