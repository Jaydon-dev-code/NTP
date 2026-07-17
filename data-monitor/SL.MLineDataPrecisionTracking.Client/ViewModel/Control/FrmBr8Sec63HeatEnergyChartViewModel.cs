using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.Streaming.Values;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using SixLabors.ImageSharp;
using SkiaSharp;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    public class FrmBr8Sec63HeatEnergyChartViewModel : ObservableObject
    {
        //a工位坐标
        ConcurrentQueue<PointData<double, double>> _pointsA =
            new ConcurrentQueue<PointData<double, double>>();

        //b工位坐标
        ConcurrentQueue<PointData<double, double>> _pointsB =
            new ConcurrentQueue<PointData<double, double>>();

        List<Tb_EnergyRangeDetail> _currentRangeDetailsA;
        List<Tb_EnergyRangeDetail> _currentRangeDetailsB;
        List<Tb_EnergyRangeDetail> _currentRangeDetails =>
            _selectedWorkStation == Br8Sec63HeatEnergyWorkEnum.A
                ? _currentRangeDetailsA
                : _currentRangeDetailsB;

        PointData<double, double> _firstPoint = new PointData<double, double>() { X = 0, Y = 0 };

        //x轴的间隔
        const double XScale = 0.1;

        //a工位 x 轴的最大值
        double _maxTimeA = 0;

        //b工位 x 轴的最大值
        double _maxTimeB = 0;

        DispatcherTimer _renderTimer;

        //图标对象
        private WpfPlot _PlotControlA;

        public WpfPlot PlotControlA
        {
            get => _PlotControlA;
            set => SetProperty(ref _PlotControlA, value);
        }

        private WpfPlot _potControlB;

        public WpfPlot PlotControlB
        {
            get => _potControlB;
            set => SetProperty(ref _potControlB, value);
        }

        private WpfPlot _settingPlotControl;
        public WpfPlot SettingPlotControl
        {
            get => _settingPlotControl;
            set => SetProperty(ref _settingPlotControl, value);
        }

        List<PointData<double, double>> _currentPointsA = new List<PointData<double, double>>();
        List<PointData<double, double>> _currentPointsB = new List<PointData<double, double>>();

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

        private Br8Sec63HeatEnergyWorkEnum _selectedWorkStation = Br8Sec63HeatEnergyWorkEnum.A;
        public Br8Sec63HeatEnergyWorkEnum SelectedWorkStation
        {
            get => _selectedWorkStation;
            set => SetProperty(ref _selectedWorkStation, value);
        }

        public ICommand DeleteMonitorItemCommand { get; }
        public ICommand RefreshEnergyRangeCommand { get; }
        public ICommand ImportEnergyRangeCommand { get; }
        public ICommand ToggleModelEnabledCommand { get; }
        public ICommand SetStationACommand { get; }
        public ICommand SetStationBCommand { get; }
        public ICommand ExportTemplateCommand { get; }
        public ICommand LoadedCommand { get; }
        public ICommand WorkChangeCommand { get; }

        private readonly EnergyRangeApi _energyRangeApi;

        public FrmBr8Sec63HeatEnergyChartViewModel(HubClien hubClien, EnergyRangeApi energyRangeApi)
        {
           
            hubClien.Start<PointData<double, double>>("SetEbergyDataA", SetEbergyDataA);
            hubClien.Start<PointData<double, double>>("SetEbergyDataB", SetEbergyDataB);

            _energyRangeApi = energyRangeApi;

            LoadedCommand = new AsyncRelayCommand(LoadAsync);
            WorkChangeCommand = new AsyncRelayCommand(WorkChange);
            DeleteMonitorItemCommand = new AsyncRelayCommand(DeleteMonitorItem);
            RefreshEnergyRangeCommand = new AsyncRelayCommand(RefreshEnergyRangeAsync);
            ImportEnergyRangeCommand = new AsyncRelayCommand(ImportEnergyRangeAsync);
            ToggleModelEnabledCommand = new AsyncRelayCommand<Tb_EnergyRange>(
                ToggleModelEnabledAsync
            );
            SetStationACommand = new AsyncRelayCommand(() => SetStationAsync("A"));
            SetStationBCommand = new AsyncRelayCommand(() => SetStationAsync("B"));
            ExportTemplateCommand = new RelayCommand(ExportTemplate);
        }

        private async Task LoadAsync()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                PlotControlA = IntiPlot();
                PlotControlB = IntiPlot();
                SettingPlotControl = IntiPlot();
                _renderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                _renderTimer.Tick += RenderTick;
                _renderTimer.Start();
            });
            await LoadCurrentModelAsync();
        }

        private void SetEbergyDataA(PointData<double, double> data)
        {
            if (data.X > _maxTimeA) return;
            _pointsA.Enqueue(data);
        }

        private void SetEbergyDataB(PointData<double, double> data)
        {
            if (data.X > _maxTimeB) return;
            _pointsB.Enqueue(data);
        }

        private void RenderTick(object sender, EventArgs e)
        {
            DrainQueue(_pointsA, _currentPointsA, PlotControlA, _currentRangeDetailsA);
            DrainQueue(_pointsB, _currentPointsB, PlotControlB, _currentRangeDetailsB);
        }

        private void DrainQueue(
            ConcurrentQueue<PointData<double, double>> queue,
            List<PointData<double, double>> displayPoints,
            WpfPlot plot,
            List<Tb_EnergyRangeDetail> rangeDetails)
        {
            var batch = new List<PointData<double, double>>();
            while (queue.TryDequeue(out var p))
                batch.Add(p);

            if (batch.Count == 0) return;

            foreach (var point in batch)
            {
                if (point.X <= 0.1)
                {
                    displayPoints.Clear();
                    plot.Plot.Clear();
                    displayPoints.Add(_firstPoint);
                }
                displayPoints.Add(point);
            }

            double currentMaxX = displayPoints.Max(p => p.X);

            DrawPartialRange(currentMaxX, plot, rangeDetails);
            DrawDataLine(plot, displayPoints);

            double dataMaxY = displayPoints.Max(p => p.Y);
            plot.Plot.Axes.SetLimits(0, currentMaxX + 0.5, 0, dataMaxY + 30);
            plot.Refresh();
        }

        private async Task WorkChange()
        {
            SwitchStation(SelectedWorkStation);
        }

        private async Task LoadCurrentModelAsync()
        {
            try
            {
                var result = await _energyRangeApi.GetAllAsync();
                if (!result.IsSuccess || result.Data == null)
                    return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProductModels.Clear();
                    foreach (var model in result.Data)
                        ProductModels.Add(model);
                });
                var stationA = result.Data.FirstOrDefault(m => m.Station == "A");
                if (stationA != null)
                {
                    _currentRangeDetailsA = stationA.Details?.OrderBy(d => d.Time).ToList();
                    double xMax = 0;
                    if (_currentRangeDetailsA != null)
                        foreach (var d in _currentRangeDetailsA)
                        {
                            var x = d.Time * XScale;
                            if (x > xMax)
                                xMax = x;
                        }
                    _maxTimeA = xMax + 0.5;
                }

                var stationB = result.Data.FirstOrDefault(m => m.Station == "B");
                if (stationB != null)
                {
                    _currentRangeDetailsB = stationB.Details?.OrderBy(d => d.Time).ToList();
                    double xMax = 0;
                    if (_currentRangeDetailsB != null)
                        foreach (var d in _currentRangeDetailsB)
                        {
                            var x = d.Time * XScale;
                            if (x > xMax)
                                xMax = x;
                        }
                    _maxTimeB = xMax + 0.5;
                }

                SelectedProductModel = stationA ?? stationB;
            }
            catch { }
        }

        private void SwitchStation(Br8Sec63HeatEnergyWorkEnum station)
        {
            var stationStr = station == Br8Sec63HeatEnergyWorkEnum.A ? "A" : "B";
            var model = ProductModels.FirstOrDefault(m => m.Station == stationStr);
            var rangeDetails = model?.Details?.OrderBy(d => d.Time).ToList();
            if (rangeDetails == null)
            {
                model = ProductModels.FirstOrDefault();
                if (model == null)
                    return;
                rangeDetails =
                    model.Details?.OrderBy(d => d.Time).ToList()
                    ?? new List<Tb_EnergyRangeDetail>();
            }

            double xMax = 0;
            foreach (var d in rangeDetails)
            {
                var x = d.Time * XScale;
                if (x > xMax)
                    xMax = x;
            }

            if (station == Br8Sec63HeatEnergyWorkEnum.A)
            {
                _currentRangeDetailsA = rangeDetails;
                _maxTimeA = xMax + 0.5;
            }
            else
            {
                _currentRangeDetailsB = rangeDetails;
                _maxTimeB = xMax + 0.5;
            }

            SelectedProductModel = model;

            var plot = station == Br8Sec63HeatEnergyWorkEnum.A ? PlotControlA : PlotControlB;
            var pts = station == Br8Sec63HeatEnergyWorkEnum.A ? _currentPointsA : _currentPointsB;

            Application.Current.Dispatcher.Invoke(() =>
            {
                plot.Plot.Clear();
                if (pts.Count > 0)
                {
                    double currentMaxX = pts.Max(p => p.X);
                    DrawPartialRange(currentMaxX, plot, rangeDetails);
                    DrawDataLine(plot, pts);
                    double dataMaxY = pts.Max(p => p.Y);
                    plot.Plot.Axes.SetLimits(0, currentMaxX + 0.5, 0, dataMaxY + 30);
                }
                plot.Refresh();
            });
        }

        private void DrawPartialRange(
            double currentMaxX,
            WpfPlot plot,
            List<Tb_EnergyRangeDetail> rangeDetails
        )
        {
            if (rangeDetails == null || rangeDetails.Count == 0)
                return;

            var lowerPts = new List<Coordinates>();
            var upperPts = new List<Coordinates>();
            bool reachedEnd = true;

            for (int i = 0; i < rangeDetails.Count; i++)
            {
                var d = rangeDetails[i];
                var x = d.Time * XScale;
                if (x > currentMaxX)
                {
                    reachedEnd = false;
                    if (i > 0)
                    {
                        var prev = rangeDetails[i - 1];
                        double t = (currentMaxX - prev.Time * XScale) / (x - prev.Time * XScale);
                        double l =
                            (double)prev.LowerLimit
                            + t * ((double)d.LowerLimit - (double)prev.LowerLimit);
                        double u =
                            (double)prev.UpperLimit
                            + t * ((double)d.UpperLimit - (double)prev.UpperLimit);
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
                var lowerLine = plot.Plot.Add.Scatter(
                    lowerPts.Select(p => p.X).ToArray(),
                    lowerPts.Select(p => p.Y).ToArray(),
                    Colors.Red
                );
                lowerLine.LineWidth = 2;
                lowerLine.MarkerSize = 0;
            }

            if (upperPts.Count >= 2)
            {
                var upperLine = plot.Plot.Add.Scatter(
                    upperPts.Select(p => p.X).ToArray(),
                    upperPts.Select(p => p.Y).ToArray(),
                    Colors.Red
                );
                upperLine.LineWidth = 2;
                upperLine.MarkerSize = 0;
            }

            var startLine = plot.Plot.Add.Line(
                lowerPts[0].X,
                lowerPts[0].Y,
                upperPts[0].X,
                upperPts[0].Y
            );
            startLine.LineWidth = 2;
            startLine.LineColor = Colors.Red;

            if (reachedEnd && lowerPts.Count >= 2)
            {
                var endLine = plot.Plot.Add.Line(
                    lowerPts[lowerPts.Count - 1].X,
                    lowerPts[lowerPts.Count - 1].Y,
                    upperPts[upperPts.Count - 1].X,
                    upperPts[upperPts.Count - 1].Y
                );
                endLine.LineWidth = 2;
                endLine.LineColor = Colors.Red;
            }
        }

        private void DrawDataLine(WpfPlot plot, List<PointData<double, double>> pts)
        {
            if (pts.Count < 2)
                return;

            var xs = pts.Select(p => p.X).ToArray();
            var ys = pts.Select(p => p.Y).ToArray();
            var scatter = plot.Plot.Add.Scatter(xs, ys, Colors.Green);
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

        private async Task DeleteMonitorItem()
        {
            if (SelectedProductModel == null)
                return;

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

            if (dialog.ShowDialog() != true)
                return;

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
                HandyControl.Controls.MessageBox.Show(
                    result.Message,
                    "导入成功",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                await RefreshEnergyRangeAsync();
            }
            else
            {
                HandyControl.Controls.MessageBox.Show(
                    result.Message,
                    "导入失败",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async Task ToggleModelEnabledAsync(Tb_EnergyRange model) { }

        private void ExportTemplate()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx",
                FileName = "能量范围.xlsx",
                Title = "导出模板文件",
            };

            if (dialog.ShowDialog() != true)
                return;

            string src = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "能量范围.xlsx"
            );
            if (System.IO.File.Exists(src))
            {
                System.IO.File.Copy(src, dialog.FileName, true);
                HandyControl.Controls.MessageBox.Show(
                    "模板导出成功",
                    "提示",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            else
            {
                HandyControl.Controls.MessageBox.Show(
                    $"模板文件不存在：{src}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async Task SetStationAsync(string station)
        {
            if (SelectedProductModel == null)
                return;

            await _energyRangeApi.SetCurrentStationModelAsync(SelectedProductModel.Id, station);

            SelectedWorkStation =
                station == "A" ? Br8Sec63HeatEnergyWorkEnum.A : Br8Sec63HeatEnergyWorkEnum.B;

            var details = SelectedProductModel.Details.OrderBy(d => d.Time).ToList();
            double xMax = 0;
            foreach (var d in details)
            {
                var x = d.Time * XScale;
                if (x > xMax)
                    xMax = x;
            }
            if (station == "A")
            {
                _currentRangeDetailsA = details;
                _maxTimeA = xMax + 0.5;
            }
            else
            {
                _currentRangeDetailsB = details;
                _maxTimeB = xMax + 0.5;
            }

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
                if (tmp.X > xMax)
                    xMax = tmp.X;
                if (tmp.Y > yMax)
                    yMax = tmp.Y;
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

            SettingPlotControl.Plot.Axes.SetLimits(0, xMax + 0.5, 0, yMax + 30);
            SettingPlotControl.Refresh();
        }
    }
}
