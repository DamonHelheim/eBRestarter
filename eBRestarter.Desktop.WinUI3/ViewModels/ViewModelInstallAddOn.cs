using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Enums;
using System;
using System.Collections.ObjectModel;
using System.Timers;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the "Install Add-on" dialog. Holds a fixed list of browser add-on status
/// view models (Brave, Chrome, Edge, Firefox, Vivaldi) and refreshes their install/extension state
/// on a timer so the user sees up-to-date status without manually refreshing.
/// </summary>
public partial class ViewModelInstallAddOn : ObservableObject, IDisposable
{
    private const double BrowserAddonRefreshIntervalMilliseconds = 2000;

    private readonly IBrowserFactory _browserFactory;

    private readonly Timer _timer;

    /// <summary>One entry per supported browser, each showing install and extension status.</summary>
    public ObservableCollection<ViewModelBrowserAddonStatus> Browsers { get; } = [];


    /// <summary>
    /// Creates browser instances for all supported types, wraps each in a
    /// <see cref="ViewModelBrowserAddonStatus"/>, and starts a 2-second timer that refreshes
    /// each entry so install/extension state stays current (e.g. after user installs the add-on).
    /// </summary>
    /// <param name="browserFactory">Used to create browser instances for status checks. Must not be null.</param>
    /// <param name="localizationService">Passed to each ViewModelBrowserAddonStatus for localized strings. Must not be null.</param>
    public ViewModelInstallAddOn(IBrowserFactory browserFactory, ILocalizationService localizationService)
    {
        ArgumentNullException.ThrowIfNull(browserFactory);
        ArgumentNullException.ThrowIfNull(localizationService);

        _browserFactory = browserFactory;

        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Brave), localizationService));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Chrome), localizationService));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Edge), localizationService));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Firefox), localizationService));
        Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Vivaldi), localizationService));

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
