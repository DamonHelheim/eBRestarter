using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Constants;
using eBRestarter.Core.Application.Extensions;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;
using eBRestarter.Core.Application.UseCases.ManageApplicationUpdates;
using eBRestarter.Core.Application.UseCases.ToggleAppAutoStart;
using eBRestarter.Core.Domain.Entities;
using eBRestarter.Desktop.WinUI3.Models;
using eBRestarter.Desktop.WinUI3.Models.Enums;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelOptionsGeneral : ObservableObject
    {
        private const string GeneralErrorKey = "General_Error";

        private readonly IComputerRestartDateService _computerRestartDateService;
        private readonly IComputerRestartScheduler _computerRestartScheduler;
        private readonly IConfigureAutoLogonUseCase _configureAutoLogonUseCase;
        private readonly IDialogService _dialogService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IManageApplicationUpdatesUseCase _manageApplicationUpdatesUseCase;
        private readonly IOperatingSystemFacade _operatingSystemFacade;
        private readonly IThemeService _themeService;
        private readonly IToggleAppAutoStartUseCase _toggleAppAutoStartUseCase;
        private readonly IUIOptionsService _uiOptionsService;

        private bool _isInitializing;
        private readonly AppConfig _currentConfig;
        private readonly DispatcherQueue _dispatcherQueue;

        [ObservableProperty]
        public partial int ComputerRestartClockTime { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CheckForUpdatesCommand))]
        public partial bool IsCheckingForUpdates { get; set; }

        [ObservableProperty]
        public partial bool IsRestartSliderVisible { get; set; }

        [ObservableProperty]
        public partial bool IsUpdateAvailable { get; set; }

        [ObservableProperty]
        public partial string RestartStatusText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool StartWithWindows { get; set; }

        [ObservableProperty]
        public partial string UpdateMessage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial ComputerRestartOption SelectedComputerRestartOption { get; set; }

        [ObservableProperty]
        public partial LanguageOption SelectedLanguageOption { get; set; }

        public int ComputerRestartClockTimeMax { get; init; }
        public int ComputerRestartClockTimeMin { get; init; }

        public ReadOnlyCollection<ComputerRestartOption> ComputerRestartList { get; }
        public ReadOnlyCollection<LanguageOption> LanguageList { get; private set; }

        public ViewModelOptionsGeneral(
            IDialogService dialogService,
            IConfigureAutoLogonUseCase configureAutoLogonUseCase,
            IToggleAppAutoStartUseCase toggleAppAutoStartUseCase,
            IManageApplicationUpdatesUseCase manageApplicationUpdatesUseCase,
            IOperatingSystemFacade operatingSystemFacade,
            IThemeService themeService,
            IEVisitorConfigService eVisitorConfigService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IUIOptionsService uiOptionsService,
            IComputerRestartDateService computerRestartDateService,
            IComputerRestartScheduler computerRestartScheduler)
        {
            _isInitializing = true;

            _computerRestartDateService = computerRestartDateService;
            _computerRestartScheduler = computerRestartScheduler;
            _configureAutoLogonUseCase = configureAutoLogonUseCase;
            _dialogService = dialogService;
            _eVisitorConfigService = eVisitorConfigService;
            _languageService = languageService;
            _localizationService = localizationService;
            _manageApplicationUpdatesUseCase = manageApplicationUpdatesUseCase;
            _operatingSystemFacade = operatingSystemFacade;
            _themeService = themeService;
            _toggleAppAutoStartUseCase = toggleAppAutoStartUseCase;
            _uiOptionsService = uiOptionsService;

            _dispatcherQueue = DispatcherQueue.GetForCurrentThread() ?? throw new InvalidOperationException("DispatcherQueue required.");

            ComputerRestartList = new ReadOnlyCollection<ComputerRestartOption>([.. _uiOptionsService.GetComputerRestartOptions()]);
            LanguageList = new ReadOnlyCollection<LanguageOption>([.. _uiOptionsService.GetAvailableLanguages()]);

            _currentConfig = _eVisitorConfigService.LoadConfig();

            if (_currentConfig.Computer.NextRestartDate.HasValue && _currentConfig.Computer.ComputerRestartIntervalDays > 0)
            {
                var targetDateTime = _currentConfig.Computer.NextRestartDate.Value.Date.AddHours(_currentConfig.Computer.RestartClockTime);
                if (DateTime.Now >= targetDateTime)
                {
                    _currentConfig.Computer.SetNextRestartDate(_computerRestartDateService.RetrieveNextRestartDate(
                        _currentConfig.Computer.ComputerRestartIntervalDays,
                        _currentConfig.Computer.RestartClockTime));
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

            _computerRestartScheduler.OnNextRestartDateChanged += OnNextRestartDateChanged;

            InitializeAsync().Forget();
        }

        [RelayCommand(CanExecute = nameof(CanCheckForUpdates))]
        private async Task CheckForUpdates()
        {
            IsCheckingForUpdates = true;

            try
            {
                var response = await _manageApplicationUpdatesUseCase.CheckForUpdatesAsync();

                if (response.IsUpdateAvailable)
                {
                    IsUpdateAvailable = true;
                    string messageFormat = _localizationService.RetrieveString("Options_UpdateAvailable");
                    UpdateMessage = string.Format(messageFormat, response.LatestVersion);

                    string promptFormat = _localizationService.RetrieveString("Options_UpdatePrompt_Message");
                    string dialogMessage = string.Format(promptFormat, UpdateMessage);

                    bool userWantsUpdate = await _dialogService.ShowConfirmationAsync(
                        _localizationService.RetrieveString("Options_UpdateAvailable_Title"),
                        dialogMessage,
                        _localizationService.RetrieveString("General_Yes"),
                        _localizationService.RetrieveString("General_No"));

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
                        _localizationService.RetrieveString("Options_UpdateNoUpdate_Title"),
                        _localizationService.RetrieveString("Options_UpdateNoUpdate_Message"),
                        DialogIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                string errorFormat = _localizationService.RetrieveString("Options_UpdateCheckError_Message");
                await _dialogService.ShowMessageAsync(
                    _localizationService.RetrieveString("Options_UpdateError_Title"),
                    string.Format(errorFormat, ex.Message),
                    DialogIcon.Error);
            }
            finally
            {
                IsCheckingForUpdates = false;
            }
        }

        [RelayCommand]
        private async Task ConfigureAutoLogon()
        {
            string currentUser = Environment.UserName;
            string currentDomain = Environment.UserDomainName;

            bool isPasswordlessEnabled = _operatingSystemFacade.WindowsAutoLogonService.IsWindowsHelloPasswordlessEnabled();
            bool isAdmin = _operatingSystemFacade.WindowsSystemInfoService.IsUserAdministrator();

            var autoLogonDialogResult = await _dialogService.ShowAutoLogonDialogAsync(currentUser, currentDomain, isPasswordlessEnabled, isAdmin);

            if (autoLogonDialogResult == null)
                return;

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
                        _localizationService.RetrieveString("General_Info"),
                        _localizationService.RetrieveString("Options_AutoLogon_Deactivated"));
                }
                else if (result.Value == AutoLogonResultStatus.Activated)
                {
                    await _dialogService.ShowMessageAsync(
                        _localizationService.RetrieveString("General_Success"),
                        _localizationService.RetrieveString("Options_AutoLogon_Success"));
                }
            }
            else
            {
                var error = result.Errors[0];
                var status = error.Metadata.TryGetValue("Status", out var s) ? (AutoLogonResultStatus)s : AutoLogonResultStatus.UnexpectedError;

                if (status == AutoLogonResultStatus.WindowsHelloBlockActive)
                {
                    await _dialogService.ShowMessageAsync(
                        _localizationService.RetrieveString("Options_AutoLogon_WindowsHelloErrorTitle"),
                        _localizationService.RetrieveString("Options_AutoLogon_WindowsHelloErrorMessage"),
                        DialogIcon.Error);
                }
                else if (status == AutoLogonResultStatus.AdminRequired)
                {
                    await _dialogService.ShowMessageAsync(
                        _localizationService.RetrieveString("General_Error"),
                        "Es sind Administratorrechte erforderlich, um diese Aktion auszuführen. Bitte starten Sie die Anwendung als Administrator.",
                        DialogIcon.Error);
                }
                else if (status == AutoLogonResultStatus.ValidationError)
                {
                    await _dialogService.ShowMessageAsync(
                        _localizationService.RetrieveString(GeneralErrorKey),
                        _localizationService.RetrieveString("Options_AutoLogon_ValidationError"));
                }
                else if (status == AutoLogonResultStatus.DomainError)
                {
                    await _dialogService.ShowMessageAsync(
                        _localizationService.RetrieveString(GeneralErrorKey),
                        _localizationService.RetrieveString("Options_AutoLogon_DomainError"));
                }
                else
                {
                    string errorFormat = _localizationService.RetrieveString("General_UnexpectedError");
                    await _dialogService.ShowMessageAsync(
                        _localizationService.RetrieveString(GeneralErrorKey),
                        string.Format(errorFormat, error.Message));
                }
            }
        }

        [RelayCommand]
        private void OpenSettingsDataFolder()
        {
            _operatingSystemFacade.WindowsProcessControlService.OpenExplorer(SystemPaths.ApplicationDataBasePath);
        }

        [RelayCommand]
        private async Task PerformUpdate()
        {
            await PerformUpdateCoreAsync();
        }

        [RelayCommand]
        private void SetDarkTheme()
        {
            _themeService.SetTheme("Dark");
            SaveThemeConfig("Dark");
        }

        [RelayCommand]
        private void SetLightTheme()
        {
            _themeService.SetTheme("Light");
            SaveThemeConfig("Light");
        }

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
                    TimeProvider.System); // Or inject TimeProvider
                UpdateRestartUiState();
                SaveSettings();
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
                TimeProvider.System); // Or inject TimeProvider
            UpdateRestartUiState();
            SaveSettings();
        }

        async partial void OnSelectedLanguageOptionChanged(LanguageOption value)
        {
            if (value == null || _isInitializing)
                return;

            if (_currentConfig.Settings.Language != value.Index)
            {
                _currentConfig.Settings.Language = value.Index;
                SaveSettings();
            }

            string newLanguageCode = value.Index == 0 ? "de-DE" : "en-US";

            if (_languageService.CurrentLanguageCode != newLanguageCode)
            {
                _languageService.SetLanguage(newLanguageCode);

                bool restartNow = await _dialogService.ShowConfirmationAsync(
                    _localizationService.RetrieveString("Options_LanguageChanged_Restart_Title"),
                    _localizationService.RetrieveString("Options_LanguageChanged_Restart_Message"),
                    _localizationService.RetrieveString("General_Yes"),
                    _localizationService.RetrieveString("General_No"));

                if (restartNow)
                {
                    AppInstance.Restart(string.Empty);
                }
            }
        }

        partial void OnStartWithWindowsChanged(bool value)
        {
            if (_isInitializing)
                return;

            ToggleAutoStartAsync(value).Forget();
        }

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

        private async Task PerformUpdateCoreAsync()
        {
            IsCheckingForUpdates = true;
            try
            {
                await _manageApplicationUpdatesUseCase.PerformUpdateAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                string errorFormat = _localizationService.RetrieveString("Options_UpdateFailed_Message");
                await _dialogService.ShowMessageAsync(
                    _localizationService.RetrieveString("Options_UpdateFailed_Title"),
                    string.Format(errorFormat, ex.Message),
                    DialogIcon.Error);
            }
            finally
            {
                IsCheckingForUpdates = false;
            }
        }

        private void RecalculateNextRestartDate()
        {
            _currentConfig.Computer.CalculateNextRestartDate(TimeProvider.System);
        }

        private void SaveSettings()
        {
            var freshConfig = _eVisitorConfigService.LoadConfig();

            freshConfig.Computer.UpdateRestartSettings(_currentConfig.Computer.ComputerRestartIntervalDays, _currentConfig.Computer.RestartClockTime, TimeProvider.System);
            freshConfig.Computer.SetNextRestartDate(_currentConfig.Computer.NextRestartDate);
            freshConfig.Settings.Language = _currentConfig.Settings.Language;
            freshConfig.Settings.StartWithWindows = _currentConfig.Settings.StartWithWindows;

            _eVisitorConfigService.SaveConfig(freshConfig);
        }

        private void SaveThemeConfig(string theme)
        {
            var config = _eVisitorConfigService.LoadConfig();
            config.Settings.Theme = theme;
            _eVisitorConfigService.SaveConfig(config);
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
                RestartStatusText = _localizationService.RetrieveString("Options_RestartStatus_None");
            }
            else
            {
                DateTime targetDate = _currentConfig.Computer.NextRestartDate ?? DateTime.MinValue;
                if (targetDate == DateTime.MinValue)
                {
                    targetDate = DateTime.Today.AddDays(days).AddHours(ComputerRestartClockTime);
                }

                string messageFormat = _localizationService.RetrieveString("Options_RestartStatus_Scheduled");
                RestartStatusText = string.Format(messageFormat, targetDate.ToString("dd.MM.yyyy"), targetDate.ToString("HH"));
            }
        }
    }
}
