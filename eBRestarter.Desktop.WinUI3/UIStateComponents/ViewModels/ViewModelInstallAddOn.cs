using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;

using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;

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

    private readonly DispatcherTimer _timer;


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
        IInboundPortLocalizationProvider localizationService)
    {
        ArgumentNullException.ThrowIfNull(browserFactory);
        ArgumentNullException.ThrowIfNull(localizationService);

        _browserFactory = browserFactory;

        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Brave), localizationService));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Chrome), localizationService));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Edge), localizationService));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Firefox), localizationService));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Vivaldi), localizationService));

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
        _timer.Stop();
        _timer.Tick -= OnBrowserAddonRefreshTimerTick;
    }

    private void OnBrowserAddonRefreshTimerTick(object? sender, object eventArgs)
    {
        foreach (ViewModelBrowserAddonStatus browserAddonStatus in Browsers)
        {
            browserAddonStatus.RefreshStatus();
        }
    }
}
