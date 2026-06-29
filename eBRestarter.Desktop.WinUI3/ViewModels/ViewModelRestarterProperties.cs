using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Extensions;
using eBRestarter.Core.Application.Models;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ScheduleBrowserCleanup;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Providers;
using eBRestarter.Core.Domain.Entities;
using eBRestarter.Desktop.WinUI3.Messages;
using eBRestarter.Desktop.WinUI3.Models;
using eBRestarter.Desktop.WinUI3.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Infrastructure.Browser;
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
/// Persists via <see cref="IEVisitorConfigPort"/> and broadcasts changes with
/// <see cref="WeakReferenceMessenger"/> so the restart task and other pages stay in sync.
/// </summary>
public sealed partial class ViewModelRestarterProperties : ObservableObject
{
    private const int BrowserInstallCheckIntervalSeconds = 5;

    private readonly IBrowserDiscoveryPort _browserService;

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

    private readonly IScheduleBrowserCleanupUseCase _scheduleBrowserCleanupUseCase;

    private readonly IUIOptionsProvider _uiOptionsService;

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
        IOsProcessControlPort osProcessControlPort, IOsAutoLogonPort osAutoLogonPort, ISystemInfoPort windowsSystemInfo, ISettingsPort settingsPort, IAutoStartPort autoStartPort, IFileSystemPort fileSystemPort, IBrowserConfigPort browserConfigPort,
        IEVisitorConfigPort EVRestarterConfigRepository,
        ILocalizationProvider LocalizationProvider,
        IUIOptionsProvider uiOptionsService,
        IDialogService dialogService,
        IBrowserDiscoveryPort browserService)
    {
        ArgumentNullException.ThrowIfNull(scheduleBrowserCleanupUseCase);
        ArgumentNullException.ThrowIfNull(osProcessControlPort);
        ArgumentNullException.ThrowIfNull(osAutoLogonPort);
        ArgumentNullException.ThrowIfNull(windowsSystemInfo);
        ArgumentNullException.ThrowIfNull(settingsPort);
        ArgumentNullException.ThrowIfNull(autoStartPort);
        ArgumentNullException.ThrowIfNull(fileSystemPort);
        ArgumentNullException.ThrowIfNull(browserConfigPort);
        ArgumentNullException.ThrowIfNull(EVRestarterConfigRepository);
        ArgumentNullException.ThrowIfNull(LocalizationProvider);
        ArgumentNullException.ThrowIfNull(uiOptionsService);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(browserService);

        _browserService = browserService;
        _dialogService = dialogService;
        _EVRestarterConfigRepository = EVRestarterConfigRepository;
        _localizationService = LocalizationProvider;
        _osProcessControlPort = osProcessControlPort;
        _osAutoLogonPort = osAutoLogonPort;
        _windowsSystemInfo = windowsSystemInfo;
        _settingsPort = settingsPort;
        _autoStartPort = autoStartPort;
        _fileSystemPort = fileSystemPort;
        _browserConfigPort = browserConfigPort;
        _scheduleBrowserCleanupUseCase = scheduleBrowserCleanupUseCase;
        _uiOptionsService = uiOptionsService;

        _dispatcherQueue =
            DispatcherQueue.GetForCurrentThread()
            ?? throw new InvalidOperationException(
                $"{nameof(ViewModelRestarterProperties)} must be constructed on a thread with a WinUI DispatcherQueue (UI thread).");

        _currentConfig = _EVRestarterConfigRepository.LoadConfig();

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
        var freshConfig = _EVRestarterConfigRepository.LoadConfig();
        freshConfig.Username = Username;
        _EVRestarterConfigRepository.SaveConfig(freshConfig);

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
        _osProcessControlPort.OpenUrlInBrowser(WebLinks.RegistrationLink, string.Empty);
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


    partial void OnCheckBrowserIsAliveIsOnChanged(bool value)
    {
        _currentConfig.Browser.CheckBrowserAliveRoutine = value;

        var freshConfig = _EVRestarterConfigRepository.LoadConfig();
        freshConfig.Browser.CheckBrowserAliveRoutine = value;
        _EVRestarterConfigRepository.SaveConfig(freshConfig);
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
            var freshConfig = _EVRestarterConfigRepository.LoadConfig();
            freshConfig.Browser.RuntimeHours = value;
            _EVRestarterConfigRepository.SaveConfig(freshConfig);
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
            var freshConfig = _EVRestarterConfigRepository.LoadConfig();
            freshConfig.Browser.RuntimePauseSeconds = value;
            _EVRestarterConfigRepository.SaveConfig(freshConfig);
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

        var freshConfig = _EVRestarterConfigRepository.LoadConfig();
        freshConfig.Browser.UpdateCleanupSettings(value.Days, TimeProvider.System);
        freshConfig.Browser.SetNextCleanupDate(scheduleUpdateResponse.NextDate ?? DateTime.MinValue);
        _EVRestarterConfigRepository.SaveConfig(freshConfig);

        if (scheduleUpdateResponse.IsActive && scheduleUpdateResponse.NextDate.HasValue)
        {
            string formatPattern = _localizationService.RetrieveString("Browser_NextDeleteDate_Format");
            string formattedDateString = string.Format(formatPattern, scheduleUpdateResponse.NextDate.Value);

            WeakReferenceMessenger.Default.Send(new NextDeletionProcessMessage(_localizationService.RetrieveString("NextDeletionProcess")));
            WeakReferenceMessenger.Default.Send(new NextDeletionProcessDate(formattedDateString));
            WeakReferenceMessenger.Default.Send(new DeleteBrowserContentActivateMessage(_localizationService.RetrieveString("Activate")));
            WeakReferenceMessenger.Default.Send(new DeleteBrowserContentIsActive(true));
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new NextDeletionProcessMessage(string.Empty));
            WeakReferenceMessenger.Default.Send(new NextDeletionProcessDate(string.Empty));
            WeakReferenceMessenger.Default.Send(new DeleteBrowserContentActivateMessage(_localizationService.RetrieveString("Disabled")));
            WeakReferenceMessenger.Default.Send(new DeleteBrowserContentIsActive(false));
        }
    }

    partial void OnStartBrowserWithProgramStartChanged(bool value)
    {
        _currentConfig.Browser.StartBrowserWithProgrammStart = value;
        var freshConfig = _EVRestarterConfigRepository.LoadConfig();
        freshConfig.Browser.StartBrowserWithProgrammStart = value;
        _EVRestarterConfigRepository.SaveConfig(freshConfig);
    }

    /// <summary>Username can be added only when the field is non-empty.</summary>
    private bool CanAddUsername() => !string.IsNullOrWhiteSpace(Username);
}

















