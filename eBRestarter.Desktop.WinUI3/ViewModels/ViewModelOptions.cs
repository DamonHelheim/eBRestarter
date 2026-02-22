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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    /// <summary>
    /// View model for the options/settings page. Manages theme, language, restart schedule,
    /// auto-login, API credentials, and update checks. Persists changes through
    /// <see cref="IEVisitorConfigService"/> and coordinates with OS services for startup and theme.
    /// </summary>
    public partial class ViewModelOptions : ObservableObject
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IWindowsAutoLogonService _autoLogonService;
        private readonly ICredentialValidationService _credentialValidationService;
        private readonly AppConfig _currentConfig;
        private readonly IDialogService _dialogService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private bool _isInitializing = false;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IOperatingSystemFacade _os;
        private readonly IRestartCalculationService _restartCalculationService;
        private readonly IThemeService _themeService;
        private readonly IUpdateService _updateService;

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

        /// <summary>Read-only list of available languages from localization; used to bind the language picker.</summary>
        public ReadOnlyCollection<LanguageOption> LanguageList { get; private set; }
        /// <summary>Read-only list of computer restart intervals (e.g. daily, weekly) from localization.</summary>
        public ReadOnlyCollection<ComputerRestartOption> ComputerRestartList { get; }
        /// <summary>Minimum allowed hour (0–23) for scheduled restart.</summary>
        public int ComputerRestartClockTimeMin { get; init; }
        /// <summary>Maximum allowed hour (0–23) for scheduled restart.</summary>
        public int ComputerRestartClockTimeMax { get; init; }

        #endregion

        // =========================================================
        // 4. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        /// <summary>
        /// Builds the options VM from config and services, populates dropdowns from localization,
        /// applies saved theme/language/restart settings, and kicks off async init (e.g. startup-with-Windows state).
        /// </summary>
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

            ComputerRestartList = new ReadOnlyCollection<ComputerRestartOption>([.. localizationService.GetComputerRestartOptions()]);

            LanguageList = new ReadOnlyCollection<LanguageOption>([.. localizationService.GetAvailableLanguages()]);

            _currentConfig = _eVisitorConfigService.LoadConfig();

            ComputerRestartClockTimeMin = 0;
            ComputerRestartClockTimeMax = 23;
            ComputerRestartClockTime = _currentConfig.Computer.RestartClockTime;

            var configDays = _currentConfig.Browser.DeleteBrowserCacheIntervalDays;
            var configLanguageIndex = _currentConfig.Settings.Language;

            SelectedComputerRestartOption = ComputerRestartList.FirstOrDefault(option => option.Days == configDays) ?? ComputerRestartList[0];
            SelectedLanguageOption = LanguageList.FirstOrDefault(option => option.Index == configLanguageIndex) ?? LanguageList[0];

            _isInitializing = false;

            _ = InitializeAsync();
        }

        #endregion

        // =========================================================
        // 5. COMMANDS (MVVM Actions)
        // =========================================================
        #region Commands

        /// <summary>
        /// Checks for application updates via <see cref="IUpdateService"/>. If an update is available,
        /// sets <see cref="IsUpdateAvailable"/> and <see cref="UpdateMessage"/> for the UI.
        /// </summary>
        [RelayCommand]
        private async Task CheckForUpdates()
        {
            try
            {
                var updateInfo = await _updateService.CheckForUpdateAsync();

                if (updateInfo.IsUpdateAvailable)
                {
                    IsUpdateAvailable = true;
                    string messageFormat = _localizationService.GetString("Options_UpdateAvailable");
                    UpdateMessage = string.Format(messageFormat, updateInfo.LatestVersion);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        /// <summary>
        /// Re-checks for updates and, if available, downloads and starts the installer via
        /// <see cref="IUpdateService.DownloadAndInstallAsync"/>.
        /// </summary>
        [RelayCommand]
        private async Task PerformUpdate()
        {
            var updateInfo = await _updateService.CheckForUpdateAsync();

            if (updateInfo.IsUpdateAvailable)
            {
                await _updateService.DownloadAndInstallAsync(updateInfo);
            }
        }

        /// <summary>Opens the application data folder in Windows Explorer using the configured base path.</summary>
        [RelayCommand]
        private async Task OpenSettingsDataFolder()
        {
            _os.WindowsProcessControlService.OpenExplorer(SystemPaths.ApplicationDataBasePath);
        }

        /// <summary>Applies the light theme via <see cref="IThemeService"/> and persists the choice in config.</summary>
        [RelayCommand]
        private void SetLightTheme()
        {
            _themeService.SetTheme("Light");
            SaveThemeConfig("Light");
        }

        /// <summary>Applies the dark theme via <see cref="IThemeService"/> and persists the choice in config.</summary>
        [RelayCommand]
        private void SetDarkTheme()
        {
            _themeService.SetTheme("Dark");
            SaveThemeConfig("Dark");
        }

        /// <summary>Opens the dialog to activate API credentials (username/key).</summary>
        [RelayCommand]
        private async Task ShowActivateApiDialog()
        {
            await _dialogService.ShowActivateApiDialogAsync();
        }

        /// <summary>Opens the dialog to import API credentials from a file.</summary>
        [RelayCommand]
        private async Task ShowImportApiDialog()
        {
            await _dialogService.ShowImportApiDialogAsync();
        }

        /// <summary>
        /// Clears stored API username and key from config and shows a success message.
        /// Does not validate or revoke on the server; only local storage is cleared.
        /// </summary>
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

        /// <summary>
        /// Opens the auto-logon configuration dialog. If the user confirms, enables or disables
        /// Windows auto-logon via <see cref="IWindowsAutoLogonService"/> after validating credentials.
        /// </summary>
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

        /// <summary>Syncs StartWithWindows with the OS startup manager and, if config says start-with-Windows but OS was off, enables it.</summary>
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

        /// <summary>Shows or hides the restart-time slider and sets RestartStatusText from next restart date or "none".</summary>
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

                string messageFormat = _localizationService.GetString("Options_RestartStatus_Scheduled");

                RestartStatusText = string.Format(messageFormat, targetDate.ToString("dd.MM.yyyy"), targetDate.ToString("HH"));
            }
        }

        /// <summary>Computes the next restart date from interval days and clock time and stores it in config.</summary>
        private void RecalculateNextRestartDate()
        {
            int days = _currentConfig.Computer.ComputerRestartIntervalDays;
            int hours = _currentConfig.Computer.RestartClockTime;

            _currentConfig.Computer.NextRestartDate = _restartCalculationService.GetNextRestartDate(days, hours);
        }

        private async void ToggleAutoStartAsync(bool enable)
        {
            if (enable)
            {
                await _os.WindowsStartupManagerService.EnableAutoStartAsync();
            }
            else {

                await _os.WindowsStartupManagerService.DisableAutoStartAsync();
            }

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
