using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Constants;
using eBRestarter.Core.Application.Extensions;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Models;
using eBRestarter.Core.Domain.Entities;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.UseCases.ScheduleBrowserCleanup;
using eBRestarter.Desktop.WinUI3.Messages;
using eBRestarter.Desktop.WinUI3.Models;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the "Restarter properties" / task configuration page. Manages runtime hours,
/// pause seconds, browser-alive check, start-with-program, cache-delete interval, and username.
/// Persists via <see cref="IEVisitorConfigService"/> and broadcasts changes with
/// <see cref="WeakReferenceMessenger"/> so the restart task and other pages stay in sync.
/// </summary>
public partial class ViewModelRestarterProperties : ObservableObject
{
    private const int BrowserInstallCheckIntervalSeconds = 5;

    private readonly IBrowserService _browserService;

    private readonly IDialogService _dialogService;

    private readonly IEVisitorConfigService _eVisitorConfigService;

    private readonly ILocalizationService _localizationService;

    private readonly IOperatingSystemFacade _operatingSystemFacade;

    private readonly IScheduleBrowserCleanupUseCase _scheduleBrowserCleanupUseCase;

    private readonly IUIOptionsService _uiOptionsService;

    private readonly DispatcherTimer _browserCheckTimer;

    private readonly AppConfig _currentConfig;

    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    public partial bool CheckBrowserIsAliveIsOn { get; set; }

    [ObservableProperty]
    public partial bool DeleteBrowserContentCommandIsEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool InstallEVisitorAddOnCommandIsEnabled { get; set; } = true;

    [ObservableProperty]
    public partial int RuntimeHours { get; set; }

    [ObservableProperty]
    public partial int RuntimePauseSeconds { get; set; }

    [ObservableProperty]
    public partial bool StartBrowserWithProgramStart { get; set; }

