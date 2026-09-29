using System.Windows;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Client.ViewModel.Dialog;

namespace SL.MLineDataPrecisionTracking.Client.View.Dialog
{
    /// <summary>
    /// MQTT 推送设置窗口
    /// </summary>
    public partial class MqttPushSettingsWindow : Window
    {
        private readonly MqttPushSettingsViewModel _viewModel;

        public MqttPushSettingsWindow(
            MqttPushConfigApi mqttPushConfigApi,
            string configKey,
            string title,
            string description
        )
        {
            _viewModel = new MqttPushSettingsViewModel(
                mqttPushConfigApi,
                configKey,
                title,
                description
            );
            _viewModel.CloseAction = Close;
            DataContext = _viewModel;
            InitializeComponent();

            Loaded += MqttPushSettingsWindow_Loaded;
        }

        private async void MqttPushSettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MqttPushSettingsWindow_Loaded;
            await _viewModel.LoadAsync();
            PasswordInput.Password = _viewModel.Password ?? string.Empty;
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = PasswordInput.Password;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
