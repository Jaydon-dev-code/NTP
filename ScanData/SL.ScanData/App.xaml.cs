using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace SL.ScanData
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex;
        private const string AppUniqueName = "SL.ScanData";

        protected override void OnStartup(StartupEventArgs e)
        {
            // 尝试创建互斥体，判断是否已有实例运行
            _mutex = new Mutex(true, AppUniqueName, out bool isNewInstance);

            if (!isNewInstance)
            {
                HandyControl.Controls.MessageBox.Show(
                    "应用程序已在运行，无法启动多个实例！",
                    "提示",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                Shutdown();
                return;
            }

            // 全局异常捕获
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
           
        }

        // 程序退出时释放互斥体
        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Close();
            base.OnExit(e);
        }

   

        // UI 线程异常（最常用）
        private void App_DispatcherUnhandledException(
            object sender,
            DispatcherUnhandledExceptionEventArgs e
        )
        {
            e.Handled = true; // 关键：标记已处理，程序不闪退

            HandyControl.Controls.MessageBox.Show(
                $"UI异常，请联系管理员。{e.Exception.Message}",
                "错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        // 非UI线程 / 后台异常
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                HandyControl.Controls.MessageBox.Show(
                    $"全局异常，请联系管理员。",
                    "发生致命错误。。",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop
                );
            }
        }
    }
}
