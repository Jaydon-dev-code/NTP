using System;
using System.Windows;

namespace SL.DataCollectionModuelScript
{
    /// <summary>
    /// 配置/还原前的二次确认窗口：展示设备信息 + 让用户选模块型号
    /// </summary>
    public partial class ConfirmWindow : Window
    {
        /// <summary>用户确认后选中的模块</summary>
        public ModuelTypeEnum SelectedModule { get; private set; } = ModuelTypeEnum.BCNet_Q;

        public ConfirmWindow(DeviceInfo device, bool isRestore)
        {
            InitializeComponent();

            TxtTitle.Text = isRestore
                ? $"确认对设备【{device.DeviceId}】执行【还原默认IP】？"
                : $"确认对设备【{device.DeviceId}】执行【配置IP】？";

            TxtInfo.Text = isRestore
                ? $"访问地址（原IP）：{device.Ip}\n将还原为所选模块的默认网络参数。"
                : $"生产线：{device.Line}\n目标IP：{device.Ip}\n掩码：{device.Mask}\n网关：{device.Gateway}";

            // 模块下拉，默认选中设备自带类型
            CboModule.ItemsSource = Enum.GetValues(typeof(ModuelTypeEnum));
            CboModule.SelectedItem = device.ModuleType;
            CboModule.SelectionChanged += (_, _) => UpdateDefaultText(isRestore);
            UpdateDefaultText(isRestore);
        }

        /// <summary>刷新所选模块的默认参数说明</summary>
        private void UpdateDefaultText(bool isRestore)
        {
            if (CboModule.SelectedItem is not ModuelTypeEnum mod) return;
            SelectedModule = mod;

            // 与 AutomationService 中 GetModuleDefaultNetwork 保持一致
            TxtDefault.Text = $"模块：{mod}\n" +
                              "默认IP：192.168.1.178  掩码：255.255.255.0  网关：192.168.1.1";
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (CboModule.SelectedItem is ModuelTypeEnum mod)
                SelectedModule = mod;
            DialogResult = true;
        }
    }
}
