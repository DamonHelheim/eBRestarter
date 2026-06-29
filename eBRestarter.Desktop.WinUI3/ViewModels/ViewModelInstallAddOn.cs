using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Ports.Outbound.Providers;
using eBRestarter.Core.Application.Providers;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using System;
using System.Collections.ObjectModel;
using System.Timers;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the "Install Add-on" dialog. Holds a fixed list of browser add-on status
/// view models (Brave, Chrome, Edge, Firefox, Vivaldi) and refreshes their install/extension state
/// on a timer so the user sees up-to-date status without manually refreshing.
/// </summary>
public sealed partial class ViewModelInstallAddOn : ObservableObject, IDisposable
{
    private const double BrowserAddonRefreshIntervalMilliseconds = 2000;

    private readonly IBrowserFactoryPort _browserFactory;

    private readonly Timer _timer;

    /// <summary>One entry per supported browser, each showing install and extension status.</summary>
    public ObservableCollection<ViewModelBrowserAddonStatus> Browsers { get; } = [];


    /// <summary>
    /// Creates browser instances for all supported types, wraps each in a
    /// <see cref="ViewModelBrowserAddonStatus"/>, and starts a 2-second timer that refreshes
    /// each entry so install/extension state stays current (e.g. after user installs the add-on).
    /// </summary>
    /// <param name="BrowserFactory">Used to create browser instances for status checks. Must not be null.</param>
    /// <param name="LocalizationProvider">Passed to each ViewModelBrowserAddonStatus for localized strings. Must not be null.</param>
    public ViewModelInstallAddOn(IBrowserFactoryPort BrowserFactory, ILocalizationProvider LocalizationProvider)
    {
        ArgumentNullException.ThrowIfNull(BrowserFactory);
        ArgumentNullException.ThrowIfNull(LocalizationProvider);

        _browserFactory = BrowserFactory;

        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Brave), LocalizationProvider));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Chrome), LocalizationProvider));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Edge), LocalizationProvider));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Firefox), LocalizationProvider));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Vivaldi), LocalizationProvider));

        _timer = new Timer(BrowserAddonRefreshIntervalMilliseconds);
        _timer.Elapsed += OnBrowserAddonRefreshTimerElapsed;
        _timer.Start();
    }

    /// <summary>Stops and disposes the refresh timer so the dialog can close without background updates.</summary>
    public void Dispose()
    {
        _timer.Stop();
        _timer.Elapsed -= OnBrowserAddonRefreshTimerElapsed;
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnBrowserAddonRefreshTimerElapsed(object? sender, ElapsedEventArgs elapsedEventArgs)
    {
        foreach (ViewModelBrowserAddonStatus browserAddonStatus in Browsers)
            browserAddonStatus.RefreshStatus();
    }
}








