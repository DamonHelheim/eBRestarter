using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using eBRestarter.Core.Application.BehavioralComponents.Extensions;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the "Installed Browsers" page. Keeps a list of <see cref="ViewModelBrowserItem"/>
/// in sync with <see cref="IOutboundPortBrowserDiscoveryProvider.FindInstalledBrowsersAsync"/>: updates existing items
/// when install state or version changes and adds new items when a new browser type appears.
/// Refreshes on a timer so the list stays current (e.g. after download/install).
/// </summary>
public sealed partial class ViewModelInstalledBrowsers : ObservableObject, IDisposable
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const int BrowserListRefreshIntervalSeconds = 2;

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    private readonly IOutboundPortBrowserDiscoveryProvider _browserService;
    private readonly IDialogService _dialogService;
    private readonly IUseCaseDownloadBrowser _downloadBrowserUseCase;
    private readonly IOutboundPortEVisitorConfigRepository _evRestarterConfigRepository;
    private readonly IInboundPortLocalizationProvider _localizationService;

    private readonly DispatcherTimer _refreshTimer;


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════
    /// <summary>One entry per supported browser type.</summary>
    public ObservableCollection<ViewModelBrowserItem> Browsers { get; } = [];

    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Wires up services and a dispatcher timer that repeatedly calls
    /// <see cref="LoadBrowsersSmartAsync"/> so the browser list stays in sync with
    /// installed browsers and versions. Kicks off the first load immediately.
    /// </summary>
    public ViewModelInstalledBrowsers(
        IOutboundPortBrowserDiscoveryProvider browserService,
        IDialogService dialogService,
        IUseCaseDownloadBrowser downloadBrowserUseCase,
        IInboundPortLocalizationProvider localizationService,
        IOutboundPortEVisitorConfigRepository evRestarterConfigRepository)
    {
        ArgumentNullException.ThrowIfNull(browserService);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(downloadBrowserUseCase);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(evRestarterConfigRepository);

        _browserService = browserService;
        _dialogService = dialogService;
        _downloadBrowserUseCase = downloadBrowserUseCase;
        _localizationService = localizationService;
        _evRestarterConfigRepository = evRestarterConfigRepository;

        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(BrowserListRefreshIntervalSeconds)
        };
        _refreshTimer.Tick += OnRefreshTimerTick;

        LoadBrowsersSmartAsync().Forget();

        _refreshTimer.Start();
    }

    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
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

            if (existingBrowserItem is not null)
            {
                existingBrowserItem.Update(installedBrowserInfo);
                continue;
            }

            var newBrowserItem = new ViewModelBrowserItem(
                installedBrowserInfo,
                _downloadBrowserUseCase,
                _evRestarterConfigRepository,
                _dialogService,
                _localizationService);

            Browsers.Add(newBrowserItem);
        }
    }

    private void OnRefreshTimerTick(object? sender, object eventArgs)
    {
        LoadBrowsersSmartAsync().Forget();
    }
}
