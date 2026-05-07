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
    /// Rcl_MeticulousPursuit.xaml 的交互逻辑
    /// </summary>
    [ViewLinkServerInfoAttribute( serverName:new string[] { nameof(Rcl_ProLineDataCollectionService) },heard: "热处理数据查询",icon: "Resources\\Image\\ProductionRecord.png")]
    public partial class Rcl_MeticulousPursuit : UserControl
    {
        public Rcl_MeticulousPursuit(Rcl_MeticulousPursuitViewModel rcl_MeticulousPursuitViewModel)
        {
            this.DataContext = rcl_MeticulousPursuitViewModel;
            InitializeComponent();
        }
    }
}
