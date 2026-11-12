using SL.ScanData.Data;
using SL.ScanData.Models;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SL.ScanData
{
    public partial class MainWindow : Window
    {
        private const int MaxDisplayCount = 20;

        private readonly DbService _dbService = new();
        private bool _isSaving;

        public ObservableCollection<ScanRecord> Records { get; } = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            this.Closing+= MainWindow_Closing;
            TimerStart();
            var asm = Assembly.GetEntryAssembly();
            var ver = asm.GetName().Version;
            VersionInfoTextBlock.Text ="版本：V"+ ver.ToString();
        }
        void TimerStart()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                TimeTextBlock.Text = "当前时间："+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            };
            timer.Start();
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = HandyControl.Controls.MessageBox.Show(
                $"确定要退出{this.Title}吗？",
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var record in _dbService.GetLatest(MaxDisplayCount))
            {
                Records.Add(record);
            }
            RenumberRecords();
            this.ScanTextBox.Focus();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            ScanTextBox.Focus();
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 保证键盘输入始终回到扫码框
            if (!ScanTextBox.IsKeyboardFocused)
            {
                ScanTextBox.Focus();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 全局回车都交给扫码框处理
            if (e.Key == Key.Enter && !ScanTextBox.IsKeyboardFocused)
            {
                ScanTextBox.Focus();
                SubmitScan();
                e.Handled = true;
            }
        }

        private void ScanTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ScanBorder.BorderBrush = System.Windows.Media.Brushes.Transparent;
            FocusHintText.Visibility = Visibility.Collapsed;
        }

        private void ScanTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ScanBorder.BorderBrush = System.Windows.Media.Brushes.Red;
            FocusHintText.Visibility = Visibility.Visible;
        }

        private void ScanTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SubmitScan();
            }
        }

        private void SubmitScan()
        {
            if (_isSaving)
                return;

            var sn = ScanTextBox.Text.Trim();
            if (string.IsNullOrEmpty(sn))
                return;

            _isSaving = true;
            try
            {
                var record = new ScanRecord { Sn = sn, ScanTime = DateTime.Now };

                if (!_dbService.TryInsert(record, out var duplicate))
                {
                    var previousTime = duplicate?.ScanTime.ToString("yyyy-MM-dd HH:mm:ss") ?? "未知";
                    var previousSn = duplicate?.Sn ?? sn;
                    HandyControl.Controls.MessageBox.Show(
                        this,
                        $"该条码录入重复！\n\n之前录入的条码：{previousSn}\n之前录入时间：{previousTime}",
                        "重复提示",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    ScanTextBox.Clear();
                    return;
                }

                Records.Insert(0, record);
                while (Records.Count > MaxDisplayCount)
                {
                    Records.RemoveAt(Records.Count - 1);
                }
                RenumberRecords();

                ScanTextBox.Clear();
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show(
                    this,
                    $"保存失败：{ex.Message}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                _isSaving = false;
                ScanTextBox.Focus();
            }
        }

        private void RenumberRecords()
        {
            for (int i = 0; i < Records.Count; i++)
            {
                Records[i].DisplayIndex = i + 1;
            }
        }
    }
}
