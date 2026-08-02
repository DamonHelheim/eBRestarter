using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using eBRestarter.Core.Application.BehavioralComponents.Extensions;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Handlers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Services;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.ValueObjects;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.Enums;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

public sealed partial class ViewModelOptionsGeneral : ObservableObject
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    private const string AdminRequiredMessage = "Es sind Administratorrechte erforderlich, um diese Aktion auszuführen. Bitte starten Sie die Anwendung als Administrator.";
    private const string DarkThemeName = "Dark";
    private const string GeneralErrorKey = "General_Error";
    private const string GeneralInfoResourceKey = "General_Info";
    private const string GeneralNoResourceKey = "General_No";
    private const string GeneralSuccessResourceKey = "General_Success";
    private const string GeneralUnexpectedErrorResourceKey = "General_UnexpectedError";
    private const string GeneralYesResourceKey = "General_Yes";
    private const string LanguageCodeEnglish = "en-US";
    private const string LanguageCodeGerman = "de-DE";
    private const string LightThemeName = "Light";
    private const string OptionsAutoLogonActivatedResourceKey = "Options_AutoLogon_Success";
    private const string OptionsAutoLogonDeactivatedResourceKey = "Options_AutoLogon_Deactivated";
    private const string OptionsAutoLogonDomainErrorResourceKey = "Options_AutoLogon_DomainError";
    private const string OptionsAutoLogonValidationErrorResourceKey = "Options_AutoLogon_ValidationError";
    private const string OptionsAutoLogonWindowsHelloErrorMessageResourceKey = "Options_AutoLogon_WindowsHelloErrorMessage";
    private const string OptionsAutoLogonWindowsHelloErrorTitleResourceKey = "Options_AutoLogon_WindowsHelloErrorTitle";
    private const string OptionsLanguageChangedRestartMessageResourceKey = "Options_LanguageChanged_Restart_Message";
    private const string OptionsLanguageChangedRestartTitleResourceKey = "Options_LanguageChanged_Restart_Title";
    private const string OptionsRestartStatusNoneResourceKey = "Options_RestartStatus_None";
    private const string OptionsRestartStatusScheduledResourceKey = "Options_RestartStatus_Scheduled";
    private const string OptionsUpdateAvailablePromptMessageResourceKey = "Options_UpdatePrompt_Message";
    private const string OptionsUpdateAvailableResourceKey = "Options_UpdateAvailable";
    private const string OptionsUpdateAvailableTitleResourceKey = "Options_UpdateAvailable_Title";
    private const string OptionsUpdateCheckErrorMessageResourceKey = "Options_UpdateCheckError_Message";
    private const string OptionsUpdateErrorTitleResourceKey = "Options_UpdateError_Title";
    private const string OptionsUpdateFailedMessageResourceKey = "Options_UpdateFailed_Message";
    private const string OptionsUpdateFailedTitleResourceKey = "Options_UpdateFailed_Title";
    private const string OptionsUpdateNoUpdateMessageResourceKey = "Options_UpdateNoUpdate_Message";
    private const string OptionsUpdateNoUpdateTitleResourceKey = "Options_UpdateNoUpdate_Title";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (Dependencies) ──
    private readonly IInboundPortOsAppPathProvider _appPathProvider;
    private readonly IInboundPortComputerRestartService _computerRestartScheduler;
    private readonly IUseCaseConfigureAutoLogon _configureAutoLogonUseCase;
    private readonly IDialogService _dialogService;
    private readonly IOutboundPortEVisitorConfigRepository _evRestarterConfigRepository;
    private readonly ILanguageHandler _languageService;
    private readonly IInboundPortLocalizationProvider _localizationService;
    private readonly IUseCaseManageApplicationUpdates _manageApplicationUpdatesUseCase;
    private readonly IInboundPortNextRestartDateHandler _nextRestartDateHandler;
    private readonly IOutboundPortOsAutoLogonRepository _osAutoLogonPort;
    private readonly IOutboundPortOsProcessControl _osProcessControlPort;
    private readonly IThemeHandler _themeService;
    private readonly IUseCaseToggleAppAutoStart _toggleAppAutoStartUseCase;
    private readonly IUIOptionsProvider _uiOptionsService;
    private readonly IOutboundPortSystemInfoProvider _windowsSystemInfo;

    // ── Block 2: Primitive Typen & Strings ──
    private bool _isInitializing;

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente ──
    private readonly AppConfig _currentConfig;
    private readonly DispatcherQueue _dispatcherQueue;

    // ═══════════════════════════════════════════════════════
    //  3. Observable Properties (+ Partial Methods)
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    [ObservableProperty] public partial int ComputerRestartClockTime { get; set; }

    partial void OnComputerRestartClockTimeChanged(int value)
    {
        int clampedValue = Math.Clamp(value, ComputerRestartClockTimeMin, ComputerRestartClockTimeMax);

        if (value != clampedValue)
        {
            ComputerRestartClockTime = clampedValue;
            return;
        }

        if (_currentConfig.Computer.RestartClockTime != value)
        {
            _currentConfig.Computer.UpdateRestartSettings(
                _currentConfig.Computer.ComputerRestartIntervalDays,
                value,
                TimeProvider.System);
            UpdateRestartUiState();
            SaveSettings();
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckForUpdatesCommand))]
    public partial bool IsCheckingForUpdates { get; set; }

    [ObservableProperty] public partial bool IsRestartSliderVisible { get; set; }
    [ObservableProperty] public partial bool IsUpdateAvailable { get; set; }
    [ObservableProperty] public partial string RestartStatusText { get; set; } = string.Empty;
    [ObservableProperty] public partial bool StartWithWindows { get; set; }

    partial void OnStartWithWindowsChanged(bool value)
    {
        if (_isInitializing)
        {
            return;
        }

        ToggleAutoStartAsync(value).Forget();
    }

    [ObservableProperty] public partial string UpdateMessage { get; set; } = string.Empty;

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente ──
    [ObservableProperty] public partial ComputerRestartOption SelectedComputerRestartOption { get; set; }

    partial void OnSelectedComputerRestartOptionChanged(ComputerRestartOption value)
    {
        if (value == null || _isInitializing)
        {
            UpdateRestartUiState();
            return;
        }

        int validClockTime = Math.Clamp(ComputerRestartClockTime, ComputerRestartClockTimeMin, ComputerRestartClockTimeMax);

        _currentConfig.Computer.UpdateRestartSettings(
            value.Days,
            validClockTime,
            TimeProvider.System);

        UpdateRestartUiState();
        SaveSettings();
    }

    [ObservableProperty] public partial LanguageOption SelectedLanguageOption { get; set; }

    async partial void OnSelectedLanguageOptionChanged(LanguageOption value)
    {
        if (value == null || _isInitializing)
        {
            return;
        }

        if (_currentConfig.Settings.Language != value.Index)
        {
            _currentConfig.Settings.Language = value.Index;
            SaveSettings();
        }

        string newLanguageCode = value.Index == 0 ? LanguageCodeGerman : LanguageCodeEnglish;

        if (_languageService.CurrentLanguageCode != newLanguageCode)
        {
            _languageService.SetLanguageOption(newLanguageCode);

            bool restartNow = await _dialogService.ShowConfirmationAsync(
                _localizationService.RetrieveString(OptionsLanguageChangedRestartTitleResourceKey),
                _localizationService.RetrieveString(OptionsLanguageChangedRestartMessageResourceKey),
                _localizationService.RetrieveString(GeneralYesResourceKey),
                _localizationService.RetrieveString(GeneralNoResourceKey));

            if (restartNow)
            {
                AppInstance.Restart(string.Empty);
            }
        }
    }


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    public int ComputerRestartClockTimeMax { get; init; }
    public int ComputerRestartClockTimeMin { get; init; }

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente ──
    public ReadOnlyCollection<ComputerRestartOption> ComputerRestartList { get; }
    public ReadOnlyCollection<LanguageOption> LanguageList { get; private set; }

    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    public ViewModelOptionsGeneral(
        IInboundPortOsAppPathProvider appPathProvider,
        IInboundPortComputerRestartService computerRestartScheduler,
        IUseCaseConfigureAutoLogon configureAutoLogonUseCase,
        IDialogService dialogService,
        IOutboundPortEVisitorConfigRepository evRestarterConfigRepository,
        ILanguageHandler languageService,
        IInboundPortLocalizationProvider localizationService,
        IUseCaseManageApplicationUpdates manageApplicationUpdatesUseCase,
        IInboundPortNextRestartDateHandler nextRestartDateHandler,
        IOutboundPortOsAutoLogonRepository osAutoLogonPort,
        IOutboundPortOsProcessControl osProcessControlPort,
        IThemeHandler themeService,
        IUseCaseToggleAppAutoStart toggleAppAutoStartUseCase,
        IUIOptionsProvider uiOptionsService,
        IOutboundPortSystemInfoProvider windowsSystemInfo)
    {
        ArgumentNullException.ThrowIfNull(appPathProvider);
        ArgumentNullException.ThrowIfNull(computerRestartScheduler);
        ArgumentNullException.ThrowIfNull(configureAutoLogonUseCase);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(evRestarterConfigRepository);
        ArgumentNullException.ThrowIfNull(languageService);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(manageApplicationUpdatesUseCase);
        ArgumentNullException.ThrowIfNull(nextRestartDateHandler);
        ArgumentNullException.ThrowIfNull(osAutoLogonPort);
        ArgumentNullException.ThrowIfNull(osProcessControlPort);
        ArgumentNullException.ThrowIfNull(themeService);
        ArgumentNullException.ThrowIfNull(toggleAppAutoStartUseCase);
        ArgumentNullException.ThrowIfNull(uiOptionsService);
        ArgumentNullException.ThrowIfNull(windowsSystemInfo);

        _isInitializing = true;

        _appPathProvider = appPathProvider;
        _computerRestartScheduler = computerRestartScheduler;
        _configureAutoLogonUseCase = configureAutoLogonUseCase;
        _dialogService = dialogService;
        _evRestarterConfigRepository = evRestarterConfigRepository;
        _languageService = languageService;
        _localizationService = localizationService;
        _manageApplicationUpdatesUseCase = manageApplicationUpdatesUseCase;
        _nextRestartDateHandler = nextRestartDateHandler;
        _osAutoLogonPort = osAutoLogonPort;
        _osProcessControlPort = osProcessControlPort;
        _themeService = themeService;
        _toggleAppAutoStartUseCase = toggleAppAutoStartUseCase;
        _uiOptionsService = uiOptionsService;
        _windowsSystemInfo = windowsSystemInfo;

        _dispatcherQueue = DispatcherQueue.GetForCurrentThread() ?? throw new InvalidOperationException("DispatcherQueue required.");

        ComputerRestartList = new ReadOnlyCollection<ComputerRestartOption>([.. _uiOptionsService.GetComputerRestartOptions()]);
        LanguageList = new ReadOnlyCollection<LanguageOption>([.. _uiOptionsService.GetAvailableLanguages()]);

        _currentConfig = _evRestarterConfigRepository.LoadConfig();

        if (_currentConfig.Computer.NextRestartDate.HasValue && _currentConfig.Computer.ComputerRestartIntervalDays > 0)
        {
            var targetDateTime = _currentConfig.Computer.NextRestartDate.Value.Date.AddHours(_currentConfig.Computer.RestartClockTime);
            if (DateTime.Now >= targetDateTime)
            {
                _currentConfig.Computer.SetNextRestartDate(_nextRestartDateHandler.RetrieveNextRestartDate(_currentConfig.Computer.ComputerRestartIntervalDays, _currentConfig.Computer.RestartClockTime));
                SaveSettings();
            }
        }

        ComputerRestartClockTimeMin = 1;
        ComputerRestartClockTimeMax = 23;

        int initialClockTime = Math.Clamp(_currentConfig.Computer.RestartClockTime, ComputerRestartClockTimeMin, ComputerRestartClockTimeMax);
        if (_currentConfig.Computer.RestartClockTime != initialClockTime)
        {
            _currentConfig.Computer.UpdateRestartSettings(
                _currentConfig.Computer.ComputerRestartIntervalDays,
                initialClockTime,
                TimeProvider.System);
        }
        ComputerRestartClockTime = initialClockTime;

        int configDays = _currentConfig.Computer.ComputerRestartIntervalDays;
        int configLanguageIndex = _currentConfig.Settings.Language;

        SelectedComputerRestartOption = ComputerRestartList.FirstOrDefault(option => option.Days == configDays) ?? ComputerRestartList[0];
        SelectedLanguageOption = LanguageList.FirstOrDefault(option => option.Index == configLanguageIndex) ?? LanguageList[0];

        _isInitializing = false;

        _computerRestartScheduler.NextRestartDateChanged += OnNextRestartDateChanged;

        InitializeAsync().Forget();
    }

    // ═══════════════════════════════════════════════════════
    //  7. Commands
    // ═══════════════════════════════════════════════════════
    [RelayCommand(CanExecute = nameof(CanCheckForUpdates))]
    private async Task CheckForUpdatesAsync()
    {
        IsCheckingForUpdates = true;

        try
        {
            var response = await _manageApplicationUpdatesUseCase.CheckForUpdatesAsync();

            if (response.IsUpdateAvailable)
            {
                IsUpdateAvailable = true;
                string messageFormat = _localizationService.RetrieveString(OptionsUpdateAvailableResourceKey);
                UpdateMessage = string.Format(messageFormat, response.LatestVersion);

                string promptFormat = _localizationService.RetrieveString(OptionsUpdateAvailablePromptMessageResourceKey);
                string dialogMessage = string.Format(promptFormat, UpdateMessage);

                bool userWantsUpdate = await _dialogService.ShowConfirmationAsync(
                    _localizationService.RetrieveString(OptionsUpdateAvailableTitleResourceKey),
                    dialogMessage,
                    _localizationService.RetrieveString(GeneralYesResourceKey),
                    _localizationService.RetrieveString(GeneralNoResourceKey));

                if (userWantsUpdate)
                {
                    await PerformUpdateCoreAsync();
                }
            }
            else
            {
                IsUpdateAvailable = false;
                UpdateMessage = string.Empty;

                await _dialogService.ShowMessageAsync(
                    _localizationService.RetrieveString(OptionsUpdateNoUpdateTitleResourceKey),
                    _localizationService.RetrieveString(OptionsUpdateNoUpdateMessageResourceKey),
                    DialogIcon.Information);
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            string errorFormat = _localizationService.RetrieveString(OptionsUpdateCheckErrorMessageResourceKey);
            await _dialogService.ShowMessageAsync(
                _localizationService.RetrieveString(OptionsUpdateErrorTitleResourceKey),
                string.Format(errorFormat, exception.Message),
                DialogIcon.Error);
        }
        finally
        {
            IsCheckingForUpdates = false;
        }
    }

    [RelayCommand]
    private async Task ConfigureAutoLogonAsync()
    {
        string currentUser = Environment.UserName;
        string currentDomain = Environment.UserDomainName;

        bool isPasswordlessEnabled = _osAutoLogonPort.IsPasswordlessAuthEnabled();
        bool isAdmin = _windowsSystemInfo.IsUserAdministrator();

        var autoLogonDialogResult = await _dialogService.ShowAutoLogonDialogAsync(currentUser, currentDomain, isPasswordlessEnabled, isAdmin);

        if (autoLogonDialogResult == null)
        {
            return;
        }

        var configureAutoLogonRequest = new ConfigureAutoLogonRequest(
            IsDeactivateAction: autoLogonDialogResult.IsDeactivateAction,
            DisablePasswordlessMode: autoLogonDialogResult.DisablePasswordlessMode,
            RestorePasswordlessMode: autoLogonDialogResult.RestorePasswordlessMode,
            Username: autoLogonDialogResult.Credentials?.Username,
            Domain: autoLogonDialogResult.Credentials?.Domain,
            Password: autoLogonDialogResult.Credentials?.Password);

        var result = _configureAutoLogonUseCase.Execute(configureAutoLogonRequest);

        if (result.IsSuccess)
        {
            if (result.Value == AutoLogonResultStatus.Deactivated)
            {
                await _dialogService.ShowMessageAsync(
                    _localizationService.RetrieveString(GeneralInfoResourceKey),
                    _localizationService.RetrieveString(OptionsAutoLogonDeactivatedResourceKey));
            }
            else if (result.Value == AutoLogonResultStatus.Activated)
            {
                await _dialogService.ShowMessageAsync(
                    _localizationService.RetrieveString(GeneralSuccessResourceKey),
                    _localizationService.RetrieveString(OptionsAutoLogonActivatedResourceKey));
            }
            return;
        }

        var error = result.Errors[0];
        var status = error.Metadata.TryGetValue("Status", out var statusValue) ? (AutoLogonResultStatus)statusValue : AutoLogonResultStatus.UnexpectedError;

        if (status == AutoLogonResultStatus.WindowsHelloBlockActive)
        {
            await _dialogService.ShowMessageAsync(
                _localizationService.RetrieveString(OptionsAutoLogonWindowsHelloErrorTitleResourceKey),
                _localizationService.RetrieveString(OptionsAutoLogonWindowsHelloErrorMessageResourceKey),
                DialogIcon.Error);
        }
        else if (status == AutoLogonResultStatus.AdminRequired)
        {
            await _dialogService.ShowMessageAsync(
                _localizationService.RetrieveString(GeneralErrorKey),
                AdminRequiredMessage,
                DialogIcon.Error);
        }
        else if (status == AutoLogonResultStatus.ValidationError)
        {
            await _dialogService.ShowMessageAsync(
                _localizationService.RetrieveString(GeneralErrorKey),
                _localizationService.RetrieveString(OptionsAutoLogonValidationErrorResourceKey));
        }
        else if (status == AutoLogonResultStatus.DomainError)
        {
            await _dialogService.ShowMessageAsync(
                _localizationService.RetrieveString(GeneralErrorKey),
                _localizationService.RetrieveString(OptionsAutoLogonDomainErrorResourceKey));
        }
        else
        {
            string errorFormat = _localizationService.RetrieveString(GeneralUnexpectedErrorResourceKey);
            await _dialogService.ShowMessageAsync(
                _localizationService.RetrieveString(GeneralErrorKey),
                string.Format(errorFormat, error.Message));
        }
    }

    [RelayCommand]
    private void OpenSettingsDataFolder()
    {
        _osProcessControlPort.OpenDirectoryInFileBrowser(_appPathProvider.RetrieveAppDataPath());
    }

    [RelayCommand]
    private Task PerformUpdateAsync() => PerformUpdateCoreAsync();

    [RelayCommand]
    private void SetDarkTheme()
    {
        _themeService.SetTheme(DarkThemeName);
        SaveThemeConfig(DarkThemeName);
    }

    [RelayCommand]
    private void SetLightTheme()
    {
        _themeService.SetTheme(LightThemeName);
        SaveThemeConfig(LightThemeName);
    }

    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    private bool CanCheckForUpdates() => !IsCheckingForUpdates;

    private async Task InitializeAsync()
    {
        _isInitializing = true;

        try
        {
            StartWithWindows = await _toggleAppAutoStartUseCase.InitializeAndGetStateAsync();
        }
        finally
        {
            _isInitializing = false;
        }
    }

    private void OnNextRestartDateChanged(object? sender, DateTime? newDate)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            _currentConfig.Computer.SetNextRestartDate(newDate);
            UpdateRestartUiState();
        });
    }

    private async Task PerformUpdateCoreAsync()
    {
        IsCheckingForUpdates = true;

        try
        {
            await _manageApplicationUpdatesUseCase.PerformUpdateAsync();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);

            string errorFormat = _localizationService.RetrieveString(OptionsUpdateFailedMessageResourceKey);

            await _dialogService.ShowMessageAsync(
                _localizationService.RetrieveString(OptionsUpdateFailedTitleResourceKey),
                string.Format(errorFormat, exception.Message),
                DialogIcon.Error);
        }
        finally
        {
            IsCheckingForUpdates = false;
        }
    }

    private void SaveSettings()
    {
        var freshConfig = _evRestarterConfigRepository.LoadConfig();

        freshConfig.Computer.UpdateRestartSettings(_currentConfig.Computer.ComputerRestartIntervalDays, _currentConfig.Computer.RestartClockTime, TimeProvider.System);
        freshConfig.Computer.SetNextRestartDate(_currentConfig.Computer.NextRestartDate);
        freshConfig.Settings.Language = _currentConfig.Settings.Language;
        freshConfig.Settings.StartWithWindows = _currentConfig.Settings.StartWithWindows;

        _evRestarterConfigRepository.SaveConfig(freshConfig);
    }

    private void SaveThemeConfig(string theme)
    {
        var config = _evRestarterConfigRepository.LoadConfig();
        config.Settings.Theme = theme;
        _evRestarterConfigRepository.SaveConfig(config);
    }

    private async Task ToggleAutoStartAsync(bool enable)
    {
        await _toggleAppAutoStartUseCase.ToggleAsync(enable);
        _currentConfig.Settings.StartWithWindows = enable;
    }

    private void UpdateRestartUiState()
    {
        int days = SelectedComputerRestartOption?.Days ?? 0;
        IsRestartSliderVisible = days > 0;

        if (days == 0)
        {
            RestartStatusText = _localizationService.RetrieveString(OptionsRestartStatusNoneResourceKey);
        }
        else
        {
            DateTime targetDate = _currentConfig.Computer.NextRestartDate ?? DateTime.MinValue;
            if (targetDate == DateTime.MinValue)
            {
                targetDate = DateTime.Today.AddDays(days).AddHours(ComputerRestartClockTime);
            }

            string messageFormat = _localizationService.RetrieveString(OptionsRestartStatusScheduledResourceKey);
            RestartStatusText = string.Format(messageFormat, targetDate.ToString("dd.MM.yyyy"), targetDate.ToString("HH"));
        }
    }
}
