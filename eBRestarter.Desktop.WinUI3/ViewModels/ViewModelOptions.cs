using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Interfaces.Update;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Core.Domain.Models.Records.Config;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Infrastructure.Constants;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelOptions : ObservableObject
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private bool _isInitializing = false;

        private readonly AppConfig _currentConfig;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly IDialogService _dialogService;
        private readonly IWindowsAutoLogonService _autoLogonService;
        private readonly IOperatingSystemFacade _os;
        private readonly IUpdateService _updateService;
        private readonly IThemeService _themeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IRestartCalculationService _restartCalculationService;
        private readonly ICredentialValidationService _credentialValidationService;

        #endregion

        // =========================================================
        // 2. OBSERVABLE PROPERTIES (MVVM State)
        // =========================================================
        #region ObservableProperties

        [ObservableProperty] public partial ComputerRestartOption SelectedComputerRestartOption { get; set; }
        [ObservableProperty] public partial LanguageOption SelectedLanguageOption { get; set; }
        [ObservableProperty] public partial int ComputerRestartClockTime { get; set; }
        [ObservableProperty] public partial bool StartWithWindows { get; set; } = false;

        [ObservableProperty] public partial bool IsRestartSliderVisible { get; set; }
        [ObservableProperty] public partial string RestartStatusText { get; set; } = string.Empty;

        [ObservableProperty] public partial bool IsUpdateAvailable { get; set; }
        [ObservableProperty] public partial string UpdateMessage { get; set; } = string.Empty;

        #endregion

        // =========================================================
        // 3. PUBLIC PROPERTIES (Data & State)
        // =========================================================
        #region PublicProperties

        public ReadOnlyCollection<LanguageOption> LanguageList { get; private set; }
        public ReadOnlyCollection<ComputerRestartOption> ComputerRestartList { get; }
        public int ComputerRestartClockTimeMin { get; init; }
        public int ComputerRestartClockTimeMax { get; init; }

        #endregion

        // =========================================================
        // 4. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        public ViewModelOptions(
            IDialogService dialogService,
            IWindowsAutoLogonService autoLogonService,
            IOperatingSystemFacade os,
            IUpdateService updateService,
            IThemeService themeService,
            IEVisitorConfigService eVisitorConfigService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IRestartCalculationService restartCalculationService,
            ICredentialValidationService credentialValidationService)
        {
            _isInitializing = true;

            _dialogService = dialogService;
            _autoLogonService = autoLogonService;
            _os = os;
            _updateService = updateService;
            _themeService = themeService;
            _eVisitorConfigService = eVisitorConfigService;
            _languageService = languageService;
            _localizationService = localizationService;
            _restartCalculationService = restartCalculationService;
            _credentialValidationService = credentialValidationService;

            ComputerRestartList = new ReadOnlyCollection<ComputerRestartOption>(
                [.. localizationService.GetComputerRestartOptions()]
            );

            LanguageList = new ReadOnlyCollection<LanguageOption>(
                [.. localizationService.GetAvailableLanguages()]
            );

            _currentConfig = _eVisitorConfigService.LoadConfig();

            ComputerRestartClockTimeMin = 0;
            ComputerRestartClockTimeMax = 23;
            ComputerRestartClockTime = _currentConfig.Computer.RestartClockTime;

            var configDays = _currentConfig.Browser.DeleteBrowserCacheIntervalDays;
            var configLanguageIndex = _currentConfig.Settings.Language;

            SelectedComputerRestartOption = ComputerRestartList.FirstOrDefault(x => x.Days == configDays) ?? ComputerRestartList[0];
            SelectedLanguageOption = LanguageList.FirstOrDefault(x => x.Index == configLanguageIndex) ?? LanguageList[0];

            _isInitializing = false;

            _ = InitializeAsync();
        }

        #endregion

        // =========================================================
        // 5. COMMANDS (MVVM Actions)
        // =========================================================
        #region Commands

        [RelayCommand]
        private async Task CheckForUpdates()
        {
            try
            {
                var info = await _updateService.CheckForUpdateAsync();

                if (info.IsUpdateAvailable)
                {
                    IsUpdateAvailable = true;
                    string format = _localizationService.GetString("Options_UpdateAvailable");
                    UpdateMessage = string.Format(format, info.LatestVersion);
                }
            }
            catch (Exception ex)
            {
                // Log error
            }
        }

        [RelayCommand]
        private async Task PerformUpdate()
        {
            var info = await _updateService.CheckForUpdateAsync();
            if (info.IsUpdateAvailable)
            {
                await _updateService.DownloadAndInstallAsync(info);
            }
        }

        [RelayCommand]
        private async Task OpenSettingsDataFolder()
        {
            _os.WindowsProcessControlService.OpenExplorer(SystemPaths.ApplicationDataBasePath);
        }

        [RelayCommand]
        private void SetLightTheme()
        {
            _themeService.SetTheme("Light");
            SaveThemeConfig("Light");
        }

        [RelayCommand]
        private void SetDarkTheme()
        {
            _themeService.SetTheme("Dark");
            SaveThemeConfig("Dark");
        }

        [RelayCommand]
        private async Task ShowActivateApiDialog()
        {
            await _dialogService.ShowActivateApiDialogAsync();
        }

        [RelayCommand]
        private async Task ShowImportApiDialog()
        {
            await _dialogService.ShowImportApiDialogAsync();
        }

        [RelayCommand]
        private async Task RemoveAPICredentials()
        {
            var currentConfig = _eVisitorConfigService.LoadConfig();
            var newConfig = currentConfig with
            {
                Settings = currentConfig.Settings with
                {
                    ApiUsername = string.Empty,
                    ApiKey = string.Empty
                }
            };
            _eVisitorConfigService.SaveConfig(newConfig);

            await _dialogService.ShowMessageAsync(
                _localizationService.GetString("Options_RemoveCreds_Title"),
                _localizationService.GetString("Options_RemoveCreds_Message"),
                DialogIcon.Success);
        }

        [RelayCommand]
        private async Task ConfigureAutoLogon()
        {
            string currentUser = Environment.UserName;
            string currentDomain = Environment.UserDomainName;

            var dialogResult = await _dialogService.ShowAutoLogonDialogAsync(currentUser, currentDomain);

            if (dialogResult == null) return;

            try
            {
                if (dialogResult.IsDeactivateAction)
                {
                    _autoLogonService.DisableAutoLogon();
                    await _dialogService.ShowMessageAsync("Info",
                        _localizationService.GetString("Options_AutoLogon_Deactivated"));
                }
                else if (dialogResult.Credentials != null)
                {
                    var user = dialogResult.Credentials.Username;
                    var domain = dialogResult.Credentials.Domain;
                    var pass = dialogResult.Credentials.Password;

                    bool isValid = _credentialValidationService.ValidateCredentials(user, domain, pass);

                    if (!isValid)
                    {
                        await _dialogService.ShowMessageAsync("Fehler",
                            _localizationService.GetString("Options_AutoLogon_ValidationError"));
                        return;
                    }

                    _autoLogonService.EnableAutoLogon(user, domain, pass);
                    await _dialogService.ShowMessageAsync("Erfolg",
                        _localizationService.GetString("Options_AutoLogon_Success"));
                }
            }
            catch (InvalidOperationException)
            {
                await _dialogService.ShowMessageAsync("Fehler",
                    _localizationService.GetString("Options_AutoLogon_DomainError"));
            }
            catch (Exception ex)
            {
                string errorFormat = _localizationService.GetString("General_UnexpectedError");
                await _dialogService.ShowMessageAsync("Fehler", string.Format(errorFormat, ex.Message));
            }
        }

        #endregion

        // =========================================================
        // 6. PROPERTY CHANGE HANDLERS (MVVM Hooks)
        // =========================================================
        #region PropertyChangeHandlers

        partial void OnSelectedComputerRestartOptionChanged(ComputerRestartOption value)
        {
            _currentConfig.Computer.ComputerRestartIntervalDays = value.Days;
            RecalculateNextRestartDate();
            UpdateRestartUiState();
            SaveSettings();
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
                _currentConfig.Computer.RestartClockTime = value;
                RecalculateNextRestartDate();
                UpdateRestartUiState();
                SaveSettings();
            }
        }

        async partial void OnSelectedLanguageOptionChanged(LanguageOption value)
        {
            if (value == null || _isInitializing) return;

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
                    "Neustart erforderlich / Restart required",
                    "Die Sprache wurde geändert. Damit alle Texte aktualisiert werden, muss die Anwendung neu gestartet werden.\n\nMöchten Sie die Anwendung jetzt neustarten?\n\n(The language has been changed. Restart now to apply all changes?)",
                    "Ja / Yes", "Nein / No"
                );

                if (restartNow)
                {
                    AppInstance.Restart(string.Empty);
                }
            }
        }

        partial void OnStartWithWindowsChanged(bool value)
        {
            if (_isInitializing) return;
            ToggleAutoStartAsync(value);
        }

        #endregion

        // =========================================================
        // 7. PRIVATE HELPER METHODS (Interne Hilfsmethoden)
        // =========================================================
        #region PrivateHelperMethods

        private void SaveThemeConfig(string theme)
        {
            var config = _eVisitorConfigService.LoadConfig();
            var newConfig = config with { Settings = config.Settings with { Theme = theme } };
            _eVisitorConfigService.SaveConfig(newConfig);
        }

        private async Task InitializeAsync()
        {
            _isInitializing = true;
            try
            {
                StartWithWindows = await _os.WindowsStartupManagerService.IsAutoStartEnabledAsync();

                if (_currentConfig.Settings.StartWithWindows && !StartWithWindows)
                {
                    await _os.WindowsStartupManagerService.EnableAutoStartAsync();
                }
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void UpdateRestartUiState()
        {
            int days = SelectedComputerRestartOption?.Days ?? 0;

            IsRestartSliderVisible = days > 0;

            if (days == 0)
            {
                RestartStatusText = _localizationService.GetString("Options_RestartStatus_None");
            }
            else
            {
                DateTime targetDate = _currentConfig.Computer.NextRestartDate ?? DateTime.MinValue;

                if (targetDate == DateTime.MinValue)
                {
                    targetDate = DateTime.Today.AddDays(days).AddHours(ComputerRestartClockTime);
                }

                string format = _localizationService.GetString("Options_RestartStatus_Scheduled");

                RestartStatusText = string.Format(format, targetDate.ToString("dd.MM.yyyy"), targetDate.ToString("HH"));
            }
        }

        private void RecalculateNextRestartDate()
        {
            int days = _currentConfig.Computer.ComputerRestartIntervalDays;
            int hours = _currentConfig.Computer.RestartClockTime;
            _currentConfig.Computer.NextRestartDate = _restartCalculationService.GetNextRestartDate(days, hours);
        }

        private async void ToggleAutoStartAsync(bool enable)
        {
            if (enable)
                await _os.WindowsStartupManagerService.EnableAutoStartAsync();
            else
                await _os.WindowsStartupManagerService.DisableAutoStartAsync();

            _currentConfig.Settings.StartWithWindows = enable;
            SaveSettings();
        }

        private void SaveSettings()
        {
            _eVisitorConfigService.SaveConfig(_currentConfig);
        }

        #endregion
    }
}
