using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Domain.Models.Records;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelGeneralOverview : ObservableObject
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IEVisitorApiService _apiService;
        private readonly ILocalizationService _localizationService;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly DispatcherTimer _timer;
        private EarningsData? _cachedEarnings;
        private readonly ColumnSeries<ObservableValue> _mainColumnSeries;
        private readonly ObservableCollection<ObservableValue> _chartValues;

        #endregion

        // =========================================================
        // 2. OBSERVABLE PROPERTIES (MVVM State)
        // =========================================================
        #region ObservableProperties

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(XAxes))]
        [NotifyPropertyChangedFor(nameof(ChartTitle))]
        public partial int SelectedPivotIndex { get; set; } = 0;

        [ObservableProperty]
        public partial ObservableCollection<ISeries> Series { get; set; }

        [ObservableProperty] public partial string EarningsThisDaySum { get; set; } = "-";
        [ObservableProperty] public partial string EarningsThisMonthSum { get; set; } = "-";
        [ObservableProperty] public partial string EarningsThisYearSum { get; set; } = "-";

        [ObservableProperty] public partial string CurrentDay { get; set; }
        [ObservableProperty] public partial string CurrentMonth { get; set; } = DateTime.Now.ToString("MMMM");
        [ObservableProperty] public partial string CurrentYear { get; set; } = DateTime.Now.ToString("yyyy");
        [ObservableProperty] public partial string ClockNextEarningsRefresh { get; set; } = "-";
        [ObservableProperty] public partial string IpAddress { get; set; } = "-";
        [ObservableProperty] public partial string Host { get; set; } = "-";
        [ObservableProperty] public partial string CountryCode { get; set; } = "-";
        [ObservableProperty] public partial string CountryName { get; set; } = "-";

        #endregion

        // =========================================================
        // 3. PUBLIC PROPERTIES (Data & State)
        // =========================================================
        #region PublicProperties

        public Axis[] YAxes { get; set; }
        public Axis[] XAxes => GetXAxesForCurrentPivot();
        public string ChartTitle => SelectedPivotIndex switch
        {
            0 => string.Format(_localizationService.GetString("Chart_TitleHourly"), DateTime.Now.ToString("dd.MM.yyyy")),
            1 => string.Format(_localizationService.GetString("Chart_TitleDaily"), DateTime.Now.ToString("MMMM")),
            2 => string.Format(_localizationService.GetString("Chart_TitleYearly"), DateTime.Now.ToString("yyyy")),
            _ => _localizationService.GetString("Chart_TitleOverview")
        };

        #endregion

        // =========================================================
        // 4. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        public ViewModelGeneralOverview(
            IEVisitorApiService apiService,
            ILocalizationService localizationService)
        {
            _apiService = apiService;
            _localizationService = localizationService;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            CurrentDay = _localizationService.GetString("General_Today");

            YAxes =
            [
                new Axis
                {
                    Name = _localizationService.GetString("Chart_YAxisPoints"),
                    LabelsDensity = 1,
                    SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)) { StrokeThickness = 1 },
                    MinStep = 100,
                    TextSize = 12
                }
            ];

            _chartValues = [];
            for (int i = 0; i < 24; i++) _chartValues.Add(new ObservableValue(0));

            _mainColumnSeries = new ColumnSeries<ObservableValue>
            {
                Values = _chartValues,
                Name = _localizationService.GetString("Chart_SeriesEarnings"),
                Rx = 200,
                Ry = 200,
                Fill = new SolidColorPaint(new SKColor(0, 120, 215)),
                DataLabelsPaint = new SolidColorPaint(new SKColor(12, 142, 168)),
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top
            };

            Series = [_mainColumnSeries];

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTimerTick;
            _timer.Start();

            Task.Run(LoadDataAsync);
        }

        #endregion

        // =========================================================
        // 5. PROPERTY CHANGE HANDLERS (MVVM Hooks)
        // =========================================================
        #region PropertyChangeHandlers

        partial void OnSelectedPivotIndexChanged(int value)
        {
            UpdateChartData();
        }

        #endregion

        // =========================================================
        // 6. PRIVATE HELPER METHODS (Interne Hilfsmethoden)
        // =========================================================
        #region PrivateHelperMethods

        private async Task LoadDataAsync()
        {
            try
            {
                var earningsTask = _apiService.GetEarningsAsync();
                var ipInfoTask = _apiService.GetIpInfoAsync();

                await Task.WhenAll(earningsTask, ipInfoTask);

                _cachedEarnings = earningsTask.Result;
                var ipData = ipInfoTask.Result;

                _dispatcherQueue.TryEnqueue(() =>
                {
                    if (ipData != null)
                    {
                        IpAddress = ipData.IpAddress;
                        Host = ipData.Hostname;
                        CountryCode = ipData.CountryCode;
                        CountryName = ipData.CountryName;
                    }

                    if (_cachedEarnings != null)
                    {
                        EarningsThisDaySum = $"BTP: {_cachedEarnings.TodaySum:N0}";
                        EarningsThisMonthSum = $"BTP: {_cachedEarnings.MonthlySum:N0}";
                        EarningsThisYearSum = $"BTP: {_cachedEarnings.YearlySum:N0}";
                        UpdateChartData();
                    }

                    var now = DateTime.Now;
                    var nextRefresh = now.AddMinutes(60 - now.Minute + 5);
                    var format = _localizationService.GetString("General_NextRefresh");
                    ClockNextEarningsRefresh = string.Format(format, nextRefresh.ToString("HH:mm"));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private void OnTimerTick(object? sender, object e)
        {
            var now = DateTime.Now;
            if (now.Minute == 5 && now.Second == 0)
            {
                if (now.Hour == 0) ResetChart();
                Task.Run(LoadDataAsync);
            }
        }

        private void ResetChart()
        {
            foreach (var val in _chartValues) val.Value = 0;
        }

        private void UpdateChartData()
        {
            if (_cachedEarnings == null) return;

            double[] sourceData = SelectedPivotIndex switch
            {
                0 => _cachedEarnings.HourlyEarnings,
                1 => _cachedEarnings.DailyEarnings,
                2 => _cachedEarnings.MonthlyEarnings,
                _ => []
            };

            while (_chartValues.Count < sourceData.Length)
                _chartValues.Add(new ObservableValue(0));

            while (_chartValues.Count > sourceData.Length)
                _chartValues.RemoveAt(_chartValues.Count - 1);

            for (int i = 0; i < sourceData.Length; i++)
            {
                if (_chartValues[i].Value != sourceData[i])
                    _chartValues[i].Value = sourceData[i];
            }
        }

        private Axis[] GetXAxesForCurrentPivot()
        {
            var axis = new Axis { TextSize = 12, LabelsRotation = 0 };

            switch (SelectedPivotIndex)
            {
                case 0:
                    axis.Name = _localizationService.GetString("Chart_XAxisTime");
                    break;
                case 1:
                    axis.Name = _localizationService.GetString("Chart_XAxisDay");
                    break;
                case 2:
                    axis.Name = _localizationService.GetString("Chart_XAxisMonth");
                    string monthsString = _localizationService.GetString("Chart_MonthsShort");
                    axis.Labels = !string.IsNullOrEmpty(monthsString)
                        ? monthsString.Split(',')
                        : ["Jan", "Feb", "Mär", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dez"];
                    break;
            }

            return [axis];
        }

        #endregion
    }
}
