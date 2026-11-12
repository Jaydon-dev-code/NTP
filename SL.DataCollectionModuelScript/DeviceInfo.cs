using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SL.DataCollectionModuelScript
{
    /// <summary>
    /// 设备模型：对应 Excel 一行数据 + 界面状态
    /// </summary>
    public class DeviceInfo : INotifyPropertyChanged
    {
        /// <summary>Excel 行号（0基），回写绿色标记用</summary>
        public int ExcelRowIndex { get; set; }

        /// <summary>设备编号</summary>
        private string _deviceId = "";
        public string DeviceId
        {
            get => _deviceId;
            set { _deviceId = value; OnPropertyChanged(); }
        }

        /// <summary>生产线号</summary>
        private string _line = "";
        public string Line
        {
            get => _line;
            set { _line = value; OnPropertyChanged(); }
        }

        /// <summary>设备 IP（同时是访问配置页的地址）</summary>
        private string _ip = "";
        public string Ip
        {
            get => _ip;
            set { _ip = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 子网掩码。
        /// 注意：Excel 表头叫「mac地址」，但内容实际是子网掩码（如 255.255.255.0）。
        /// </summary>
        private string _mask = "";
        public string Mask
        {
            get => _mask;
            set { _mask = value; OnPropertyChanged(); }
        }

        /// <summary>网关</summary>
        private string _gateway = "";
        public string Gateway
        {
            get => _gateway;
            set { _gateway = value; OnPropertyChanged(); }
        }

        /// <summary>模块类型（Excel 可选列，默认 BCNet_Q）</summary>
        private ModuelTypeEnum _moduleType = ModuelTypeEnum.BCNet_Q;
        public ModuelTypeEnum ModuleType
        {
            get => _moduleType;
            set { _moduleType = value; OnPropertyChanged(); }
        }

        /// <summary>是否配置成功（绿色标记）</summary>
        private bool _isSuccess;
        public bool IsSuccess
        {
            get => _isSuccess;
            set { _isSuccess = value; OnPropertyChanged(); OnPropertyChanged(nameof(RowBackground)); }
        }

        /// <summary>是否正在执行（防重复点击）</summary>
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanClick)); }
        }

        /// <summary>行背景色：成功=浅绿，否则白</summary>
        public string RowBackground => IsSuccess ? "#C6EFCE" : "White";

        /// <summary>按钮是否可点</summary>
        public bool CanClick => !IsBusy;

        /// <summary>状态文本：待执行/进行中/成功/错误</summary>
        private string _status = "待执行";
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
