using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

using SkiaSharp;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.SignalDTO.Messages;
using Microsoft.Extensions.Logging;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the general overview / dashboard page. Loads earnings and IP info from
/// <see cref="IOutboundPortEVisitorApiProvider"/>, displays BTP sums for day/month/year and a chart that
/// can pivot by hour, day, or month. Refreshes data on a timer (e.g. at minute 5) and keeps
/// chart and labels in sync on the UI thread via <see cref="DispatcherQueue"/>.
/// </summary>
public sealed partial class ViewModelGeneralOverview : ObservableObject,
                                                      IDisposable,
                                                      IRecipient<ApiCredentialsUpdatedMessage>,
                                                      IRecipient<ApiCredentialsRemovedMessage>
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const double ChartValueChangeEpsilon = 0.0001;
    private const int ColumnSeriesCornerRadius = 5;
    private const int DaysPerMonthLabelCount = 31;
    private const int EarningsRefreshTriggerMinute = 5;
    private const int EarningsRefreshTriggerSecond = 0;
    private const int InitialHourlyChartSlotCount = 24;
    private const int MidnightHour = 0;
    private const int TimerTickIntervalSeconds = 1;
    private const int YAxisMinStep = 100;

    private const string BtpDisplayFormat = "BTP: {0:N0}";
    private const string DateFormatDayMonthYear = "dd.MM.yyyy";
    private const string DateFormatMonthFull = "MMMM";
    private const string DateFormatTimeHourMinute = "HH:mm";
    private const string DateFormatYearFull = "yyyy";
    private const string DefaultPlaceholderDash = "-";
    private const string RouteNameOptions = "Options";

    private const string KeyApiIsNotEnabled = "API_isNotEnabled";
    private const string KeyChartFakeDataMode = "Chart_FakeDataMode";
    private const string KeyChartMonthsShort = "Chart_MonthsShort";
    private const string KeyChartSeriesEarnings = "Chart_SeriesEarnings";
    private const string KeyChartTitleDaily = "Chart_TitleDaily";
    private const string KeyChartTitleHourly = "Chart_TitleHourly";
    private const string KeyChartTitleOverview = "Chart_TitleOverview";
    private const string KeyChartTitleYearly = "Chart_TitleYearly";
    private const string KeyChartXAxisDay = "Chart_XAxisDay";
    private const string KeyChartXAxisMonth = "Chart_XAxisMonth";
    private const string KeyChartXAxisTime = "Chart_XAxisTime";
    private const string KeyChartYAxisPoints = "Chart_YAxisPoints";
    private const string KeyGeneralNextRefresh = "General_NextRefresh";
    private const string KeyGeneralToday = "General_Today";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies (alphabetical A–Z) ──
    private readonly IOutboundPortEVisitorConfigRepository _configService;
    private readonly ILogger<ViewModelGeneralOverview> _logger;
    private readonly IOutboundPortEVisitorApiProvider _eVisitorApiService;
    private readonly IInboundPortLocalizationProvider _localizationService;
    private readonly INavigationService _navigationService;

    // ── Block 2: Primitives / Primitive wrappers (alphabetical A–Z) ──
    private volatile bool _disposed;
    private bool _isApiConfigured;

    // ── Block 4: Complex types / Repositories / Objects (alphabetical A–Z) ──
    private EarningsData? _cachedEarnings;
    private readonly ObservableCollection<ObservableValue> _chartValues;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly ColumnSeries<ObservableValue> _mainColumnSeries;
    private readonly DispatcherTimer _timer;

    // ⚡ Performance optimization: X-axes configurations for each pivot index are prebuilt once
    // in the constructor to eliminate per-binding-read array and LINQ closure allocations.
    private readonly ICartesianAxis[][] _xAxesByPivotIndex;


    // ═══════════════════════════════════════════════════════
    //  3. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Sets up API and localization, creates the chart series and 24-slot value collection,
    /// starts a 1-second timer to trigger refresh at minute 5, and kicks off the first async load
    /// so the UI gets earnings and IP data as soon as possible.
    /// </summary>
    public ViewModelGeneralOverview(
        INavigationService navigationService,
        IOutboundPortEVisitorApiProvider eVisitorApiService,
        IInboundPortLocalizationProvider localizationService,
        ILogger<ViewModelGeneralOverview> logger,
        IOutboundPortEVisitorConfigRepository configService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(eVisitorApiService);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(configService);

        _logger = logger;

        _navigationService = navigationService;
        _eVisitorApiService = eVisitorApiService;
        _localizationService = localizationService;
        _configService = configService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        var config = _configService.LoadConfig();
        _isApiConfigured = !string.IsNullOrEmpty(config?.Settings?.ApiKey);

        CurrentDay = _localizationService.RetrieveString(KeyGeneralToday);

        YAxes =
        [
            new Axis
            {
                Name = _localizationService.RetrieveString(KeyChartYAxisPoints),
                LabelsDensity = 1,
                SeparatorsPaint = new SolidColorPaint(new SKColor(40, 40, 40)) { StrokeThickness = 1 },
                MinStep = YAxisMinStep,
                MinLimit = 0,
                TextSize = 12
            }
        ];

        _chartValues = [];

        for (int index = 0; index < InitialHourlyChartSlotCount; index++)
        {
            _chartValues.Add(new ObservableValue(0));
        }

        _mainColumnSeries = new ColumnSeries<ObservableValue>
        {
            Values = _chartValues,
            Name = _localizationService.RetrieveString(KeyChartSeriesEarnings),
            Rx = ColumnSeriesCornerRadius,
            Ry = ColumnSeriesCornerRadius,
            Fill = new SolidColorPaint(new SKColor(0, 120, 215)),
            DataLabelsPaint = new SolidColorPaint(new SKColor(12, 142, 168)),
            DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top
        };

        Series = [_mainColumnSeries];

        _xAxesByPivotIndex = BuildXAxesForAllPivots();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(TimerTickIntervalSeconds) };
        _timer.Tick += OnTimerTick;
        _timer.Start();

        Task.Run(LoadDataAsync);

        // Messenger subscription: RegisterAll with IRecipient<T> prevents closure memory leaks,
        // allowing handlers to be cleanly unregistered in Dispose().
        WeakReferenceMessenger.Default.RegisterAll(this);
    }


    // ═══════════════════════════════════════════════════════
    //  6. Properties
    // ═══════════════════════════════════════════════════════
    /// <summary>Gets the chart title formatted based on current pivot and API status.</summary>
    public string ChartTitle
    {
        get
        {
            if (!_isApiConfigured && !UseScreenshotFakeData)
            {
                return _localizationService.RetrieveString(KeyApiIsNotEnabled);
            }

            string title = SelectedPivotIndex switch
            {
                0 => string.Format(_localizationService.RetrieveString(KeyChartTitleHourly), DateTime.Now.ToString(DateFormatDayMonthYear)),
                1 => string.Format(_localizationService.RetrieveString(KeyChartTitleDaily), DateTime.Now.ToString(DateFormatMonthFull)),
                2 => string.Format(_localizationService.RetrieveString(KeyChartTitleYearly), DateTime.Now.ToString(DateFormatYearFull)),
                _ => _localizationService.RetrieveString(KeyChartTitleOverview)
            };

            if (UseScreenshotFakeData)
            {
                title += _localizationService.RetrieveString(KeyChartFakeDataMode);
            }

            return title;
        }
    }

    /// <summary>Gets or sets the formatted next earnings refresh clock display string.</summary>
    [ObservableProperty]
    public partial string ClockNextEarningsRefresh { get; set; } = DefaultPlaceholderDash;

    /// <summary>Gets or sets the IP country code.</summary>
    [ObservableProperty]
    public partial string CountryCode { get; set; } = DefaultPlaceholderDash;

    /// <summary>Gets or sets the IP country name.</summary>
    [ObservableProperty]
    public partial string CountryName { get; set; } = DefaultPlaceholderDash;

    /// <summary>Gets or sets the current day label.</summary>
    [ObservableProperty]
    public partial string CurrentDay { get; set; } = DefaultPlaceholderDash;

    /// <summary>Gets or sets the current month label.</summary>
    [ObservableProperty]
    public partial string CurrentMonth { get; set; } = DateTime.Now.ToString(DateFormatMonthFull);

    /// <summary>Gets or sets the current year label.</summary>
    [ObservableProperty]
    public partial string CurrentYear { get; set; } = DateTime.Now.ToString(DateFormatYearFull);

    /// <summary>Gets or sets the BTP sum for today.</summary>
    [ObservableProperty]
    public partial string EarningsThisDaySum { get; set; } = DefaultPlaceholderDash;

    /// <summary>Gets or sets the BTP sum for this month.</summary>
    [ObservableProperty]
    public partial string EarningsThisMonthSum { get; set; } = DefaultPlaceholderDash;

    /// <summary>Gets or sets the BTP sum for this year.</summary>
    [ObservableProperty]
    public partial string EarningsThisYearSum { get; set; } = DefaultPlaceholderDash;

    /// <summary>Gets or sets the host name.</summary>
    [ObservableProperty]
    public partial string Host { get; set; } = DefaultPlaceholderDash;

    /// <summary>Gets or sets the public IP address.</summary>
    [ObservableProperty]
    public partial string IpAddress { get; set; } = DefaultPlaceholderDash;

    /// <summary>Gets or sets the selected pivot index (0 = Hour, 1 = Day, 2 = Year).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(XAxes))]
    [NotifyPropertyChangedFor(nameof(ChartTitle))]
    public partial int SelectedPivotIndex { get; set; } = 0;

    /// <summary>Gets or sets the LiveCharts series collection.</summary>
    [ObservableProperty]
    public partial ObservableCollection<ISeries> Series { get; set; }

    /// <summary>Set this to true to automatically generate fake values (chart and BTP totals) for screenshots.</summary>
    public bool UseScreenshotFakeData { get; set; } = false;

    /// <summary>X-axis depends on pivot: hour, day, or month labels. Served from a prebuilt, allocation-free cache.</summary>
    /// <remarks>
    /// Typed as <see cref="ICartesianAxis"/> because that is what LiveCharts' CartesianChart.XAxes
    /// expects; <c>{x:Bind}</c> verifies binding types at compile time (Guide Kap. 23.1) and does
    /// not accept the previous <c>Axis[]</c> declaration.
    /// </remarks>
    public IEnumerable<ICartesianAxis> XAxes => _xAxesByPivotIndex[Math.Clamp(SelectedPivotIndex, 0, _xAxesByPivotIndex.Length - 1)];

    /// <summary>Y-axis configuration for the chart (e.g. points label and separators).</summary>
    public IEnumerable<ICartesianAxis> YAxes { get; set; } = null!;


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Stops the refresh timer, unsubscribes its handler and detaches all messenger registrations
    /// so no callback can keep this view model alive after teardown (Guide Kap. 22.6 / 22.10).
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _timer.Stop();
        _timer.Tick -= OnTimerTick;

        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    /// <summary>Handles activation of API credentials: enables API mode and reloads earnings.</summary>
    public void Receive(ApiCredentialsUpdatedMessage message)
    {
        if (_disposed)
        {
            return;
        }

        _isApiConfigured = true;
        _dispatcherQueue.TryEnqueue(() => OnPropertyChanged(nameof(ChartTitle)));
        Task.Run(LoadDataAsync);
    }

    /// <summary>Handles removal of API credentials: clears chart, sums and refresh clock.</summary>
    public void Receive(ApiCredentialsRemovedMessage message)
    {
        if (_disposed)
        {
            return;
        }

        _isApiConfigured = false;

        _dispatcherQueue.TryEnqueue(() =>
        {
            ResetChart();
            _cachedEarnings = null;
            UpdateChartData();
            EarningsThisDaySum = DefaultPlaceholderDash;
            EarningsThisMonthSum = DefaultPlaceholderDash;
            EarningsThisYearSum = DefaultPlaceholderDash;
            ClockNextEarningsRefresh = DefaultPlaceholderDash;
            OnPropertyChanged(nameof(ChartTitle));
        });
    }

    /// <summary>
    /// Generates synthetic earnings data array for screenshot demonstration mode.
    /// </summary>
    /// <returns>Array of generated fake earnings values matching the selected pivot.</returns>
    private double[] GenerateFakeEarningsData()
    {
        var rnd = new Random(SelectedPivotIndex);
        int count = SelectedPivotIndex switch
        {
            0 => 24,
            1 => DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month),
            2 => 12,
            _ => 0
        };

        var earningsValuesForPivot = new double[count];
        for (int i = 0; i < count; i++)
        {
            earningsValuesForPivot[i] = SelectedPivotIndex switch
            {
                0 => rnd.Next(500, 801),
                1 => rnd.Next(12000, 17001),
                2 => rnd.Next(500000, 650001),
                _ => 0
            };
        }
        return earningsValuesForPivot;
    }

    /// <summary>
    /// Retrieves cached earnings data array matching the currently selected pivot index.
    /// </summary>
    /// <returns>Array of real earnings values for the active pivot.</returns>
    private double[] GetRealEarningsData()
    {
        return SelectedPivotIndex switch
        {
            0 => _cachedEarnings!.HourlyEarnings,
            1 => _cachedEarnings!.DailyEarnings,
            2 => _cachedEarnings!.MonthlyEarnings,
            _ => []
        };
    }

    /// <summary>
    /// Builds the X-axis configuration for every pivot exactly once (hour, day of month, month names).
    /// Labels and localized names are constant for the lifetime of the view model, so caching them
    /// removes all per-binding-read allocations (Guide Kap. 12 / Kap. 21).
    /// </summary>
    private ICartesianAxis[][] BuildXAxesForAllPivots()
    {
        // Pivot 1: Days 1–31 – generated once without LINQ chains or closures.
        var dayOfMonthLabels = new string[DaysPerMonthLabelCount];

        for (int day = 1; day <= DaysPerMonthLabelCount; day++)
        {
            dayOfMonthLabels[day - 1] = day.ToString(CultureInfo.InvariantCulture);
        }

        string monthsString = _localizationService.RetrieveString(KeyChartMonthsShort);

        string[] monthLabels = !string.IsNullOrEmpty(monthsString)
            ? monthsString.Split(',')
            : ["Jan", "Feb", "Mär", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dez"];

        return
        [
            [new Axis
            {
                TextSize = 12,
                LabelsRotation = 0,
                Name = _localizationService.RetrieveString(KeyChartXAxisTime)
            }],
            [new Axis
            {
                TextSize = 12,
                LabelsRotation = 0,
                Name = _localizationService.RetrieveString(KeyChartXAxisDay),
                Labels = dayOfMonthLabels
            }],
            [new Axis
            {
                TextSize = 12,
                LabelsRotation = 0,
                Name = _localizationService.RetrieveString(KeyChartXAxisMonth),
                Labels = monthLabels
            }]
        ];
    }

    /// <summary>Navigates the user to the Options / API settings page.</summary>
    [RelayCommand]
    private void GoToAPILogin() => _navigationService.NavigateTo(RouteNameOptions, parameter: 1);

    /// <summary>
    /// Fetches earnings and IP info in parallel from the API, then on the UI thread updates
    /// IP fields, BTP sums, chart data, and the next-refresh time. Keeps chart updates on the
    /// dispatcher so LiveCharts and bindings stay consistent.
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            IpInfoData? ipInfo = null;

            if (!UseScreenshotFakeData)
            {
                Task<EarningsData?> earningsTask = _eVisitorApiService.RetrieveEarningsAsync();
                Task<IpInfoData?> ipInfoTask = _eVisitorApiService.RetrieveIpInfoAsync();

                await Task.WhenAll(earningsTask, ipInfoTask);

                _cachedEarnings = await earningsTask;
                ipInfo = await ipInfoTask;
            }

            _dispatcherQueue.TryEnqueue(() =>
            {
                if (ipInfo != null)
                {
                    IpAddress = ipInfo.IpAddress;
                    Host = ipInfo.Hostname;
                    CountryCode = ipInfo.CountryCode;
                    CountryName = ipInfo.CountryName;
                }

                if (UseScreenshotFakeData)
                {
                    EarningsThisDaySum = string.Format(BtpDisplayFormat, 16450);
                    EarningsThisMonthSum = string.Format(BtpDisplayFormat, 425800);
                    EarningsThisYearSum = string.Format(BtpDisplayFormat, 5120000);
                    UpdateChartData();
                }
                else if (_cachedEarnings != null)
                {
                    EarningsThisDaySum = string.Format(BtpDisplayFormat, _cachedEarnings.TodaySum);
                    EarningsThisMonthSum = string.Format(BtpDisplayFormat, _cachedEarnings.MonthlySum);
                    EarningsThisYearSum = string.Format(BtpDisplayFormat, _cachedEarnings.YearlySum);
                    UpdateChartData();
                }

                var now = DateTime.Now;

                CurrentMonth = now.ToString(DateFormatMonthFull);
                CurrentYear = now.ToString(DateFormatYearFull);

                var nextRefresh = now.AddMinutes(60 - now.Minute + EarningsRefreshTriggerMinute);
                var nextRefreshTimeFormat = _localizationService.RetrieveString(KeyGeneralNextRefresh);
                ClockNextEarningsRefresh = _isApiConfigured
                    ? string.Format(nextRefreshTimeFormat, nextRefresh.ToString(DateFormatTimeHourMinute))
                    : DefaultPlaceholderDash;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                LogEventIds.Api.EarningsRetrievalFailed,
                ex,
                "Refreshing the earnings overview failed.");
        }
    }

    /// <summary>
    /// Handles changes to <see cref="SelectedPivotIndex"/> by updating chart series data.
    /// </summary>
    /// <param name="value">The new selected pivot index.</param>
    partial void OnSelectedPivotIndexChanged(int value)
    {
        UpdateChartData();
    }

    /// <summary>At minute 5 and second 0, refreshes data; at 00:05 also resets chart values for the new day.</summary>
    private void OnTimerTick(object? sender, object eventArgs)
    {
        // Guard against timer callbacks firing after disposal.
        if (_disposed)
        {
            return;
        }

        var now = DateTime.Now;
        if (now.Minute == EarningsRefreshTriggerMinute && now.Second == EarningsRefreshTriggerSecond)
        {
            if (now.Hour == MidnightHour)
            {
                ResetChart();
            }
            Task.Run(LoadDataAsync);
        }
    }

    /// <summary>
    /// Resets all values in the chart collection to zero.
    /// </summary>
    private void ResetChart()
    {
        foreach (var chartValue in _chartValues)
        {
            chartValue.Value = 0;
        }
    }

    /// <summary>
    /// Synchronizes the observable chart value collection with the target earnings array, updating only changed values.
    /// </summary>
    /// <param name="earningsValuesForPivot">The target values for the current pivot.</param>
    private void SyncChartValues(double[] earningsValuesForPivot)
    {
        while (_chartValues.Count < earningsValuesForPivot.Length)
        {
            _chartValues.Add(new ObservableValue(0));
        }

        while (_chartValues.Count > earningsValuesForPivot.Length)
        {
            _chartValues.RemoveAt(_chartValues.Count - 1);
        }

        for (int index = 0; index < earningsValuesForPivot.Length; index++)
        {
            double currentValue = _chartValues[index].Value ?? 0.0;

            if (Math.Abs(currentValue - earningsValuesForPivot[index]) > ChartValueChangeEpsilon)
            {
                _chartValues[index].Value = earningsValuesForPivot[index];
            }
        }
    }

    /// <summary>Maps cached earnings to the chart series for the current pivot (hourly/daily/monthly); resizes collection if needed and only updates changed values.</summary>
    private void UpdateChartData()
    {
        if (_cachedEarnings == null && !UseScreenshotFakeData)
        {
            return;
        }

        double[] earningsValuesForPivot = UseScreenshotFakeData
            ? GenerateFakeEarningsData()
            : GetRealEarningsData();

        SyncChartValues(earningsValuesForPivot);
    }
}
