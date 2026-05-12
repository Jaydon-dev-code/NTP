using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using NPOI.SS.Formula.Functions;
using SL.MLineDataPrecisionTracking.Client.Common;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Client.View.Control;
using SL.MLineDataPrecisionTracking.Client.ViewModel.Control;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SqlSugar.Extensions;
using Window = System.Windows.Window;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        string lastMenuName;

        List<MenuItems> _menus;

        /// <summary>
        /// 中间的控件
        /// </summary>
        private object _currentContent;
        public object CurrentContent
        {
            get => _currentContent;
            set => SetProperty(ref _currentContent, value);
        }

        private string _softwareName;
        public string SoftwareName
        {
            get => _softwareName;
            set => SetProperty(ref _softwareName, value);
        }

        private string _versionInfo;
        public string VersionInfo
        {
            get => _versionInfo;
            set => SetProperty(ref _versionInfo, value);
        }

        private string _tbTime;
        public string TbTime
        {
            get => _tbTime;
            set => SetProperty(ref _tbTime, value);
        }

        private AsyncRelayCommand<SideMenu> _loadedCommand;
        public AsyncRelayCommand<SideMenu> LoadedCommand
        {
            get
            {
                if (_loadedCommand == null)
                {
                    _loadedCommand = new AsyncRelayCommand<SideMenu>(Loaded);
                }
                return _loadedCommand;
            }
        }

        private RelayCommand<FunctionEventArgs<object>> _selectionChangedCommand;
        public IRelayCommand<FunctionEventArgs<object>> SelectionChangedCommand
        {
            get
            {
                if (_selectionChangedCommand == null)
                {
                    _selectionChangedCommand = new RelayCommand<FunctionEventArgs<object>>(
                        SelectionChanged
                    );
                }
                return _selectionChangedCommand;
            }
        }
        private RelayCommand _timerStartCommand;
        public IRelayCommand TimerStartCommand
        {
            get
            {
                if (_timerStartCommand == null)
                {
                    _timerStartCommand = new RelayCommand(TimerStart);
                }
                return _timerStartCommand;
            }
        }

        private RelayCommand<Window> _minWindowCommand;
        public IRelayCommand<Window> MinWindowCommand
        {
            get
            {
                if (_minWindowCommand == null)
                {
                    _minWindowCommand = new RelayCommand<Window>(MinWindow);
                }
                return _minWindowCommand;
            }
        }
        private RelayCommand<Window> _closeWindowCommand;
        public IRelayCommand<Window> CloseWindowCommand
        {
            get
            {
                if (_closeWindowCommand == null)
                {
                    _closeWindowCommand = new RelayCommand<System.Windows.Window>(CloseWindow);
                }
                return _closeWindowCommand;
            }
        }

        private void CloseWindow(Window window)
        {
            window.Close();
        }

        private void MinWindow(System.Windows.Window window)
        {
            window.WindowState = WindowState.Minimized;
        }

        ServiceApi _serviceApi;

        public MainWindowViewModel(ServiceApi serviceApi)
        {
            _serviceApi = serviceApi;
            SoftwareName = string.IsNullOrEmpty(ConfigurationManager.AppSettings["SoftwareName"])
                ? "湖北双林轴承数字追溯管理系统"
                : ConfigurationManager.AppSettings["SoftwareName"];
            VersionInfo = System
                .Reflection.Assembly.GetExecutingAssembly()
                .GetName()
                .Version.ToString();
           
        }

      

        /// <summary>
        /// 从文件路径加载图片到 Image 控件
        /// </summary>
        private BitmapImage LoadImageFromPath(string path)
        {
            try
            {
                // 1. 创建 BitmapImage 对象
                BitmapImage bitmap = new BitmapImage();

                // 2. 初始化（必须）
                bitmap.BeginInit();

                // 3. 设置路径
                bitmap.UriSource = new Uri(path, UriKind.Absolute);

                // 4. 结束初始化
                bitmap.EndInit();

                // 5. 赋值给 Image 控件
                return bitmap;
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show($"加载失败：{ex.Message}");
            }
            return new BitmapImage();
        }

        async Task Loaded(SideMenu sideMenu)
        {
           await GetMeun();
            bool isFirst = true;
            foreach (var item in _menus)
            {
                sideMenu.Items.Add(
                    new SideMenuItem()
                    {
                        Header = item.Header,
                        IsSelected = isFirst,
                        Icon = new Image() { Source = LoadImageFromPath(item.Icon) },
                    }
                );
                if (isFirst)
                {
                    isFirst = false;
                    lastMenuName = item.Header;
                    CurrentContent = item.Page;
                }
            }
        }
        private async Task GetMeun()
        {
            _menus = new List<MenuItems>();
            var serviceInfo = await _serviceApi.GetAllServicesAsync();

            if (serviceInfo.IsSuccess)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                // 2. 目标命名空间
                string targetNamespaceView = "SL.MLineDataPrecisionTracking.Client.View.Control";
                var types = assembly
                    .GetTypes()
                    .Where(t =>
                        t.IsClass
                        && // 是类
                        !t.IsAbstract
                        && // 不是抽象类
                        !t.IsGenericType
                        && // 不是泛型类
                        !t.IsInterface
                        && // 不是接口
                        t.Namespace==targetNamespaceView // 匹配命名空间
                    )
                    .Where(t => !t.Name.StartsWith("<"))
                    .ToList();
                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute<ViewLinkServerInfoAttribute>();
                    if (attr == null)
                    {
                        continue;
                    }
                    else
                    {
                        bool allServersValid = true;

                        foreach (var typeName in attr.ServiceType)
                        {
                            if (
                                serviceInfo.Data.FirstOrDefault(x => x.ServiceType == typeName && x.IsEnabled)
                                == null
                            )
                            {
                                allServersValid = false;
                                break;
                            }
                        }
                        if (allServersValid)
                        {
                            _menus.Add(
                                new MenuItems()
                                {
                                    Header = attr.Header,
                                    Page = App.Container.Resolve(type),
                                    Icon = $"{AppDomain.CurrentDomain.BaseDirectory}{attr.Icon}",
                                }
                            );
                        }
                    }
                }
            }

            _menus.Add(
                new MenuItems()
                {
                    Header = "设置",
                    Icon = $"{AppDomain.CurrentDomain.BaseDirectory}Resources\\Image\\Setting.png",
                    Page = App.Container.Resolve<DeviceCollectionConfig>(),
                }
            );
        }
        void SelectionChanged(FunctionEventArgs<object> e)
        {
            if (e.Info is SideMenuItem menu && menu.Header != lastMenuName)
            {
                var select = _menus.FirstOrDefault(x => x.Header == menu.Header);
                CurrentContent = select?.Page;
                lastMenuName = select.Header;
            }
        }

        void TimerStart()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                TbTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            };
            timer.Start();
        }
    }
}
