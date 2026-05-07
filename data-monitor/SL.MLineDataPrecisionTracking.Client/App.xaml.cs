using Autofac;
using Serilog;
using SL.MLineDataPrecisionTracking.Client.Middleware;
using SL.MLineDataPrecisionTracking.Client.View;
using SL.MLineDataPrecisionTracking.Core.Middleware;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SL.MLineDataPrecisionTracking.Client
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex;
        private const string AppUniqueName = "SL.MLineDataPrecisionTracking.Client";
        public static IContainer Container { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {

             // 尝试创建互斥体，判断是否已有实例运行
            _mutex = new Mutex(true, AppUniqueName, out bool isNewInstance);

            if (!isNewInstance)
            {
                HandyControl.Controls.MessageBox .Show("应用程序已在运行，无法启动多个实例！", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown();
                return;
            }



            // 全局异常捕获
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            // 1. 创建容器建造者
            var builder = new ContainerBuilder();

            builder.AddViewMiddleware();
            builder.AddHttpMiddleware();
            builder.AddInfrastructureMiddleware();
            builder.AddCoreMiddleware();
            builder.AddLogMiddleware();

            // 4. 构建容器
            Container = builder.Build();

            var mainWindow = Container.Resolve<MainWindow>();


            mainWindow.Closing += MainWindow_Closing;
            mainWindow.Show();
        }

        // 程序退出时释放互斥体
        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Close();
            base.OnExit(e);
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

            Log.Error($"UI异常，请联系管理员。\r\n{e.Exception.Message}");
            HandyControl.Controls.MessageBox.Show(
                $"UI异常，请联系管理员。{e.Exception.Message}",
                "错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            Log.Error($"UI异常，请联系管理员。\r\n{ e.Exception.Message}");
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