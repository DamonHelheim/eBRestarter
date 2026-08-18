using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Desktop.WinUI3.BehavioralComponents.Extensions;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Models;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.Validators;
using eBRestarter.Core.Domain.ValueObjects;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.SignalDTO.Messages;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;
using eBRestarter.Infrastructure.Common.Statics;
using Microsoft.Extensions.Logging;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the "Restarter properties" / task configuration page. Manages runtime hours,
/// pause seconds, browser-alive check, start-with-program, cache-delete interval, and username.
/// Persists via <see cref="IOutboundPortEVisitorConfigRepository"/> and broadcasts changes with
/// <see cref="WeakReferenceMessenger"/> so the restart task and other pages stay in sync.
/// </summary>
public sealed partial class ViewModelRestarterProperties : ObservableObject, IDisposable
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
    private const string ActivateResourceKey = "Activate";
    private const int BrowserInstallCheckIntervalSeconds = 5;
    private const string BrowserNextDeleteDateFormatResourceKey = "Browser_NextDeleteDate_Format";
    private const string DisabledResourceKey = "Disabled";
    private const string NextDeletionProcessResourceKey = "NextDeletionProcess";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies ──
    private readonly IOutboundPortBrowserDiscoveryProvider _browserService;
    private readonly ILogger<ViewModelRestarterProperties> _logger;
    private readonly IDialogService _dialogService;
    private readonly IOutboundPortEVisitorConfigRepository _evRestarterConfigRepository;
    private readonly IInboundPortLocalizationProvider _localizationService;
    private readonly IOutboundPortOsProcessControl _osProcessControlPort;
    private readonly IUseCaseScheduleBrowserCleanup _scheduleBrowserCleanupUseCase;
    private readonly IUIOptionsProvider _uiOptionsService;

    // ── Block 2: Primitives & strings ──
    private volatile bool _disposed;

    // ⚡ Overlap prevention: Ensures background timer ticks do not stack. 0 = free, 1 = active.
    private int _browserCheckInFlight;

    // ── Block 4: Complex types, collections & UI elements ──
    private readonly DispatcherTimer _browserCheckTimer;
    private readonly AppConfig _currentConfig;
    private readonly DispatcherQueue _dispatcherQueue;

    // ═══════════════════════════════════════════════════════
    //  3. Observable Properties (+ Partial Methods)
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
    [ObservableProperty] public partial bool CheckBrowserIsAliveIsOn { get; set; }

    partial void OnCheckBrowserIsAliveIsOnChanged(bool value)
    {
        _currentConfig.Browser.CheckBrowserAliveRoutine = value;

        var freshConfig = _evRestarterConfigRepository.LoadConfig();
        freshConfig.Browser.CheckBrowserAliveRoutine = value;
        _evRestarterConfigRepository.SaveConfig(freshConfig);
    }

    [ObservableProperty] public partial bool DeleteBrowserContentCommandIsEnabled { get; set; } = true;
    [ObservableProperty] public partial bool InstallEVisitorAddOnCommandIsEnabled { get; set; } = true;
    [ObservableProperty] public partial int RuntimeHours { get; set; }

    partial void OnRuntimeHoursChanged(int value)
    {
        int clampedValue = Math.Clamp(value, BrowserRuntimeHoursMin, BrowserRuntimeHoursMax);

        if (value != clampedValue)
        {
            RuntimeHours = clampedValue;
            return;
        }

        if (_currentConfig.Browser.RuntimeHours != value)
        {
            _currentConfig.Browser.RuntimeHours = value;
            var freshConfig = _evRestarterConfigRepository.LoadConfig();
            freshConfig.Browser.RuntimeHours = value;
            _evRestarterConfigRepository.SaveConfig(freshConfig);
        }
    }

    [ObservableProperty] public partial int RuntimePauseSeconds { get; set; }

    partial void OnRuntimePauseSecondsChanged(int value)
    {
        int clampedValue = Math.Clamp(value, RuntimePauseSecondsMin, RuntimePauseSecondsMax);

        if (value != clampedValue)
        {
            RuntimePauseSeconds = clampedValue;
            return;
        }

        if (_currentConfig.Browser.RuntimePauseSeconds != value)
        {
            _currentConfig.Browser.RuntimePauseSeconds = value;
            var freshConfig = _evRestarterConfigRepository.LoadConfig();
            freshConfig.Browser.RuntimePauseSeconds = value;
            _evRestarterConfigRepository.SaveConfig(freshConfig);
        }
    }

    [ObservableProperty] public partial bool StartBrowserWithProgramStart { get; set; }

    partial void OnStartBrowserWithProgramStartChanged(bool value)
    {
        _currentConfig.Browser.StartBrowserWithProgrammStart = value;
        var freshConfig = _evRestarterConfigRepository.LoadConfig();
        freshConfig.Browser.StartBrowserWithProgrammStart = value;
        _evRestarterConfigRepository.SaveConfig(freshConfig);
    }

    [ObservableProperty] private partial string StandardBrowser { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddEVisitorUsernameCommand))]
    public partial string Username { get; set; } = string.Empty;

    // ── Block 4: Complex types, collections & UI elements ──
    [ObservableProperty] public partial Visibility NoBrowserInstalledSectionVisibility { get; set; } = Visibility.Collapsed;
    [ObservableProperty] public partial BrowserCacheDeleteOption SelectedDeleteBrowserCacheOption { get; set; }

    partial void OnSelectedDeleteBrowserCacheOptionChanged(BrowserCacheDeleteOption value)
    {
        if (value == null)
        {
            return;
        }

        var scheduleUpdateResponse =
            _scheduleBrowserCleanupUseCase.UpdateSchedule(new ScheduleBrowserCleanupRequest(value.Days));

        _currentConfig.Browser.UpdateCleanupSettings(value.Days, TimeProvider.System);
        _currentConfig.Browser.SetNextCleanupDate(scheduleUpdateResponse.NextDate ?? DateTime.MinValue);

        var freshConfig = _evRestarterConfigRepository.LoadConfig();
        freshConfig.Browser.UpdateCleanupSettings(value.Days, TimeProvider.System);
        freshConfig.Browser.SetNextCleanupDate(scheduleUpdateResponse.NextDate ?? DateTime.MinValue);
        _evRestarterConfigRepository.SaveConfig(freshConfig);

        if (scheduleUpdateResponse.IsActive && scheduleUpdateResponse.NextDate.HasValue)
        {
            string formatPattern = _localizationService.RetrieveString(BrowserNextDeleteDateFormatResourceKey);
            string formattedDateString = string.Format(formatPattern, scheduleUpdateResponse.NextDate.Value);

            WeakReferenceMessenger.Default.Send(new NextDeletionProcessMessage(_localizationService.RetrieveString(NextDeletionProcessResourceKey)));
            WeakReferenceMessenger.Default.Send(new NextDeletionProcessDate(formattedDateString));
            WeakReferenceMessenger.Default.Send(new DeleteBrowserContentActivateMessage(_localizationService.RetrieveString(ActivateResourceKey)));
            WeakReferenceMessenger.Default.Send(new DeleteBrowserContentIsActive(true));
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new NextDeletionProcessMessage(string.Empty));
            WeakReferenceMessenger.Default.Send(new NextDeletionProcessDate(string.Empty));
            WeakReferenceMessenger.Default.Send(new DeleteBrowserContentActivateMessage(_localizationService.RetrieveString(DisabledResourceKey)));
            WeakReferenceMessenger.Default.Send(new DeleteBrowserContentIsActive(false));
        }
    }


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
    public int BrowserRuntimeHoursMax { get; init; }
    public int BrowserRuntimeHoursMin { get; init; }
    public int RuntimePauseSecondsMax { get; init; }
    public int RuntimePauseSecondsMin { get; init; }

    // ── Block 4: Complex types, collections & UI elements ──
    /// <summary>Read-only list of cache-delete interval options (e.g. daily, weekly) from localization.</summary>
    public ReadOnlyCollection<BrowserCacheDeleteOption> BrowserDeleteCacheOptionList { get; }

    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Loads config and populates bounds and options from localization. Binds observable properties
    /// to saved values (runtime, pause, cache interval, start-with-program, browser-alive check).
    /// Does not send messages yet; property change handlers do that when the user edits.
    /// </summary>
    public ViewModelRestarterProperties(
        IOutboundPortBrowserDiscoveryProvider browserService,
        IDialogService dialogService,
        IOutboundPortEVisitorConfigRepository evRestarterConfigRepository,
        IInboundPortLocalizationProvider localizationService,
        ILogger<ViewModelRestarterProperties> logger,
        IOutboundPortOsProcessControl osProcessControlPort,
        IUseCaseScheduleBrowserCleanup scheduleBrowserCleanupUseCase,
        IUIOptionsProvider uiOptionsService)
    {
        ArgumentNullException.ThrowIfNull(browserService);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(evRestarterConfigRepository);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(osProcessControlPort);
        ArgumentNullException.ThrowIfNull(scheduleBrowserCleanupUseCase);
        ArgumentNullException.ThrowIfNull(uiOptionsService);

        _browserService = browserService;
        _dialogService = dialogService;
        _evRestarterConfigRepository = evRestarterConfigRepository;
        _localizationService = localizationService;
        _logger = logger;
        _osProcessControlPort = osProcessControlPort;
        _scheduleBrowserCleanupUseCase = scheduleBrowserCleanupUseCase;
        _uiOptionsService = uiOptionsService;

        _dispatcherQueue =
            DispatcherQueue.GetForCurrentThread()
            ?? throw new InvalidOperationException(
                $"{nameof(ViewModelRestarterProperties)} must be constructed on a thread with a WinUI DispatcherQueue (UI thread).");

        _currentConfig = _evRestarterConfigRepository.LoadConfig();

        RuntimePauseSecondsMin = 20;
        RuntimePauseSecondsMax = 60;
        BrowserRuntimeHoursMin = 1;
        BrowserRuntimeHoursMax = 12;

        RuntimePauseSeconds = _currentConfig.Browser.RuntimePauseSeconds;
        RuntimeHours = _currentConfig.Browser.RuntimeHours;
        StartBrowserWithProgramStart = _currentConfig.Browser.StartBrowserWithProgrammStart;
        CheckBrowserIsAliveIsOn = _currentConfig.Browser.CheckBrowserAliveRoutine;

        BrowserDeleteCacheOptionList = new ReadOnlyCollection<BrowserCacheDeleteOption>([.. _uiOptionsService.GetBrowserCacheOptions()]);

        if (BrowserDeleteCacheOptionList.Count == 0)
        {
            throw new InvalidOperationException(
                $"{nameof(ViewModelRestarterProperties)} requires at least one entry from {nameof(IUIOptionsProvider.GetBrowserCacheOptions)}.");
        }

        int configDays = _currentConfig.Browser.DeleteBrowserCacheIntervalDays;

        SelectedDeleteBrowserCacheOption =
            BrowserDeleteCacheOptionList.FirstOrDefault(option => option.Days == configDays)
            ?? BrowserDeleteCacheOptionList[0];

        _browserCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(BrowserInstallCheckIntervalSeconds)
        };

        _browserCheckTimer.Tick += OnBrowserCheckTimerTick;
        _browserCheckTimer.Start();

        CheckInstalledBrowsersAsync().Forget(_logger, nameof(CheckInstalledBrowsersAsync));
    }

    // ═══════════════════════════════════════════════════════
    //  7. Commands
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Saves the current <see cref="Username"/> to config, sends <see cref="UsernameChangedMessage"/> so
    /// the restart task and other UIs update, then clears the username field for the next entry.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddUsername))]
    private void AddEVisitorUsername()
    {
        var freshConfig = _evRestarterConfigRepository.LoadConfig();
        freshConfig.Username = Username;
        _evRestarterConfigRepository.SaveConfig(freshConfig);

        _currentConfig.Username = Username;

        WeakReferenceMessenger.Default.Send(new UsernameChangedMessage(Username));

        Username = string.Empty;
    }

    /// <summary>Opens the Edge Startup Boost dialog so the user can disable Startup Boost to reduce background usage.</summary>
    [RelayCommand]
    private Task OpenStartupBoostDialogAsync() => _dialogService.ShowTurnOffEdgeStartupBoostDialogAsync();

    /// <summary>Opens the eBesucher registration URL in the default browser.</summary>
    [RelayCommand]
    public void RegisterToEVisitor()
    {
        _osProcessControlPort.OpenUrlInBrowser(WebLinks.RegistrationLink, string.Empty);
    }

    /// <summary>Opens the delete-browser-content dialog in manual mode (no auto-start after completion).</summary>
    [RelayCommand]
    private Task ShowBrowserDeleteContentAsync() => _dialogService.ShowDeleteBrowserContentDialogAsync(shouldAutoStart: false);

    /// <summary>Opens the install add-on dialog so the user can install the extension in supported browsers.</summary>
    [RelayCommand]
    private Task ShowInstallAddOnDialogAsync() => _dialogService.ShowInstallAddOnDialogAsync();

    /// <summary>Opens the informational dialog about the add-on (what it does, why it is needed).</summary>
    [RelayCommand]
    private Task ShowInstallAddOnInfoDialogAsync() => _dialogService.ShowInstallAddOnInfoDialogAsync();

    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Stops the browser-check timer and unsubscribes its handler so the callback cannot keep this
    /// view model alive after teardown (Guide Kap. 22.6 / 22.10).
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _browserCheckTimer.Stop();
        _browserCheckTimer.Tick -= OnBrowserCheckTimerTick;
    }

    /// <summary>
    /// Enables the "add username" command only for names that satisfy the whitelist.
    /// </summary>
    /// <remarks>
    /// 🔒 Security guideline: Restricts the username to the character set defined in <see cref="EVisitorUsernamePolicy"/>
    /// to prevent injection into browser command lines. This check disables the UI button; enforcement is also handled by validators.
    /// </remarks>
    private bool CanAddUsername() => EVisitorUsernamePolicy.IsValid(Username);

    /// <summary>
    /// Checks in the background whether at least one supported browser is installed, then updates UI bindings on the dispatcher.
    /// </summary>
    private async Task CheckInstalledBrowsersAsync()
    {
        // Offload work so registry/file checks do not block the UI thread between timer ticks.
        IEnumerable<BrowserInfo>? installedBrowsers =
            await Task.Run(() => _browserService.FindInstalledBrowsersAsync()).ConfigureAwait(false);

        bool hasInstalledBrowsers =
            installedBrowsers?.Any(browser => browser.IsInstalled) == true;

        _dispatcherQueue.TryEnqueue(() =>
        {
            Visibility newVisibility = hasInstalledBrowsers ? Visibility.Collapsed : Visibility.Visible;

            if (NoBrowserInstalledSectionVisibility != newVisibility)
            {
                NoBrowserInstalledSectionVisibility = newVisibility;
                DeleteBrowserContentCommandIsEnabled = hasInstalledBrowsers;
                InstallEVisitorAddOnCommandIsEnabled = hasInstalledBrowsers;
            }
        });
    }

    /// <summary>
    /// Triggers periodic installed-browser checks on timer tick.
    /// </summary>
    /// <param name="sender">The timer instance.</param>
    /// <param name="eventArgs">Event arguments associated with the tick event.</param>
    private async void OnBrowserCheckTimerTick(object? sender, object eventArgs)
    {
        // Guard against timer callbacks firing after disposal.
        if (_disposed)
        {
            return;
        }

        // Drop overlapping ticks to prevent concurrency buildup.
        if (Interlocked.Exchange(ref _browserCheckInFlight, 1) == 1)
        {
            return;
        }

        try
        {
            await CheckInstalledBrowsersAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                LogEventIds.Browser.BrowserProfileDiscoveryFailed,
                exception,
                "Checking for installed browsers failed.");
        }
        finally
        {
            Volatile.Write(ref _browserCheckInFlight, 0);
        }
    }
}
