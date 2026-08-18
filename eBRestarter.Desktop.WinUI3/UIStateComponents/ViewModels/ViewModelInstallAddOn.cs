using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using Microsoft.Extensions.Logging;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the "Install Add-on" dialog. Holds a fixed list of browser add-on status
/// view models (Brave, Chrome, Edge, Firefox, Vivaldi) and refreshes their install/extension state
/// on a timer so the user sees up-to-date status without manually refreshing.
/// </summary>
public sealed partial class ViewModelInstallAddOn : ObservableObject, IDisposable
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const double BrowserAddonRefreshIntervalMilliseconds = 2000;

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    private readonly IOutboundPortBrowserFactory _browserFactory;
    private readonly ILogger<ViewModelInstallAddOn> _logger;

    private readonly DispatcherTimer _timer;

    private volatile bool _disposed;

    // ⚡ Overlap prevention: Ensures background probe ticks do not stack if a check takes longer than 2 seconds. 0 = free, 1 = active.
    private int _refreshInFlight;


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════
    /// <summary>One entry per supported browser, each showing install and extension status.</summary>
    public ObservableCollection<ViewModelBrowserAddonStatus> Browsers { get; } = [];

    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Creates browser instances for all supported types, wraps each in a
    /// <see cref="ViewModelBrowserAddonStatus"/>, and starts a 2-second timer that refreshes
    /// each entry so install/extension state stays current (e.g. after user installs the add-on).
    /// </summary>
    /// <param name="browserFactory">Used to create browser instances for status checks. Must not be null.</param>
    /// <param name="localizationService">Passed to each ViewModelBrowserAddonStatus for localized strings. Must not be null.</param>
    public ViewModelInstallAddOn(
        IOutboundPortBrowserFactory browserFactory,
        IInboundPortLocalizationProvider localizationService,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(browserFactory);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _browserFactory = browserFactory;
        _logger = loggerFactory.CreateLogger<ViewModelInstallAddOn>();

        // Logging guideline: Child view models are instantiated manually but receive their own logger category via ILoggerFactory.
        var addonStatusLogger = loggerFactory.CreateLogger<ViewModelBrowserAddonStatus>();

        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Brave), localizationService, addonStatusLogger));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Chrome), localizationService, addonStatusLogger));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Edge), localizationService, addonStatusLogger));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Firefox), localizationService, addonStatusLogger));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Vivaldi), localizationService, addonStatusLogger));

        // ✅ .NET 10 / WinUI 3: Use DispatcherTimer to ensure UI thread safety for binding property changes
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(BrowserAddonRefreshIntervalMilliseconds)
        };
        _timer.Tick += OnBrowserAddonRefreshTimerTick;
        _timer.Start();
    }

    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>Stops the refresh timer so the dialog can close without background updates.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _timer.Stop();
        _timer.Tick -= OnBrowserAddonRefreshTimerTick;
    }

    /// <summary>
    /// Refreshes status for all browser add-ons asynchronously on timer tick.
    /// </summary>
    /// <param name="sender">The timer instance.</param>
    /// <param name="eventArgs">Event arguments associated with the tick event.</param>
    private async void OnBrowserAddonRefreshTimerTick(object? sender, object eventArgs)
    {
        // Guard against timer callbacks firing after disposal.
        if (_disposed)
        {
            return;
        }

        // Drop overlapping ticks to prevent concurrency buildup.
        if (Interlocked.Exchange(ref _refreshInFlight, 1) == 1)
        {
            return;
        }

        // Declared outside try so the catch block can inspect individual tasks.
        var pendingRefreshes = new List<Task>(Browsers.Count);

        try
        {
            // Independent browser probes execute concurrently via Task.WhenAll.

            foreach (ViewModelBrowserAddonStatus browserAddonStatus in Browsers)
            {
                pendingRefreshes.Add(browserAddonStatus.RefreshStatusAsync());
            }

            await Task.WhenAll(pendingRefreshes);
        }
        catch (Exception)
        {
            // ⚠️ Exception guideline: Task.WhenAll only rethrows the first exception. Inspecting individual
            // faulted tasks and flattening AggregateExceptions ensures all probe failures are logged.
            foreach (var failure in pendingRefreshes
                         .Where(task => task.IsFaulted)
                         .SelectMany(task => task.Exception!.Flatten().InnerExceptions))
            {
                _logger.LogError(
                    LogEventIds.Browser.BrowserProfileDiscoveryFailed,
                    failure,
                    "Refreshing the add-on status of one browser failed.");
            }
        }
        finally
        {
            Volatile.Write(ref _refreshInFlight, 0);
        }
    }
}
