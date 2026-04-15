using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using Serilog;
using SL.MLineDataPrecisionTracking.Client.Middleware;
using SL.MLineDataPrecisionTracking.Client.View;
using SL.MLineDataPrecisionTracking.Core.Middleware;

namespace SL.MLineDataPrecisionTracking.Client
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        // 全局容器（整个程序唯一）
        public static IContainer Container { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // 全局异常捕获
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            // 1. 创建容器建造者
            var builder = new ContainerBuilder();

            builder.AddViewMiddleware();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddSqlSugerMiddleware();
            builder.AddLogMiddleware();

            // 4. 构建容器
            Container = builder.Build();

            // 5. 从容器解析主窗口（关键！这样才能自动注入）

            var mainWindow = Container.Resolve<MainWindow>();
           //调试模式强行顶置会 卡
#if DEBUG
            mainWindow.Topmost = false;

#else
            mainWindow.Topmost = true;

#endif

            mainWindow.Closing += MainWindow_Closing;
            mainWindow.Show();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = HandyControl.Controls.MessageBox.Show(
                "确定要退出双林轴承数字追溯管理系统吗？",
                "确认关闭",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
            {
                // 取消关闭
                e.Cancel = true;
            }
        }

        // UI 线程异常（最常用）
        private void App_DispatcherUnhandledException(
            object sender,
            DispatcherUnhandledExceptionEventArgs e
        )
        {
            e.Handled = true; // 关键：标记已处理，程序不闪退

            HandyControl.Controls.MessageBox.Show(
                $"UI异常，请联系管理员。",
                "错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            Log.Error("UI异常，请联系管理员。\r\n{e}", e);
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
                Log.Error("全局异常，请联系管理员。\r\n{ex}", ex);
            }

            // 这里可以加日志保存
        }
    }
}