    [ObservableProperty]
    private partial string StandardBrowser { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddEVisitorUsernameCommand))]
    public partial string Username { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Visibility NoBrowserInstalledSectionVisibility { get; set; } = Visibility.Collapsed;

    [ObservableProperty]
    public partial BrowserCacheDeleteOption SelectedDeleteBrowserCacheOption { get; set; }

    public int BrowserRuntimeHoursMax { get; init; }

    public int BrowserRuntimeHoursMin { get; init; }

    public int RuntimePauseSecondsMax { get; init; }

    public int RuntimePauseSecondsMin { get; init; }

    /// <summary>Read-only list of cache-delete interval options (e.g. daily, weekly) from localization.</summary>
    public ReadOnlyCollection<BrowserCacheDeleteOption> BrowserDeleteCacheOptionList { get; }

    /// <summary>
    /// Loads config and populates bounds and options from localization. Binds observable properties
    /// to saved values (runtime, pause, cache interval, start-with-program, browser-alive check).
    /// Does not send messages yet; property change handlers do that when the user edits.
    /// </summary>
    public ViewModelRestarterProperties(
        IScheduleBrowserCleanupUseCase scheduleBrowserCleanupUseCase,
        IOperatingSystemFacade operatingSystemFacade,
        IEVisitorConfigService eVisitorConfigService,
        ILocalizationService localizationService,
        IUIOptionsService uiOptionsService,
        IDialogService dialogService,
        IBrowserService browserService)
    {
        ArgumentNullException.ThrowIfNull(scheduleBrowserCleanupUseCase);
        ArgumentNullException.ThrowIfNull(operatingSystemFacade);
        ArgumentNullException.ThrowIfNull(eVisitorConfigService);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(uiOptionsService);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(browserService);

        _browserService = browserService;
        _dialogService = dialogService;
        _eVisitorConfigService = eVisitorConfigService;
        _localizationService = localizationService;
        _operatingSystemFacade = operatingSystemFacade;
        _scheduleBrowserCleanupUseCase = scheduleBrowserCleanupUseCase;
        _uiOptionsService = uiOptionsService;

        _dispatcherQueue =
            DispatcherQueue.GetForCurrentThread()
            ?? throw new InvalidOperationException(
                $"{nameof(ViewModelRestarterProperties)} must be constructed on a thread with a WinUI DispatcherQueue (UI thread).");

        _currentConfig = _eVisitorConfigService.LoadConfig();

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
                $"{nameof(ViewModelRestarterProperties)} requires at least one entry from {nameof(IUIOptionsService.GetBrowserCacheOptions)}.");
        }

        int configDays = _currentConfig.Browser.DeleteBrowserCacheIntervalDays;

        SelectedDeleteBrowserCacheOption =
            BrowserDeleteCacheOptionList.FirstOrDefault(option => option.Days == configDays)
            ?? BrowserDeleteCacheOptionList[0];

        _browserCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(BrowserInstallCheckIntervalSeconds)
        };

        _browserCheckTimer.Tick += async (_, _) =>
        {
            try
            {
                await CheckInstalledBrowsersAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        };

        _browserCheckTimer.Start();

        CheckInstalledBrowsersAsync().Forget();
    }

    /// <summary>
    /// Saves the current <see cref="Username"/> to config, sends <see cref="UsernameChangedMessage"/> so
    /// the restart task and other UIs update, then clears the username field for the next entry.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddUsername))]
    private void AddEVisitorUsername()
    {
        var freshConfig = _eVisitorConfigService.LoadConfig();
        freshConfig.Username = Username;
        _eVisitorConfigService.SaveConfig(freshConfig);

        _currentConfig.Username = Username;

        WeakReferenceMessenger.Default.Send(new UsernameChangedMessage(Username));

        Username = string.Empty;
    }

    /// <summary>Opens the Edge Startup Boost dialog so the user can disable Startup Boost to reduce background usage.</summary>
    [RelayCommand]
    private async Task OpenStartupBoostDialog()
    {
        await _dialogService.ShowTurnOffEdgeStartupBoostDialogAsync();
    }

    /// <summary>Opens the eBesucher registration URL in the default browser.</summary>
    [RelayCommand]
    public void RegisterToEVisitor()
    {
        _operatingSystemFacade.WindowsProcessControlService.OpenUrlInBrowser(WebLinks.RegistrationLink, string.Empty);
    }

    /// <summary>Opens the delete-browser-content dialog in manual mode (no auto-start after completion).</summary>
    [RelayCommand]
    private async Task ShowBrowserDeleteContent()
    {
        await _dialogService.ShowDeleteBrowserContentDialogAsync(autoStart: false);
    }

    /// <summary>Opens the install add-on dialog so the user can install the extension in supported browsers.</summary>
    [RelayCommand]
    private async Task ShowInstallAddOnDialog()
    {
        await _dialogService.ShowInstallAddOnDialogAsync();
    }

    /// <summary>Opens the informational dialog about the add-on (what it does, why it is needed).</summary>
    [RelayCommand]
    private async Task ShowInstallAddOnInfoDialog()
    {
        await _dialogService.ShowInstallAddOnInfoDialogAsync();
    }

    /// <summary>
    /// Checks in the background whether at least one supported browser is installed, then updates UI bindings on the dispatcher.
    /// </summary>
    private async Task CheckInstalledBrowsersAsync()
    {
        // Offload work so registry/file checks do not block the UI thread between timer ticks.
        IEnumerable<BrowserInfo>? installedBrowsers =
            await Task.Run(() => _browserService.GetInstalledBrowsersAsync()).ConfigureAwait(false);

        bool hasInstalledBrowsers =
            installedBrowsers != null && installedBrowsers.Any(browser => browser.IsInstalled);

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

    // ObservableProperty partials: parameter name 'value' follows CommunityToolkit source generator convention.
    partial void OnCheckBrowserIsAliveIsOnChanged(bool value)
    {
        _currentConfig.Browser.CheckBrowserAliveRoutine = value;

        var freshConfig = _eVisitorConfigService.LoadConfig();
        freshConfig.Browser.CheckBrowserAliveRoutine = value;
        _eVisitorConfigService.SaveConfig(freshConfig);
    }

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
            var freshConfig = _eVisitorConfigService.LoadConfig();
            freshConfig.Browser.RuntimeHours = value;
            _eVisitorConfigService.SaveConfig(freshConfig);
        }
    }

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
            var freshConfig = _eVisitorConfigService.LoadConfig();
            freshConfig.Browser.RuntimePauseSeconds = value;
            _eVisitorConfigService.SaveConfig(freshConfig);
        }
    }

    partial void OnSelectedDeleteBrowserCacheOptionChanged(BrowserCacheDeleteOption value)
    {
        if (value == null)
            return;

        var scheduleUpdateResponse =
            _scheduleBrowserCleanupUseCase.UpdateSchedule(new ScheduleBrowserCleanupRequest(value.Days));

        _currentConfig.Browser.UpdateCleanupSettings(value.Days, TimeProvider.System);
        _currentConfig.Browser.SetNextCleanupDate(scheduleUpdateResponse.NextDate ?? DateTime.MinValue);

        var freshConfig = _eVisitorConfigService.LoadConfig();
        freshConfig.Browser.UpdateCleanupSettings(value.Days, TimeProvider.System);
        freshConfig.Browser.SetNextCleanupDate(scheduleUpdateResponse.NextDate ?? DateTime.MinValue);
        _eVisitorConfigService.SaveConfig(freshConfig);

        if (scheduleUpdateResponse.IsActive && scheduleUpdateResponse.NextDate.HasValue)
        {
            string formatPattern = _localizationService.GetString("Browser_NextDeleteDate_Format");
            string formattedDateString = string.Format(formatPattern, scheduleUpdateResponse.NextDate.Value);

            WeakReferenceMessenger.Default.Send(new NextDeletionProcess(_localizationService.GetString("NextDeletionProcess")));
            WeakReferenceMessenger.Default.Send(new NextDeletionProcessDate(formattedDateString));
            WeakReferenceMessenger.Default.Send(new DeleteBrowserContentActivateMessage(_localizationService.GetString("Activate")));
            WeakReferenceMessenger.Default.Send(new DeleteBrowserContentIsActive(true));
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new NextDeletionProcess(string.Empty));
            WeakReferenceMessenger.Default.Send(new NextDeletionProcessDate(string.Empty));
            WeakReferenceMessenger.Default.Send(new DeleteBrowserContentActivateMessage(_localizationService.GetString("Disabled")));
            WeakReferenceMessenger.Default.Send(new DeleteBrowserContentIsActive(false));
        }
    }

    partial void OnStartBrowserWithProgramStartChanged(bool value)
    {
        _currentConfig.Browser.StartBrowserWithProgrammStart = value;
        var freshConfig = _eVisitorConfigService.LoadConfig();
        freshConfig.Browser.StartBrowserWithProgrammStart = value;
        _eVisitorConfigService.SaveConfig(freshConfig);
    }

    // Private helpers (alphabetically after ObservableProperty partials).
    /// <summary>Username can be added only when the field is non-empty.</summary>
    private bool CanAddUsername() => !string.IsNullOrWhiteSpace(Username);
}
