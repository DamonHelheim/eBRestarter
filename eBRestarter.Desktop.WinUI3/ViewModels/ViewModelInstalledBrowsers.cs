using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Extensions;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Providers;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the "Installed Browsers" page. Keeps a list of <see cref="ViewModelBrowserItem"/>
/// in sync with <see cref="IBrowserDiscoveryPort.FindInstalledBrowsersAsync"/>: updates existing items
/// when install state or version changes and adds new items when a new browser type appears.
/// Refreshes on a timer so the list stays current (e.g. after download/install).
/// </summary>
public sealed partial class ViewModelInstalledBrowsers : ObservableObject, IDisposable
{
    private const int BrowserListRefreshIntervalSeconds = 2;

    private readonly IBrowserDiscoveryPort _browserService;

    private readonly IBrowserDownloadPort _downloadService;

    private readonly IDialogService _dialogService;

    private readonly IEVisitorConfigPort _EVRestarterConfigRepository;

    private readonly ILocalizationProvider _localizationService;

    private readonly IOsProcessControlPort _osProcessControlPort;
    private readonly IOsAutoLogonPort _osAutoLogonPort;
    private readonly ISystemInfoPort _windowsSystemInfo;
    private readonly ISettingsPort _settingsPort;
    private readonly IAutoStartPort _autoStartPort;
    private readonly IFileSystemPort _fileSystemPort;
    private readonly IBrowserConfigPort _browserConfigPort;

    private readonly DispatcherTimer _refreshTimer;

    [ObservableProperty]
    public partial ObservableCollection<ViewModelBrowserItem> Browsers { get; set; } = [];


    /// <summary>
    /// Wires up services and a dispatcher timer that repeatedly calls
    /// <see cref="LoadBrowsersSmartAsync"/> so the browser list stays in sync with
    /// installed browsers and versions. Kicks off the first load immediately.
    /// </summary>
    public ViewModelInstalledBrowsers(
        IBrowserDiscoveryPort browserService,
        IBrowserDownloadPort downloadService,
        IOsProcessControlPort osProcessControlPort, IOsAutoLogonPort osAutoLogonPort, ISystemInfoPort windowsSystemInfo, ISettingsPort settingsPort, IAutoStartPort autoStartPort, IFileSystemPort fileSystemPort, IBrowserConfigPort browserConfigPort,
        IDialogService dialogService,
        ILocalizationProvider LocalizationProvider,
        IEVisitorConfigPort EVRestarterConfigRepository)
    {
        ArgumentNullException.ThrowIfNull(browserService);
        ArgumentNullException.ThrowIfNull(downloadService);
        ArgumentNullException.ThrowIfNull(osProcessControlPort);
        ArgumentNullException.ThrowIfNull(osAutoLogonPort);
        ArgumentNullException.ThrowIfNull(windowsSystemInfo);
        ArgumentNullException.ThrowIfNull(settingsPort);
        ArgumentNullException.ThrowIfNull(autoStartPort);
        ArgumentNullException.ThrowIfNull(fileSystemPort);
        ArgumentNullException.ThrowIfNull(browserConfigPort);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(LocalizationProvider);
        ArgumentNullException.ThrowIfNull(EVRestarterConfigRepository);

        _browserService = browserService;
        _downloadService = downloadService;
        _osProcessControlPort = osProcessControlPort;
        _osAutoLogonPort = osAutoLogonPort;
        _windowsSystemInfo = windowsSystemInfo;
        _settingsPort = settingsPort;
        _autoStartPort = autoStartPort;
        _fileSystemPort = fileSystemPort;
        _browserConfigPort = browserConfigPort;
        _dialogService = dialogService;
        _localizationService = LocalizationProvider;
        _EVRestarterConfigRepository = EVRestarterConfigRepository;

        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(BrowserListRefreshIntervalSeconds)
        };
        _refreshTimer.Tick += OnRefreshTimerTick;

        LoadBrowsersSmartAsync().Forget();

        _refreshTimer.Start();
    }

    /// <summary>Stops the refresh timer and unsubscribes from tick events. Call when leaving the page or disposing the VM.</summary>
    public void Dispose()
    {
        _refreshTimer.Stop();
        _refreshTimer.Tick -= OnRefreshTimerTick;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Fetches the current list of installed browsers from the service. For each result,
    /// either updates the matching existing <see cref="ViewModelBrowserItem"/> (by browser type)
    /// or creates a new one and adds it, so the UI list reflects current install state and versions.
    /// </summary>
    public async Task LoadBrowsersSmartAsync()
    {
        var freshBrowserInfos = await _browserService.FindInstalledBrowsersAsync();

        foreach (var installedBrowserInfo in freshBrowserInfos)
        {
            var existingBrowserItem = Browsers.FirstOrDefault(browserItem => browserItem.BrowserType == installedBrowserInfo.Type);

            if (existingBrowserItem != null)
            {
                existingBrowserItem.Update(installedBrowserInfo);
            }
            else
            {
                var newBrowserItem = new ViewModelBrowserItem(
                    installedBrowserInfo,
                    _downloadService,
                    _osProcessControlPort, _osAutoLogonPort, _windowsSystemInfo, _settingsPort, _autoStartPort, _fileSystemPort, _browserConfigPort,
                    _EVRestarterConfigRepository,
                    _dialogService,
                    _localizationService);
                Browsers.Add(newBrowserItem);
            }
        }
    }

    private void OnRefreshTimerTick(object? sender, object eventArgs)
    {
        LoadBrowsersSmartAsync().Forget();
    }
}















