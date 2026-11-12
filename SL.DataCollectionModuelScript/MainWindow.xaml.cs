using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Win32;

namespace SL.DataCollectionModuelScript
{
    /// <summary>
    /// 主窗口：搜索、设备列表、配置IP/还原默认IP、右侧日志
    /// </summary>
    public partial class MainWindow : Window
    {
        private ExcelService? _excel;
        private readonly AutomationService _auto = new();
        private List<DeviceInfo> _all = new();
        private CancellationTokenSource? _cts;

        public MainWindow()
        {
            InitializeComponent();
            // 模块下拉框（与 ModuelTypeEnum 一致）
            CboModule.ItemsSource = Enum.GetValues(typeof(ModuelTypeEnum));
            CboModule.SelectedItem = ModuelTypeEnum.BCNet_Q;
            // 默认无头，不观察页面
            ChkObserve.IsChecked = true;
            _auto.ShowBrowser = true;
        }

        /// <summary>观察页面勾选变化：同步到自动化服务（下次执行生效）</summary>
        private void ChkObserve_Changed(object sender, RoutedEventArgs e)
        {
            _auto.ShowBrowser = ChkObserve.IsChecked == true;
        }

        // ==================== 加载 ====================

        /// <summary>窗体加载：选Excel → 读数据 → 绑定列表</summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "选择设备Excel文件",
                Filter = "Excel文件|*.xlsx;*.xls"
            };
            if (ofd.ShowDialog() != true)
            {
                MessageBox.Show("未选择Excel文件，无法加载数据。", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _excel = new ExcelService(ofd.FileName);
            _auto.OnLog += OnAutoLog;
            _cts = new CancellationTokenSource();
            Reload();
            AppendLog("系统", "初始化", "成功", "已加载：" + ofd.FileName);
        }

        /// <summary>关闭窗体：取消任务、关浏览器</summary>
        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _cts?.Cancel();
            _auto.Dispose();
        }

        /// <summary>重新读取Excel并刷新列表</summary>
        private void Reload()
        {
            if (_excel == null) return;
            try
            {
                _all = _excel.ReadAll();
                ApplyFilter();
                AppendLog("系统", "加载数据", "成功", $"共 {_all.Count} 台设备");
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取Excel失败：" + ex.Message, "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== 搜索 ====================

        /// <summary>模糊搜索：匹配设备编号或生产线号</summary>
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        private void ApplyFilter()
        {
            string kw = TxtSearch.Text.Trim();
            var list = string.IsNullOrEmpty(kw)
                ? _all
                : _all.Where(d =>
                    d.DeviceId.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    d.Line.Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
            Dgv.ItemsSource = null;
            Dgv.ItemsSource = list;
        }

        private void BtnReload_Click(object sender, RoutedEventArgs e) => Reload();

        // ==================== 按钮事件 ====================

        /// <summary>配置IP：确认弹窗（含模块选择）→ 异步执行</summary>
        private async void BtnConfigure_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not DeviceInfo d) return;
            if (d.IsBusy) return;
            if (string.IsNullOrWhiteSpace(d.Ip))
            {
                MessageBox.Show("该设备IP为空，无法执行。", "错误");
                return;
            }

            // 确认弹窗：可改模块，确认后才执行
            var dlg = new ConfirmWindow(d, isRestore: false)
            {
                Owner = this
            };
            if (dlg.ShowDialog() != true) return;

            var mod = dlg.SelectedModule;
            d.ModuleType = mod;
            await RunTaskAsync(d, isRestore: false);
        }

        /// <summary>还原默认IP：确认弹窗 → 异步执行</summary>
        private async void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not DeviceInfo d) return;
            if (d.IsBusy) return;
            if (string.IsNullOrWhiteSpace(d.Ip))
            {
                MessageBox.Show("该设备IP为空，无法执行。", "错误");
                return;
            }

            var dlg = new ConfirmWindow(d, isRestore: true) { Owner = this };
            if (dlg.ShowDialog() != true) return;

            d.ModuleType = dlg.SelectedModule;
            await RunTaskAsync(d, isRestore: true);
        }

        /// <summary>双击行也可打开配置确认</summary>
        private void Dgv_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Dgv.SelectedItem is DeviceInfo d)
            {
                // 找到该行的配置按钮逻辑入口
                var dlg = new ConfirmWindow(d, isRestore: false) { Owner = this };
                if (dlg.ShowDialog() == true)
                {
                    d.ModuleType = dlg.SelectedModule;
                    _ = RunTaskAsync(d, isRestore: false);
                }
            }
        }

        // ==================== 异步执行 ====================

        /// <summary>
        /// 统一异步入口：标记进行中 → 调用服务 → 成功写Excel绿/清绿 → 刷新行
        /// </summary>
        private async Task RunTaskAsync(DeviceInfo d, bool isRestore)
        {
            d.IsBusy = true;
            d.Status = "进行中";

            bool ok = false;
            try
            {
                ok = isRestore
                    ? await _auto.RestoreDefaultIpAsync(d, _cts?.Token ?? CancellationToken.None)
                    : await _auto.ConfigureIpAsync(d, _cts?.Token ?? CancellationToken.None);
            }
            catch (Exception ex)
            {
                AppendLog(d.DeviceId, isRestore ? "还原默认IP" : "配置IP", "错误", ex.Message);
            }

            // 回UI线程更新状态和颜色
            Dispatcher.Invoke(() =>
            {
                try
                {
                    if (ok)
                    {
                        d.Status = "成功";
                        d.IsSuccess = !isRestore; // 配置成功=绿，还原成功=清绿
                        try
                        {
                            //if (isRestore) _excel?.ClearSuccess(d);
                           // else _excel?.MarkSuccess(d);
                        }
                        catch (Exception ex)
                        {
                            AppendLog(d.DeviceId, "Excel写入", "错误", ex.Message);
                        }
                    }
                    else
                    {
                        d.Status = "错误";
                    }
                }
                finally
                {
                    d.IsBusy = false;
                }
            });
        }

        // ==================== 日志 ====================

        /// <summary>自动化服务日志回调（可能来自后台线程）</summary>
        private void OnAutoLog(string id, string step, string status, string? detail)
        {
            if (Dispatcher.CheckAccess()) AppendLog(id, step, status, detail);
            else Dispatcher.Invoke(() => AppendLog(id, step, status, detail));
        }

        /// <summary>向右侧日志区追加一行（带颜色）</summary>
        private void AppendLog(string id, string step, string status, string? detail)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] [{id}] {step} — {status}";
            if (!string.IsNullOrEmpty(detail)) line += " — " + detail;

            Color c = status switch
            {
                "成功" => Colors.Green,
                "错误" => Colors.Red,
                _ => Colors.Black
            };

            var para = new Paragraph
            {
                Inlines = { new Run(line) { Foreground = new SolidColorBrush(c) } },
                Margin = new Thickness(0)
            };
            RtbLog.Document.Blocks.Add(para);

            // 滚到底部
            RtbLog.ScrollToEnd();
        }
    }
}
