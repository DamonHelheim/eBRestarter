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
        private readonly IEVisitorApiService _apiService;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly DispatcherTimer _timer;
        private EarningsData? _cachedEarnings;

        // --- UI Properties ---

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(XAxes))]
        [NotifyPropertyChangedFor(nameof(ChartTitle))] // Titel hängt auch vom Pivot ab
        public partial int SelectedPivotIndex { get; set; } = 0;

        // FIX: Series muss ein ObservableProperty sein, damit die UI Änderungen mitbekommt
        [ObservableProperty]
        public partial ObservableCollection<ISeries> Series { get; set; }

        // Lokaler Zugriff auf die ColumnSeries, um Daten später zu pushen
        private readonly ColumnSeries<ObservableValue> _mainColumnSeries;

        // Deine ObservableCollection für die Werte
        private readonly ObservableCollection<ObservableValue> _chartValues;

        public Axis[] YAxes { get; set; } =
        {
            new Axis
            {
                Name = "Punkte",
                // 1. Farbe für den TITEL ("Punkte") -> Schwarz
        NamePaint = new SolidColorPaint(SKColors.DarkGray), 
        
        LabelsDensity = 1,

        SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200))
        {
            StrokeThickness = 1,
        },

        // 2. Farbe für die ZAHLEN (0, 100, 200...) -> Schwarz
        LabelsPaint = new SolidColorPaint(SKColors.Black),
                MinStep = 100,
                TextSize = 12
            }
        };

        public Axis[] XAxes => GetXAxesForCurrentPivot();

        // UI Texte
        public string ChartTitle => SelectedPivotIndex switch
        {
            0 => $"Stundenübersicht ({DateTime.Now:dd.MM.yyyy})",
            1 => $"Tagesübersicht ({DateTime.Now:MMMM})",
            2 => $"Jahresübersicht ({DateTime.Now:yyyy})",
            _ => "Übersicht"
        };

        [ObservableProperty] public partial string EarningsThisDaySum { get; set; } = "-";
        [ObservableProperty] public partial string EarningsThisMonthSum { get; set; } = "-";
        [ObservableProperty] public partial string EarningsThisYearSum { get; set; } = "-";
        [ObservableProperty] public partial string CurrentDay { get; set; } = "Heute";
        [ObservableProperty] public partial string CurrentMonth { get; set; } = DateTime.Now.ToString("MMMM");
        [ObservableProperty] public partial string CurrentYear { get; set; } = DateTime.Now.ToString("yyyy");
        [ObservableProperty] public partial string ClockNextEarningsRefresh { get; set; } = "-";
        [ObservableProperty] public partial string IpAddress { get; set; } = "-";
        [ObservableProperty] public partial string Host { get; set; } = "-";
        [ObservableProperty] public partial string CountryCode { get; set; } = "-";
        [ObservableProperty] public partial string CountryName { get; set; } = "-";

        public ViewModelGeneralOverview(IEVisitorApiService apiService)
        {
            _apiService = apiService;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // 1. Werte initialisieren
            _chartValues = [];

            for (int i = 0; i < 24; i++) _chartValues.Add(new ObservableValue(0));

            // 2. Series Setup
            _mainColumnSeries = new ColumnSeries<ObservableValue>
            {
                Values = _chartValues,
                Name = "Verdienst",

                // HIER: Ecken abrunden
                Rx = 200, // Radius in Pixeln (Horizontal)
                Ry = 200, // Radius in Pixeln (Vertikal)

                Fill = new SolidColorPaint(new SKColor(0, 120, 215)),
                DataLabelsPaint = new SolidColorPaint(new SKColor(12, 142, 168)),
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top
            };

            // Initialisierung der Property
            Series = [_mainColumnSeries];

            // 3. Timer Setup
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTimerTick;
            _timer.Start();

            Task.Run(LoadDataAsync);
        }

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
                    ClockNextEarningsRefresh = $"Nächste Aktualisierung: {nextRefresh:HH:mm}";
                });
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung hier wichtig, falls API failt
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        partial void OnSelectedPivotIndexChanged(int value)
        {
            UpdateChartData();
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

            // LiveCharts Performance Optimierung:
            // Statt Clear() und Add(), was die Animationen manchmal stört,
            // gleichen wir die Anzahl der Elemente an und updaten die Werte.

            // 1. Sicherstellen, dass wir genug ObservableValues haben
            while (_chartValues.Count < sourceData.Length)
                _chartValues.Add(new ObservableValue(0));

            while (_chartValues.Count > sourceData.Length)
                _chartValues.RemoveAt(_chartValues.Count - 1);

            // 2. Werte updaten (löst Notification pro Wert aus, was LiveCharts animiert)
            for (int i = 0; i < sourceData.Length; i++)
            {
                if (_chartValues[i].Value != sourceData[i])
                {
                    _chartValues[i].Value = sourceData[i];
                }
            }
        }

        private Axis[] GetXAxesForCurrentPivot()
        {
            var axis = new Axis
            {
                NamePaint = new SolidColorPaint(SKColors.DarkGray),
                LabelsPaint = new SolidColorPaint(SKColors.Black),
                TextSize = 12,
                LabelsRotation = 0
            };

            switch (SelectedPivotIndex)
            {
                case 0:
                    axis.Name = "Uhrzeit (0-23h)";
                    break;
                case 1:
                    axis.Name = "Tag im Monat";
                    break;
                case 2:
                    axis.Name = "Monat";
                    axis.Labels = ["Jan", "Feb", "Mär", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dez"];
                    break;
            }

            return new[] { axis };
        }
    }
}