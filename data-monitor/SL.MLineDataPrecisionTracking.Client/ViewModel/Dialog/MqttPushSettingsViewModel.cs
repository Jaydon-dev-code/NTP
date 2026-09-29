using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Dialog
{
    /// <summary>
    /// MQTT 推送设置窗口 VM：加载/保存指定采集服务的 MqttPushConfig 配置
    /// </summary>
    public partial class MqttPushSettingsViewModel : ObservableObject
    {
        private readonly MqttPushConfigApi _mqttPushConfigApi;
        private readonly string _configKey;

        public string Title { get; }
        public string Description { get; }

        /// <summary>
        /// 保存成功后由窗口代码接管关闭动作
        /// </summary>
        public Action CloseAction { get; set; }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private string _host;
        public string Host
        {
            get => _host;
            set => SetProperty(ref _host, value);
        }

        private string _portText;
        public string PortText
        {
            get => _portText;
            set => SetProperty(ref _portText, value);
        }

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _topic;
        public string Topic
        {
            get => _topic;
            set => SetProperty(ref _topic, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private AsyncRelayCommand _loadCommand;
        public AsyncRelayCommand LoadCommand =>
            _loadCommand ??= new AsyncRelayCommand(LoadAsync);

        private AsyncRelayCommand _saveCommand;
        public AsyncRelayCommand SaveCommand =>
            _saveCommand ??= new AsyncRelayCommand(SaveAsync);

        public MqttPushSettingsViewModel(
            MqttPushConfigApi mqttPushConfigApi,
            string configKey,
            string title,
            string description
        )
        {
            _mqttPushConfigApi = mqttPushConfigApi;
            _configKey = configKey;
            Title = title;
            Description = description;
            PortText = "1883";
            Topic = configKey;
        }

        public async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                var result = await _mqttPushConfigApi.GetAsync(_configKey);
                if (result.IsSuccess && result.Data != null)
                {
                    var config = result.Data;
                    IsEnabled = config.IsEnabled;
                    Host = config.Host;
                    PortText = config.Port > 0 ? config.Port.ToString() : "1883";
                    Username = config.Username;
                    Password = config.Password;
                    Topic = string.IsNullOrWhiteSpace(config.Topic)
                        ? _configKey
                        : config.Topic;
                }
                else if (!result.IsSuccess)
                {
                    HandyControl.Controls.MessageBox.Error(
                        result.Message ?? "读取配置失败"
                    );
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"读取配置失败：{ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAsync()
        {
            if (IsEnabled && string.IsNullOrWhiteSpace(Host))
            {
                HandyControl.Controls.MessageBox.Warning("开启推送时必须填写 MQTT 服务器地址");
                return;
            }

            if (!int.TryParse(PortText, out var port) || port <= 0 || port > 65535)
            {
                HandyControl.Controls.MessageBox.Warning("端口必须是 1-65535 之间的数字");
                return;
            }

            IsLoading = true;
            try
            {
                var config = new Tb_MqttPushConfig
                {
                    ConfigKey = _configKey,
                    IsEnabled = IsEnabled,
                    Host = Host?.Trim(),
                    Port = port,
                    Username = Username?.Trim(),
                    Password = Password,
                    Topic = string.IsNullOrWhiteSpace(Topic) ? _configKey : Topic.Trim(),
                };

                var result = await _mqttPushConfigApi.SaveAsync(config);
                if (result.IsSuccess)
                {
                    HandyControl.Controls.MessageBox.Success("保存成功");
                    CloseAction?.Invoke();
                }
                else
                {
                    HandyControl.Controls.MessageBox.Error(result.Message ?? "保存失败");
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"保存失败：{ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
