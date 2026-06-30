using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NPOI.SS.Formula.Functions;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using SkiaSharp;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    public class FrmBr8Sec63HeatEnergyChartViewModel : ObservableObject
    {
        private WpfPlot _PlotControl;

        public WpfPlot PlotControl
        {
            get => _PlotControl;
            set => SetProperty(ref _PlotControl, value);
        }

        List<PointData<double, double>> _points = new List<PointData<double, double>>();
        const double XScale = 0.1;
        double _maxTime = 0;
        List<Tb_EnergyRangeDetail> _currentRangeDetails;
        PointData<double, double> _firstPoint = new PointData<double, double>() { X = 0, Y = 0 };

        private readonly EnergyRangeApi _energyRangeApi;
        private readonly object _pointsLock = new object();

        private WpfPlot _settingPlotControl;
        public WpfPlot SettingPlotControl
        {
            get => _settingPlotControl;
            set => SetProperty(ref _settingPlotControl, value);
        }

        public ObservableCollection<Tb_EnergyRange> ProductModels { get; } =
            new ObservableCollection<Tb_EnergyRange>();

        private Tb_EnergyRange _selectedProductModel;
        public Tb_EnergyRange SelectedProductModel
        {
            get => _selectedProductModel;
            set
            {
                if (SetProperty(ref _selectedProductModel, value))
                {
                    OnPropertyChanged(nameof(CurrentModelName));
                    PlotEnergyRangeData();
                }
            }
        }

        public string CurrentModelName => SelectedProductModel?.ProductModel ?? "未选择";

        public ICommand DeleteMonitorItemCommand { get; }
        public ICommand StartEditCommand { get; }
        public ICommand ConfirmRenameCommand { get; }
        public ICommand RefreshEnergyRangeCommand { get; }
        public ICommand ImportEnergyRangeCommand { get; }
        public ICommand ToggleModelEnabledCommand { get; }
        public ICommand SetCurrentProductionCommand { get; }

        public FrmBr8Sec63HeatEnergyChartViewModel(HubClien hubClien, EnergyRangeApi energyRangeApi)
        {
            PlotControl = IntiPlot();
            SettingPlotControl = IntiPlot();
            hubClien.Start<PointData<double, double>>("EnergyRangeUpdated", EnergyRangeUpdated());

            _energyRangeApi = energyRangeApi;

            DeleteMonitorItemCommand = new AsyncRelayCommand(DeleteMonitorItem);
            StartEditCommand = new RelayCommand<ChartMonitorItem>(StartEdit);
            ConfirmRenameCommand = new RelayCommand<ChartMonitorItem>(ConfirmRename);
            RefreshEnergyRangeCommand = new AsyncRelayCommand(RefreshEnergyRangeAsync);
            ImportEnergyRangeCommand = new AsyncRelayCommand(ImportEnergyRangeAsync);
            ToggleModelEnabledCommand = new AsyncRelayCommand<Tb_EnergyRange>(
                ToggleModelEnabledAsync
            );
            SetCurrentProductionCommand = new AsyncRelayCommand(SetCurrentProductionAsync);

            _ = LoadCurrentModelAsync();
        }

        private async Task LoadCurrentModelAsync()
        {
            try
            {
                var result = await _energyRangeApi.GetAllAsync();
                if (!result.IsSuccess || result.Data == null) return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProductModels.Clear();
                    foreach (var model in result.Data)
                        ProductModels.Add(model);
                });

                var enabled = result.Data.FirstOrDefault(m => m.IsEnabled);
                if (enabled == null) return;

                _currentRangeDetails = enabled.Details.OrderBy(d => d.Time).ToList();
                double xMax = 0;
                foreach (var d in _currentRangeDetails)
                {
                    var x = d.Time * XScale;
                    if (x > xMax) xMax = x;
                }
                _maxTime = xMax + 0.5;
                SelectedProductModel = enabled;
            }
            catch { }
        }

        private Action<PointData<double, double>> EnergyRangeUpdated()
        {
            return point =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (point.X > _maxTime)
                        return;

                    lock (_pointsLock)
                    {
                        if (point.X <= 0.1)
                        {
                            _points.Clear();
                            PlotControl.Plot.Clear();
                            _points.Add(_firstPoint);
                        }

                        _points.Add(point);

                        double currentMaxX = _points.Max(p => p.X);

                        DrawPartialRange(currentMaxX);
                        DrawDataLine();

                        double dataMaxY = _points.Max(p => p.Y);
                        PlotControl.Plot.Axes.SetLimits(0, currentMaxX + 0.5, 0, dataMaxY + 30);
                    }

                    PlotControl.Refresh();
                });
            };
        }

        private void DrawPartialRange(double currentMaxX)
        {
            if (_currentRangeDetails == null || _currentRangeDetails.Count == 0)
                return;

            var lowerPts = new List<Coordinates>();
            var upperPts = new List<Coordinates>();
            bool reachedEnd = true;

            for (int i = 0; i < _currentRangeDetails.Count; i++)
            {
                var d = _currentRangeDetails[i];
                var x = d.Time * XScale;
                if (x > currentMaxX)
                {
                    reachedEnd = false;
                    if (i > 0)
                    {
                        var prev = _currentRangeDetails[i - 1];
                        double t = (currentMaxX - prev.Time * XScale) / (x - prev.Time * XScale);
                        double l = (double)prev.LowerLimit + t * ((double)d.LowerLimit - (double)prev.LowerLimit);
                        double u = (double)prev.UpperLimit + t * ((double)d.UpperLimit - (double)prev.UpperLimit);
                        lowerPts.Add(new Coordinates(currentMaxX, l));
                        upperPts.Add(new Coordinates(currentMaxX, u));
                    }
                    break;
                }
                lowerPts.Add(new Coordinates(x, (double)d.LowerLimit));
                upperPts.Add(new Coordinates(x, (double)d.UpperLimit));
            }

            if (lowerPts.Count == 0)
                return;

            if (lowerPts.Count >= 2)
            {
                var lowerLine = PlotControl.Plot.Add.Scatter(
                    lowerPts.Select(p => p.X).ToArray(),
                    lowerPts.Select(p => p.Y).ToArray(),
                    Colors.Red);
                lowerLine.LineWidth = 2;
                lowerLine.MarkerSize = 0;
            }

            if (upperPts.Count >= 2)
            {
                var upperLine = PlotControl.Plot.Add.Scatter(
                    upperPts.Select(p => p.X).ToArray(),
                    upperPts.Select(p => p.Y).ToArray(),
                    Colors.Red);
                upperLine.LineWidth = 2;
                upperLine.MarkerSize = 0;
            }

            var startLine = PlotControl.Plot.Add.Line(
                lowerPts[0].X, lowerPts[0].Y,
                upperPts[0].X, upperPts[0].Y);
            startLine.LineWidth = 2;
            startLine.LineColor = Colors.Red;

            if (reachedEnd && lowerPts.Count >= 2)
            {
                var endLine = PlotControl.Plot.Add.Line(
                    lowerPts[lowerPts.Count - 1].X, lowerPts[lowerPts.Count - 1].Y,
                    upperPts[upperPts.Count - 1].X, upperPts[upperPts.Count - 1].Y);
                endLine.LineWidth = 2;
                endLine.LineColor = Colors.Red;
            }
        }

        private void DrawDataLine()
        {
            if (_points.Count < 2) return;

            var xs = _points.Select(p => p.X).ToArray();
            var ys = _points.Select(p => p.Y).ToArray();
            var scatter = PlotControl.Plot.Add.Scatter(xs, ys, Colors.Green);
            scatter.LineWidth = 2;
            scatter.MarkerSize = 0;
        }

        private WpfPlot IntiPlot()
        {
            WpfPlot plotControl = new WpfPlot();
            plotControl.Plot.Axes.Left.TickGenerator =
                new ScottPlot.TickGenerators.NumericFixedInterval(5);
            plotControl.Plot.Axes.Right.TickGenerator =
                new ScottPlot.TickGenerators.NumericFixedInterval(0.1);
            plotControl.UserInputProcessor.IsEnabled = false;
            plotControl.Plot.Axes.SetLimits(left: 0, bottom: 0);
            plotControl.Plot.XLabel("时间 (秒)");
            plotControl.Plot.Font.Automatic();
            plotControl.Refresh();
            return plotControl;
        }

        private async Task RefreshEnergyRangeAsync()
        {
            try
            {
                var result = await _energyRangeApi.GetAllAsync();
                if (result.IsSuccess && result.Data != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProductModels.Clear();
                        foreach (var model in result.Data)
                            ProductModels.Add(model);
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        HandyControl.Controls.MessageBox.Show(
                            result.Message ?? "加载失败",
                            "刷新失败",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    });
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    HandyControl.Controls.MessageBox.Show(
                        $"刷新失败：{ex.Message}",
                        "错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                });
            }
        }

        private void StartEdit(ChartMonitorItem item)
        {
            if (item == null) return;
            item.EditingName = item.Name;
            item.IsEditing = true;
        }

        private void ConfirmRename(ChartMonitorItem item)
        {
            if (item == null) return;
            if (!string.IsNullOrWhiteSpace(item.EditingName))
                item.Name = item.EditingName.Trim();
            item.IsEditing = false;
        }

        private async Task DeleteMonitorItem()
        {
            if (SelectedProductModel == null) return;

            var result = HandyControl.Controls.MessageBox.Show(
                $"确定要删除监控项 \"{SelectedProductModel.ProductModel}\" 吗？",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                await _energyRangeApi.DeleteNavAsync(SelectedProductModel.Id);
                ProductModels.Remove(SelectedProductModel);
                SelectedProductModel = null;
            }
        }

        private async Task ImportEnergyRangeAsync()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx",
                Title = "选择能量范围Excel文件",
            };

            if (dialog.ShowDialog() != true) return;

            var result = await _energyRangeApi.ImportAsync(dialog.FileName, false);

            if (!result.IsSuccess && result.Message.Contains("已存在"))
            {
                var confirm = HandyControl.Controls.MessageBox.Show(
                    result.Message,
                    "确认覆盖",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (confirm == MessageBoxResult.Yes)
                    result = await _energyRangeApi.ImportAsync(dialog.FileName, true);
                else
                    return;
            }

            if (result.IsSuccess)
            {
                HandyControl.Controls.MessageBox.Show(result.Message, "导入成功", MessageBoxButton.OK, MessageBoxImage.Information);
                await RefreshEnergyRangeAsync();
            }
            else
            {
                HandyControl.Controls.MessageBox.Show(result.Message, "导入失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ToggleModelEnabledAsync(Tb_EnergyRange model)
        {
            if (model == null) return;
            try
            {
                await _energyRangeApi.ToggleEnabledAsync(model.Id, model.IsEnabled);
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Show($"更新失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SetCurrentProductionAsync()
        {
            if (SelectedProductModel == null) return;

            await _energyRangeApi.SetCurrentModelAsync(SelectedProductModel.Id);

            _currentRangeDetails = SelectedProductModel.Details.OrderBy(d => d.Time).ToList();
            double xMax = 0;
            foreach (var d in _currentRangeDetails)
            {
                var x = d.Time * XScale;
                if (x > xMax) xMax = x;
            }
            _maxTime = xMax + 0.5;

            var result = await _energyRangeApi.GetAllAsync();
            if (result.IsSuccess && result.Data != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProductModels.Clear();
                    foreach (var model in result.Data)
                        ProductModels.Add(model);
                });

                var enabled = result.Data.FirstOrDefault(m => m.IsEnabled);
                if (enabled != null)
                    SelectedProductModel = enabled;
            }
        }

        private void PlotEnergyRangeData()
        {
            SettingPlotControl.Plot.Clear();

            if (SelectedProductModel?.Details == null || SelectedProductModel.Details.Count == 0)
            {
                SettingPlotControl.Refresh();
                return;
            }

            var sorted = SelectedProductModel.Details.OrderBy(d => d.Time).ToList();
            var coords = new List<Coordinates>();
            double xMax = 0;
            double yMax = 0;
            foreach (var d in sorted)
            {
                var tmp = new Coordinates(d.Time * XScale, (double)d.LowerLimit);
                coords.Add(tmp);
                if (tmp.X > xMax) xMax = tmp.X;
                if (tmp.Y > yMax) yMax = tmp.Y;
            }
            foreach (var d in Enumerable.Reverse(sorted))
            {
                coords.Add(new Coordinates(d.Time * XScale, (double)d.UpperLimit));
            }

            if (coords.Count >= 3)
            {
                var polygon = SettingPlotControl.Plot.Add.Polygon(coords.ToArray());
                polygon.FillStyle.Color = Colors.Transparent;
                polygon.LineStyle.Color = Colors.Red;
                polygon.LineStyle.Width = 2;
                polygon.LineStyle.Pattern = LinePattern.Solid;
            }

            SettingPlotControl.Plot.Axes.SetLimits(0, xMax + 0.5, 0, yMax + 5);
            SettingPlotControl.Refresh();
        }
    }
}
