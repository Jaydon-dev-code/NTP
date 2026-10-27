using System.Windows;
using System.Windows.Input;

namespace SL.MLineDataPrecisionTracking.Client.View.Control
{
    /// <summary>
    /// 6分厂2号线型号不一致告警窗口：
    /// 置顶、不可缩放/最小化、按住可拖动、非模态（不影响主窗体操作）。
    /// 型号恢复一致后由 ViewModel 关闭。
    /// </summary>
    public partial class Factory6Line2AlertWindow : Window
    {
        public Factory6Line2AlertWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 无边框窗口拖动
            DragMove();
        }
    }
}
