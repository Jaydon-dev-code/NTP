using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Win32;
using ScottPlot;
using ScottPlot.WPF;
using SL.MLineDataPrecisionTracking.Client.Http;
using SL.MLineDataPrecisionTracking.Core.Services;
using SL.MLineDataPrecisionTracking.Infrastructure.Common;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SL.MLineDataPrecisionTracking.Models.Dtos.Request;
using SL.MLineDataPrecisionTracking.Models.Dtos.Response;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SL.MLineDataPrecisionTracking.Models.Entities.Factory6Workshop6_3AssemblyLine;
using SqlSugar;
using static SL.MLineDataPrecisionTracking.Infrastructure.Common.Expand;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    public partial class Rcl_MeticulousPursuitViewModel : ObservableObject
    {
        const double _xDefMaxValue = 12.5;
        const double _yDefMaxValue = 1400;
        #region 能量监控
        List<HeatTreatmentDataDto> _mkNoQuValues;
        List<HeatTreatmentDataDto> _quValues;
        private readonly Rcl_MeticulousPursuitApi _meticulousPursuitApi;
        List<PointData<double, double>> _displayPoints;
        PointData<double, double> _firstPoint = new PointData<double, double>() { X = 0, Y = 0 };
        ConcurrentQueue<PointData<double, double>> _points =
            new ConcurrentQueue<PointData<double, double>>();
        DispatcherTimer _renderTimer;
        List<Tb_EnergyRangeDetail> _currentRangeDetails;
        double _lastX;

        //图标对象
        private WpfPlot _plotControl;

        public WpfPlot PlotControl
        {
            get => _plotControl;
            set => SetProperty(ref _plotControl, value);
        }

        public WpfPlot SettingPlotControl
        {
            get => _settingPlotControl;
            set => SetProperty(ref _settingPlotControl, value);
        }

        private WpfPlot _settingPlotControl;

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

        private int? _currentModelNo;
        public int? CurrentModelNo
        {
            get => _currentModelNo;
            set => SetProperty(ref _currentModelNo, value);
        }

        public ICommand DeleteMonitorItemCommand { get; }
        public ICommand RefreshEnergyRangeCommand { get; }
        public ICommand ImportEnergyRangeCommand { get; }
        public ICommand ToggleModelEnabledCommand { get; }
        public ICommand SetCurrentProductionCommand { get; }
        public ICommand ExportTemplateCommand { get; }
        #endregion

        #region 能量监控查询
        private DateTime _energyQueryStartDate = DateTime.Now.Date;
        public DateTime EnergyQueryStartDate
        {
            get => _energyQueryStartDate;
            set => SetProperty(ref _energyQueryStartDate, value);
        }

        private DateTime _energyQueryStartTime = DateTime.Now.Date;
        public DateTime EnergyQueryStartTime
        {
            get => _energyQueryStartTime;
            set => SetProperty(ref _energyQueryStartTime, value);
        }

        private DateTime _energyQueryEndDate = DateTime.Now.Date;
        public DateTime EnergyQueryEndDate
        {
            get => _energyQueryEndDate;
            set => SetProperty(ref _energyQueryEndDate, value);
        }

        private DateTime _energyQueryEndTime = DateTime.Now.Date.AddTicks(-1);
        public DateTime EnergyQueryEndTime
        {
            get => _energyQueryEndTime;
            set => SetProperty(ref _energyQueryEndTime, value);
        }

        public ObservableCollection<Tb_Factory6Workshop6_3Line_EnergyRange> EnergyRangeItems { get; } =
            new ObservableCollection<Tb_Factory6Workshop6_3Line_EnergyRange>();

        private Tb_Factory6Workshop6_3Line_EnergyRange _selectedEnergyRange;
        public Tb_Factory6Workshop6_3Line_EnergyRange SelectedEnergyRange
        {
            get => _selectedEnergyRange;
            set
            {
                if (SetProperty(ref _selectedEnergyRange, value))
                {
                    _ = LoadEnergyPointsAsync();
                }
            }
        }

        private PaginationPage _energyRangePagination = new PaginationPage() {  DataCountPerPage=100};
        public PaginationPage EnergyRangePagination
        {
            get => _energyRangePagination;
            set => SetProperty(ref _energyRangePagination, value);
        }

        public ObservableCollection<Tb_Factory6Workshop6_3Line_EnergyRangePoint> EnergyPointItems { get; } =
            new ObservableCollection<Tb_Factory6Workshop6_3Line_EnergyRangePoint>();

        public WpfPlot EnergyPointPlot
        {
            get => _energyPointPlot;
            set => SetProperty(ref _energyPointPlot, value);
        }

        private WpfPlot _energyPointPlot;

        public ICommand EnergyRangeQueryCommand { get; }
        public ICommand EnergyRangePageUpdatedCommand { get; }
      
        #endregion

        #region 精追

        private ObservableCollection<HeatTreatmentDataDto> _queryValue;

        public ObservableCollection<HeatTreatmentDataDto> QueryValue
        {
            get => _queryValue;
            set => SetProperty(ref _queryValue, value);
        }
        private PaginationPage _paginationPages;
        public PaginationPage PaginationPages
        {
            get => _paginationPages;
            set => SetProperty(ref _paginationPages, value);
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        private RefinedSearchCriteria _queryConditions;
        public RefinedSearchCriteria QueryConditions
        {
            get => _queryConditions;
            set => SetProperty(ref _queryConditions, value);
        }

        private static ObservableCollection<int> _pageSoure;
        public ObservableCollection<int> PageSoure
        {
            get => _pageSoure;
            set => SetProperty(ref _pageSoure, value);
        }

        private static ObservableCollection<ScanRecord> _historyScanMarkingNos;
        public ObservableCollection<ScanRecord> HistoryScanMarkingNos
        {
            get => _historyScanMarkingNos;
            set => SetProperty(ref _historyScanMarkingNos, value);
        }

        private AsyncRelayCommand _queryCommand;
        public AsyncRelayCommand QueryCommand
        {
            get
            {
                if (_queryCommand == null)
                {
                    _queryCommand = new AsyncRelayCommand(Query);
                }
                return _queryCommand;
            }
        }
        private IRelayCommand<System.Windows.Controls.TextBox> _openScanCodeModelCommand;
        public IRelayCommand<System.Windows.Controls.TextBox> OpenScanCodeModelCommand
        {
            get
            {
                if (_openScanCodeModelCommand == null)
                {
                    _openScanCodeModelCommand = new RelayCommand<System.Windows.Controls.TextBox>(
                        OpenScanCodeModel
                    );
                }
                return _openScanCodeModelCommand;
            }
        }

        private AsyncRelayCommand<System.Windows.Controls.TextBox> _markingNoQueryCommand;
        public AsyncRelayCommand<System.Windows.Controls.TextBox> MarkingNoQueryCommand
        {
            get
            {
                if (_markingNoQueryCommand == null)
                {
                    _markingNoQueryCommand = new AsyncRelayCommand<System.Windows.Controls.TextBox>(
                        MarkingNoQuery,
                        (x) => QueryConditions?.IsScanCode == true
                    );
                }
                return _markingNoQueryCommand;
            }
        }

        private RelayCommand _saveQueryCommand;
        public IRelayCommand SaveQueryCommand
        {
            get
            {
                if (_saveQueryCommand == null)
                {
                    _saveQueryCommand = new RelayCommand(SaveQuery);
                }
                return _saveQueryCommand;
            }
        }

        private AsyncRelayCommand _pageUpdatedCommand;
        public IAsyncRelayCommand PageUpdatedCommand
        {
            get
            {
                if (_pageUpdatedCommand == null)
                {
                    _pageUpdatedCommand = new AsyncRelayCommand(PageUpdated);
                }
                return _pageUpdatedCommand;
            }
        }

        private RelayCommand<ScanRecord> _removeHistoryScanCommand;
        public IRelayCommand<ScanRecord> RemoveHistoryScanCommand
        {
            get
            {
                if (_removeHistoryScanCommand == null)
                {
                    _removeHistoryScanCommand = new RelayCommand<ScanRecord>(RemoveHistoryScan);
                }
                return _removeHistoryScanCommand;
            }
        }

        private RelayCommand _clearHistoryMarkingNoCommand;
        public IRelayCommand ClearHistoryMarkingNoCommand
        {
            get
            {
                if (_clearHistoryMarkingNoCommand == null)
                {
                    _clearHistoryMarkingNoCommand = new RelayCommand(ClearHistoryMarkingNo);
                }
                return _clearHistoryMarkingNoCommand;
            }
        }

        #endregion
        public Rcl_MeticulousPursuitViewModel(
            Rcl_MeticulousPursuitApi meticulousPursuitApi,
            HubClien hubClien
        )
        {
            #region 精追
            _meticulousPursuitApi = meticulousPursuitApi;
            QueryValue = new ObservableCollection<HeatTreatmentDataDto>();
            PaginationPages = new PaginationPage();
            _mkNoQuValues = new List<HeatTreatmentDataDto>();
            _quValues = new List<HeatTreatmentDataDto>();
            HistoryScanMarkingNos = new ObservableCollection<ScanRecord>();
            var lastDay = DateTime.Now.Date;
            QueryConditions = new RefinedSearchCriteria()
            {
                StartDate = lastDay.Date,
                EndDate = lastDay.Date,
                StartTime = lastDay,
                EndTime = lastDay.AddTicks(-1),
            };
            PageSoure = new ObservableCollection<int>() { 100, 200, 500 };
            PaginationPages.DataCountPerPage = 100;
            #endregion

            #region 能量
            hubClien.Start<PointData<double, double>>(
                "Factory6Section6_3HeatTreatEnergyMgmt",
                SetEbergyData
            );
            PlotControl = IntiPlot();
            _displayPoints = new List<PointData<double, double>>();
            _renderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _renderTimer.Tick += RenderTick;
            _renderTimer.Start();

            SettingPlotControl = IntiPlot();
            RefreshEnergyRangeCommand = new AsyncRelayCommand(RefreshEnergyRangeAsync);
            ImportEnergyRangeCommand = new AsyncRelayCommand(ImportEnergyRangeAsync);
            DeleteMonitorItemCommand = new AsyncRelayCommand(DeleteMonitorItem);
            ToggleModelEnabledCommand = new AsyncRelayCommand<Tb_EnergyRange>(
                ToggleModelEnabledAsync
            );
            SetCurrentProductionCommand = new AsyncRelayCommand(SetCurrentProductionAsync);
            ExportTemplateCommand = new RelayCommand(ExportTemplate);
            _ = LoadCurrentModelAsync();
            #endregion

            #region 能量监控查询
            EnergyPointPlot = IntiPlot();
            EnergyRangeQueryCommand = new AsyncRelayCommand(EnergyRangeQueryAsync);
            EnergyRangePageUpdatedCommand = new AsyncRelayCommand(EnergyRangePageUpdated);
            #endregion
        }

        private void RenderTick(object sender, EventArgs e)
        {
            var batch = new List<PointData<double, double>>();
            while (_points.TryDequeue(out var p))
                batch.Add(p);

            if (batch.Count == 0)
                return;

            foreach (var point in batch)
            {
                // 新工件开始：时间倒退（X 小于上一采集点），清空上一工件曲线，避免首尾相连画成闭合图形
                if (point.X + 0.001 < _lastX)
                {
                    _displayPoints.Clear();
                    PlotControl.Plot.Clear();
                    _displayPoints.Add(_firstPoint);
                }
                _displayPoints.Add(point);
                _lastX = point.X;
            }

            double currentMaxX = _displayPoints.Max(p => p.X);

            DrawPartialRange(currentMaxX, PlotControl, _currentRangeDetails);
            DrawDataLine(PlotControl, _displayPoints);

            double dataMaxY = _displayPoints.Max(p => p.Y);
            var xLimits = currentMaxX + 2.5;
            var yLimits = dataMaxY + 500;
            PlotControl.Plot.Axes.SetLimits(0, xLimits > _xDefMaxValue? xLimits : _xDefMaxValue, 0, yLimits > _yDefMaxValue? yLimits : _yDefMaxValue);
            PlotControl.Refresh();
        }

        private void SetEbergyData(PointData<double, double> data)
        {
            _points.Enqueue(data);
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
                var x = d.Time;
                if (x > currentMaxX)
                {
                    reachedEnd = false;
                    if (i > 0)
                    {
                        var prev = rangeDetails[i - 1];
                        double t = (currentMaxX - prev.Time) / (x - prev.Time);
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
                new ScottPlot.TickGenerators.NumericFixedInterval(100);
            plotControl.Plot.Axes.Right.TickGenerator =
                new ScottPlot.TickGenerators.NumericFixedInterval(0.5);
            plotControl.UserInputProcessor.IsEnabled = false;
            plotControl.Plot.Axes.SetLimits(0, _xDefMaxValue, 0, _yDefMaxValue);
            plotControl.Plot.XLabel("时间 (秒)");
            plotControl.Plot.Font.Automatic();
            plotControl.Refresh();
            return plotControl;
        }

        private async Task LoadCurrentModelAsync()
        {
            try
            {
                var result = await _meticulousPursuitApi.GetAllAsync();
                if (!result.IsSuccess || result.Data == null)
                    return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProductModels.Clear();
                    foreach (var model in result.Data)
                        ProductModels.Add(model);
                });

                var current =
                    result.Data.FirstOrDefault(m => m.Station == "A")
                    ?? result.Data.FirstOrDefault();
                if (current != null)
                {
                    _currentRangeDetails = current.Details?.OrderBy(d => d.Time).ToList();
                    SelectedProductModel = current;
                    CurrentModelNo = current.Id;
                }
            }
            catch { }
        }

        private async Task SetCurrentProductionAsync()
        {
            if (SelectedProductModel == null)
                return;

            await _meticulousPursuitApi.SetCurrentStationModelAsync(
                SelectedProductModel.Id,
                "A"
            );

            var result = await _meticulousPursuitApi.GetAllAsync();
            if (result.IsSuccess && result.Data != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProductModels.Clear();
                    foreach (var model in result.Data)
                        ProductModels.Add(model);
                });

                var current = result.Data.FirstOrDefault(m => m.Station == "A");
                if (current != null)
                {
                    _currentRangeDetails = current.Details?.OrderBy(d => d.Time).ToList();
                    SelectedProductModel = current;
                    CurrentModelNo = current.Id;
                }
            }
        }

        private async Task RefreshEnergyRangeAsync()
        {
            try
            {
                var result = await _meticulousPursuitApi.GetAllAsync();
                if (result.IsSuccess && result.Data != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProductModels.Clear();
                        foreach (var model in result.Data)
                            ProductModels.Add(model);
                        CurrentModelNo = result.Data
                            .FirstOrDefault(m => m.Station == "A")
                            ?.Id;
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

        private async Task ImportEnergyRangeAsync()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx",
                Title = "选择能量范围Excel文件",
            };

            if (dialog.ShowDialog() != true)
                return;

            var result = await _meticulousPursuitApi.ImportAsync(dialog.FileName, false);

            if (!result.IsSuccess && result.Message.Contains("已存在"))
            {
                var confirm = HandyControl.Controls.MessageBox.Show(
                    result.Message,
                    "确认覆盖",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (confirm == MessageBoxResult.Yes)
                    result = await _meticulousPursuitApi.ImportAsync(dialog.FileName, true);
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
                await _meticulousPursuitApi.DeleteNavAsync(SelectedProductModel.Id);
                ProductModels.Remove(SelectedProductModel);
                SelectedProductModel = null;
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
                var tmp = new Coordinates(d.Time, (double)d.LowerLimit);
                coords.Add(tmp);
                if (tmp.X > xMax)
                    xMax = tmp.X;
                if (tmp.Y > yMax)
                    yMax = tmp.Y;
            }
            foreach (var d in Enumerable.Reverse(sorted))
            {
                coords.Add(new Coordinates(d.Time, (double)d.UpperLimit));
            }

            if (coords.Count >= 3)
            {
                var polygon = SettingPlotControl.Plot.Add.Polygon(coords.ToArray());
                polygon.FillStyle.Color = Colors.Transparent;
                polygon.LineStyle.Color = Colors.Red;
                polygon.LineStyle.Width = 2;
                polygon.LineStyle.Pattern = LinePattern.Solid;
            }

            SettingPlotControl.Plot.Axes.SetLimits(0, xMax + 2.5, 0, yMax + 500);
            SettingPlotControl.Refresh();
        }

        #region 能量监控查询
        private async Task EnergyRangeQueryAsync()
        {
            var startTime = EnergyQueryStartDate.Date + EnergyQueryStartTime.TimeOfDay;
            var endTime = EnergyQueryEndDate.Date + EnergyQueryEndTime.TimeOfDay;
            if (startTime > endTime)
            {
                HandyControl.Controls.MessageBox.Warning("起始时间不能大于结束时间！");
                return;
            }

            EnergyRangePagination.PageIndex = 1;
            await LoadEnergyRangesAsync();
        }

        private async Task EnergyRangePageUpdated()
        {
            await LoadEnergyRangesAsync();
        }

        private async Task LoadEnergyRangesAsync()
        {
            try
            {
                var request = new EnergyRangeQueryRequestDto
                {
                    StartTime = EnergyQueryStartDate.Date + EnergyQueryStartTime.TimeOfDay,
                    EndTime = EnergyQueryEndDate.Date + EnergyQueryEndTime.TimeOfDay,
                    PageIndex = EnergyRangePagination.PageIndex,
                    DataCountPerPage = EnergyRangePagination.DataCountPerPage,
                };

                var result = await _meticulousPursuitApi.GetEnergyRangesAsync(request);
                if (!result.IsSuccess)
                {
                    HandyControl.Controls.MessageBox.Warning(result.Message ?? "查询失败");
                    return;
                }

                EnergyRangeItems.Clear();
                foreach (var item in result.Data.List ?? new List<Tb_Factory6Workshop6_3Line_EnergyRange>())
                    EnergyRangeItems.Add(item);

                EnergyRangePagination.TotalCount = result.Data.TotalCount;
                EnergyRangePagination.MaxPageCount = result.Data.TotalPage;

                SelectedEnergyRange = null;
                EnergyPointItems.Clear();
                EnergyPointPlot.Plot.Clear();
                EnergyPointPlot.Refresh();
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"查询失败：{ex.Message}");
            }
        }

        private async Task LoadEnergyPointsAsync()
        {
            if (SelectedEnergyRange == null)
                return;

            try
            {
                // 已知子项 Count，按 RecordTime 条件一次查出全部点，不分页
                var request = new EnergyRangePointQueryRequestDto
                {
                    EnergyRangeRecordTime = SelectedEnergyRange.RecordTime,
                    Count = SelectedEnergyRange.Count,
                };

                var result = await _meticulousPursuitApi.GetEnergyRangePointsAsync(request);
                if (!result.IsSuccess)
                {
                    HandyControl.Controls.MessageBox.Warning(result.Message ?? "查询失败");
                    return;
                }

                EnergyPointItems.Clear();
                EnergyPointItems.Add(new Tb_Factory6Workshop6_3Line_EnergyRangePoint() { Time=0, Value=0 });
                foreach (var item in result.Data.List ?? new List<Tb_Factory6Workshop6_3Line_EnergyRangePoint>())
                    EnergyPointItems.Add(item);

                DrawEnergyPointPlot();
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"查询失败：{ex.Message}");
            }
        }

        private void DrawEnergyPointPlot()
        {
            EnergyPointPlot.Plot.Clear();

            if (EnergyPointItems.Count < 2)
            {
                EnergyPointPlot.Refresh();
                return;
            }

            var xs = EnergyPointItems.Select(p => p.Time).ToArray();
            var ys = EnergyPointItems.Select(p => p.Value).ToArray();
            var scatter = EnergyPointPlot.Plot.Add.Scatter(xs, ys, Colors.Green);
            scatter.LineWidth = 2;
            scatter.MarkerSize = 0;

            double xMax = EnergyPointItems.Max(p => p.Time);
            double yMax = EnergyPointItems.Max(p => p.Value);
            EnergyPointPlot.Plot.Axes.SetLimits(0, xMax + 0.5, 0, yMax + 30);
            EnergyPointPlot.Refresh();
        }
        #endregion

        #region 精追
        async Task Query()
        {
            //默认查询第一页
            bool flowControl = await QuData(1);
            if (!flowControl)
            {
                return;
            }
        }

        private void RemoveHistoryScan(ScanRecord record)
        {
            if (record != null)
            {
                HistoryScanMarkingNos.Remove(record);

                QueryValue.Remove(QueryValue.FirstOrDefault(x => x.MarkingNo == record.MarkingNo));
            }
        }

        private void ClearHistoryMarkingNo()
        {
            HistoryScanMarkingNos.Clear();
            _mkNoQuValues.Clear();
            QueryValue.Clear();
        }

        private void OpenScanCodeModel(System.Windows.Controls.TextBox textBox)
        {
            if (QueryConditions?.IsScanCode == true)
            {
                _quValues = QueryValue.ToList();
                QueryValue.Clear();
                foreach (var item in _mkNoQuValues)
                {
                    QueryValue.Add(item);
                }
                Keyboard.Focus(textBox);
            }
            else
            { //无扫码模式
                _mkNoQuValues = QueryValue.ToList();
                QueryValue.Clear();
                foreach (var item in _quValues)
                {
                    QueryValue.Add(item);
                }
            }
        }

        async Task PageUpdated()
        {
            await QuData(PaginationPages.PageIndex);
        }

        private async Task<bool> QuData(int pageIndex)
        {
            IsLoading = true;
            try
            {
                var result = await _meticulousPursuitApi.QueryablToPagee(
                    GetQueryRequest(pageIndex)
                );
                if (result.IsSuccess)
                {
                    QueryValue.Clear();
                    foreach (var item in result.Data.List)
                    {
                        QueryValue.Add(new HeatTreatmentDataDto(item));
                    }
                    PaginationPages.TotalCount = result.Data.TotalCount;
                    PaginationPages.MaxPageCount = result.Data.TotalPage;
                }
                else
                {
                    HandyControl.Controls.MessageBox.Error(result.Message);
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"查询数据时发生错误: {ex.Message}");
                return false;
            }
            finally
            {
                IsLoading = false;
            }
            return true;
        }

        HeatTreatmentDataQueryRequestDto GetQueryRequest(int pageIndex)
        {
            HeatTreatmentDataQueryRequestDto queryRequest = new HeatTreatmentDataQueryRequestDto();
            queryRequest.PageIndex = pageIndex;
            queryRequest.DataCountPerPage = PaginationPages.DataCountPerPage;
            queryRequest.RefinedSearch = QueryConditions;
            return queryRequest;
        }

        private async Task MarkingNoQuery(System.Windows.Controls.TextBox textBox)
        {
            if (string.IsNullOrEmpty(QueryConditions?.MarkingNo))
            {
                return;
            }

            var request = new HeatTreatmentDataQueryRequestDto
            {
                RefinedSearch = new RefinedSearchCriteria { MarkingNo = QueryConditions.MarkingNo },
            };
            var apiResult = await _meticulousPursuitApi.MarkingNoQuery(request);

            HeatTreatmentDataDto findValue;
            if (apiResult.IsSuccess && apiResult.Data?.List?.Count > 0)
            {
                findValue = new HeatTreatmentDataDto(apiResult.Data.List.First());
            }
            else
            {
                findValue = HeatTreatmentDataDto.NotFindMakringNo(QueryConditions.MarkingNo);
            }

            var historyScanMarkingNo = HistoryScanMarkingNos.FirstOrDefault(x =>
                x.MarkingNo == QueryConditions.MarkingNo
            );

            if (historyScanMarkingNo != null)
            {
                HistoryScanMarkingNos.Remove(historyScanMarkingNo);
                HistoryScanMarkingNos.Insert(0, historyScanMarkingNo);
                var tmp = QueryValue.FirstOrDefault(x => x.MarkingNo == QueryConditions.MarkingNo);
                if (tmp != null)
                {
                    QueryValue.Remove(tmp);
                }
            }
            else
            {
                HistoryScanMarkingNos.Insert(
                    0,
                    new ScanRecord()
                    {
                        MarkingNo = QueryConditions.MarkingNo,
                        IsHave = findValue.IsHave,
                    }
                );
            }

            QueryValue.Insert(0, findValue);

            for (int i = 0; i <= HistoryScanMarkingNos.Count - 101; i++)
            {
                HistoryScanMarkingNos.RemoveAt(HistoryScanMarkingNos.Count - 1);
                QueryValue.RemoveAt(QueryValue.Count - 1);
            }
            textBox.Text = string.Empty;
        }

        private async void SaveQuery()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx",
                Title = "保存Excel文件",
                FileName = $"数据导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx",
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            // 扫码模式：直接导出当前已加载的数据（数据量小）
            if (QueryConditions?.IsScanCode == true)
            {
                if (HistoryScanMarkingNos.Count == 0)
                {
                    HandyControl.Controls.MessageBox.Warning("暂无可导出的扫码数据！");
                    return;
                }

                var scanData = QueryValue.ToList();
                if (scanData.Count == 0)
                {
                    HandyControl.Controls.MessageBox.Warning(
                        "未检测到导出的数据信息，请检测搜索条件后再次导出！"
                    );
                    return;
                }
                var scanEx = Expand.ExportToExcel(scanData, saveFileDialog.FileName);
                if (scanEx.IsSuccess)
                {
                    HandyControl.Controls.MessageBox.Success("导出完成！");
                }
                else
                {
                    HandyControl.Controls.MessageBox.Warning($"导出失败！/r/n{scanEx.Message}");
                }
                return;
            }

            // 时间范围查询：分页导出，边查边写，避免大数据量占用内存
            var startTime =
                QueryConditions.StartDate.Date + QueryConditions.StartTime.TimeOfDay;
            var endTime = QueryConditions.EndDate.Date + QueryConditions.EndTime.TimeOfDay;
            if (startTime > endTime)
            {
                HandyControl.Controls.MessageBox.Warning("起始时间不能大于结束时间！");
                return;
            }

            const int exportPageSize = 5000;
            var request = new HeatTreatmentDataQueryRequestDto
            {
                RefinedSearch = QueryConditions,
                PageIndex = 1,
                DataCountPerPage = exportPageSize
            };

            var firstPage = await _meticulousPursuitApi.QueryablToPagee(request);

            if (!firstPage.IsSuccess)
            {
                HandyControl.Controls.MessageBox.Warning($"查询失败：{firstPage.Message}");
                return;
            }

            if (firstPage.Data?.List == null || firstPage.Data.List.Count == 0)
            {
                HandyControl.Controls.MessageBox.Warning(
                    "未检测到导出的数据信息，请检测搜索条件后再次导出！"
                );
                return;
            }

            try
            {
                using (
                    ExcelExportWriter<HeatTreatmentDataDto> writer =
                        new ExcelExportWriter<HeatTreatmentDataDto>(saveFileDialog.FileName)
                )
                {
                    writer.WriteRows(
                        firstPage.Data.List.Select(x => new HeatTreatmentDataDto(x))
                    );

                    for (int page = 2; page <= firstPage.Data.TotalPage; page++)
                    {
                        request.PageIndex = page;
                        var apiResult = await _meticulousPursuitApi.QueryablToPagee(request);
                        if (!apiResult.IsSuccess)
                        {
                            HandyControl.Controls.MessageBox.Warning(
                                $"查询失败：{apiResult.Message}"
                            );
                            return;
                        }
                        writer.WriteRows(
                            apiResult.Data.List.Select(x => new HeatTreatmentDataDto(x))
                        );
                    }
                }

                HandyControl.Controls.MessageBox.Success("导出完成！");
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Warning($"导出失败！/r/n{ex.Message}");
            }
        }
    }
        #endregion
}
