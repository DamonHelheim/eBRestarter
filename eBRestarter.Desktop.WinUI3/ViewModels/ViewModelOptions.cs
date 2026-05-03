using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Constants;
using eBRestarter.Core.Application.Extensions;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Interfaces.Update;
using eBRestarter.Core.Application.Models.Config;
using eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;
using eBRestarter.Core.Application.UseCases.ManageApplicationUpdates;
using eBRestarter.Core.Application.UseCases.RemoveApiCredentials;
using eBRestarter.Core.Application.UseCases.ToggleAppAutoStart;
using eBRestarter.Desktop.WinUI3.Messages;
using eBRestarter.Desktop.WinUI3.Models;
using eBRestarter.Desktop.WinUI3.Models.Enums;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
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
        private const string ExtensionConfigFileName = "tab_restarter_config.json";

        private const string GeneralErrorKey = "General_Error";

        private const int ExtensionSaveStatusDisplayDurationMilliseconds = 3000;

        private const int MillisecondsPerMinute = 60000;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            TypeInfoResolver = ExtensionConfigJsonContext.Default
        };

        private readonly IBrowserExtensionDeploymentService _browserExtensionDeploymentService;

        private readonly IComputerRestartDateService _computerRestartDateService;

        private readonly IComputerRestartScheduler _computerRestartScheduler;

        private readonly IConfigureAutoLogonUseCase _configureAutoLogonUseCase;

        private readonly IDialogService _dialogService;

        private readonly IEVisitorConfigService _eVisitorConfigService;

        private readonly ILanguageService _languageService;

        private readonly ILocalizationService _localizationService;

        private readonly IManageApplicationUpdatesUseCase _manageApplicationUpdatesUseCase;

        private readonly IOperatingSystemFacade _operatingSystemFacade;

        private readonly IRemoveApiCredentialsUseCase _removeApiCredentialsUseCase;

        private readonly IThemeService _themeService;

        private readonly IToggleAppAutoStartUseCase _toggleAppAutoStartUseCase;

        private readonly IUIOptionsService _uiOptionsService;

        private bool _isInitializing;

        private readonly AppConfig _currentConfig;

        private readonly DispatcherQueue _dispatcherQueue;

        [ObservableProperty]
        public partial int ComputerRestartClockTime { get; set; }

        [ObservableProperty]
        public partial string ExtensionSaveStatus { get; set; } = string.Empty;

        [ObservableProperty]
        public partial double ExtensionWaitTimeMinutes { get; set; } = 3;

        [ObservableProperty]
        public partial string ExtensionUrl { get; set; } = string.Empty;

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
        public partial LanguageOption SelectedExtensionLanguage { get; set; }

        [ObservableProperty]
        public partial LanguageOption SelectedLanguageOption { get; set; }

        /// <summary>Maximum allowed hour (1–23) for scheduled restart.</summary>
        public int ComputerRestartClockTimeMax { get; init; }

        /// <summary>Minimum allowed hour (1–23) for scheduled restart.</summary>
        public int ComputerRestartClockTimeMin { get; init; }

        public ReadOnlyCollection<ComputerRestartOption> ComputerRestartList { get; }

        public ReadOnlyCollection<LanguageOption> ExtensionLanguages { get; }

        /// <summary>Read-only list of available languages from localization; used to bind the language picker.</summary>
        public ReadOnlyCollection<LanguageOption> LanguageList { get; private set; }

        /// <summary>
        /// Builds the options VM from config and services, populates dropdowns from localization,
        /// applies saved theme/language/restart settings, and kicks off async init (e.g. startup-with-Windows state).
        /// </summary>
        public ViewModelOptions(
            IDialogService dialogService,
            IConfigureAutoLogonUseCase configureAutoLogonUseCase,
            IToggleAppAutoStartUseCase toggleAppAutoStartUseCase,
            IManageApplicationUpdatesUseCase manageApplicationUpdatesUseCase,
            IRemoveApiCredentialsUseCase removeApiCredentialsUseCase,
            IOperatingSystemFacade operatingSystemFacade,
            IThemeService themeService,
            IEVisitorConfigService eVisitorConfigService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IUIOptionsService uiOptionsService,
            IComputerRestartDateService computerRestartDateService,
            IComputerRestartScheduler computerRestartScheduler,
            IBrowserExtensionDeploymentService browserExtensionDeploymentService)
        {
            ArgumentNullException.ThrowIfNull(dialogService);
            ArgumentNullException.ThrowIfNull(configureAutoLogonUseCase);
            ArgumentNullException.ThrowIfNull(toggleAppAutoStartUseCase);
            ArgumentNullException.ThrowIfNull(manageApplicationUpdatesUseCase);
            ArgumentNullException.ThrowIfNull(removeApiCredentialsUseCase);
            ArgumentNullException.ThrowIfNull(operatingSystemFacade);
            ArgumentNullException.ThrowIfNull(themeService);
            ArgumentNullException.ThrowIfNull(eVisitorConfigService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(localizationService);
            ArgumentNullException.ThrowIfNull(uiOptionsService);
            ArgumentNullException.ThrowIfNull(computerRestartDateService);
            ArgumentNullException.ThrowIfNull(computerRestartScheduler);
            ArgumentNullException.ThrowIfNull(browserExtensionDeploymentService);

            _isInitializing = true;

            _browserExtensionDeploymentService = browserExtensionDeploymentService;
            _computerRestartDateService = computerRestartDateService;
            _computerRestartScheduler = computerRestartScheduler;
            _configureAutoLogonUseCase = configureAutoLogonUseCase;
            _dialogService = dialogService;
            _eVisitorConfigService = eVisitorConfigService;
            _languageService = languageService;
            _localizationService = localizationService;
            _manageApplicationUpdatesUseCase = manageApplicationUpdatesUseCase;
            _operatingSystemFacade = operatingSystemFacade;
            _removeApiCredentialsUseCase = removeApiCredentialsUseCase;
            _themeService = themeService;
            _toggleAppAutoStartUseCase = toggleAppAutoStartUseCase;
            _uiOptionsService = uiOptionsService;

            _dispatcherQueue =
                DispatcherQueue.GetForCurrentThread()
                ?? throw new InvalidOperationException(
                    $"{nameof(ViewModelOptions)} must be constructed on a thread with a WinUI DispatcherQueue (UI thread).");

            _browserExtensionDeploymentService.EnsureExtensionIsDeployed();
            ComputerRestartList = new ReadOnlyCollection<ComputerRestartOption>([.. _uiOptionsService.GetComputerRestartOptions()]);
            LanguageList = new ReadOnlyCollection<LanguageOption>([.. _uiOptionsService.GetAvailableLanguages()]);

            if (ComputerRestartList.Count == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(ViewModelOptions)} requires at least one entry from {nameof(IUIOptionsService.GetComputerRestartOptions)}.");
            }

            if (LanguageList.Count == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(ViewModelOptions)} requires at least one entry from {nameof(IUIOptionsService.GetAvailableLanguages)} for the language list.");
            }

            _currentConfig = _eVisitorConfigService.LoadConfig();

            ComputerRestartClockTimeMin = 1;
            ComputerRestartClockTimeMax = 23;
            ComputerRestartClockTime = _currentConfig.Computer.RestartClockTime;

            int configDays = _currentConfig.Computer.ComputerRestartIntervalDays;
            int configLanguageIndex = _currentConfig.Settings.Language;

            SelectedComputerRestartOption = ComputerRestartList.FirstOrDefault(option => option.Days == configDays) ?? ComputerRestartList[0];
            SelectedLanguageOption = LanguageList.FirstOrDefault(option => option.Index == configLanguageIndex) ?? LanguageList[0];

            _isInitializing = false;

            _computerRestartScheduler.OnNextRestartDateChanged += OnNextRestartDateChanged;

            ExtensionLanguages = new ReadOnlyCollection<LanguageOption>([.. _uiOptionsService.GetAvailableLanguages()]);
            if (ExtensionLanguages.Count == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(ViewModelOptions)} requires at least one entry from {nameof(IUIOptionsService.GetAvailableLanguages)} for extension languages.");
            }

            SelectedExtensionLanguage = ExtensionLanguages[0];

            LoadExtensionConfig();

            InitializeAsync().Forget();
        }

        /// <summary>
        /// Checks for application updates via <see cref="IUpdateService"/>. If an update is available,
        /// asks the user via dialog if they want to install it right away.
        /// </summary>
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

                    string messageFormat = _localizationService.GetString("Options_UpdateAvailable");
                    UpdateMessage = string.Format(messageFormat, response.LatestVersion);

                    string title = _localizationService.GetString("Options_UpdateAvailable_Title");

                    string promptFormat = _localizationService.GetString("Options_UpdatePrompt_Message");
                    string dialogMessage = string.Format(promptFormat, UpdateMessage);

                    bool userWantsUpdate = await _dialogService.ShowConfirmationAsync(
                        title,
                        dialogMessage,
                        _localizationService.GetString("General_Yes"),
                        _localizationService.GetString("General_No"));

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
                        _localizationService.GetString("Options_UpdateNoUpdate_Title"),
                        _localizationService.GetString("Options_UpdateNoUpdate_Message"),
                        DialogIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                string errorFormat = _localizationService.GetString("Options_UpdateCheckError_Message");
                string errorMessage = string.Format(errorFormat, ex.Message);

                await _dialogService.ShowMessageAsync(
                    _localizationService.GetString("Options_UpdateError_Title"),
                    errorMessage,
                    DialogIcon.Error);
            }
            finally
            {
                IsCheckingForUpdates = false;
            }
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

            var autoLogonDialogResult = await _dialogService.ShowAutoLogonDialogAsync(currentUser, currentDomain);

            if (autoLogonDialogResult == null)
                return;

            var configureAutoLogonRequest = new ConfigureAutoLogonRequest(
                IsDeactivateAction: autoLogonDialogResult.IsDeactivateAction,
                Username: autoLogonDialogResult.Credentials?.Username,
                Domain: autoLogonDialogResult.Credentials?.Domain,
                Password: autoLogonDialogResult.Credentials?.Password);

            var configureAutoLogonResponse = _configureAutoLogonUseCase.Execute(configureAutoLogonRequest);

            if (configureAutoLogonResponse.Success)
            {
                if (configureAutoLogonResponse.Status == AutoLogonResultStatus.Deactivated)
                {
                    await _dialogService.ShowMessageAsync(
                        _localizationService.GetString("General_Info"),
                        _localizationService.GetString("Options_AutoLogon_Deactivated"));
                }
                else if (configureAutoLogonResponse.Status == AutoLogonResultStatus.Activated)
                {
                    await _dialogService.ShowMessageAsync(
                        _localizationService.GetString("General_Success"),
                        _localizationService.GetString("Options_AutoLogon_Success"));
                }
            }
            else
            {
                if (configureAutoLogonResponse.Status == AutoLogonResultStatus.WindowsHelloBlockActive)
                {
                    string title = _localizationService.GetString("Options_AutoLogon_WindowsHelloErrorTitle");
                    string message = _localizationService.GetString("Options_AutoLogon_WindowsHelloErrorMessage");

                    await _dialogService.ShowMessageAsync(title, message, DialogIcon.Error);
                }
                else if (configureAutoLogonResponse.Status == AutoLogonResultStatus.ValidationError)
                {
                    await _dialogService.ShowMessageAsync(
                        _localizationService.GetString(GeneralErrorKey),
                        _localizationService.GetString("Options_AutoLogon_ValidationError"));
                }
                else if (configureAutoLogonResponse.Status == AutoLogonResultStatus.DomainError)
                {
                    await _dialogService.ShowMessageAsync(
                        _localizationService.GetString(GeneralErrorKey),
                        _localizationService.GetString("Options_AutoLogon_DomainError"));
                }
                else
                {
                    string errorFormat = _localizationService.GetString("General_UnexpectedError");
                    await _dialogService.ShowMessageAsync(
                        _localizationService.GetString(GeneralErrorKey),
                        string.Format(errorFormat, configureAutoLogonResponse.ErrorMessage));
                }
            }
        }

        /// <summary>
        /// Opens the folder of the Chrome extension in Windows Explorer.
        /// </summary>
        [RelayCommand]
        private async Task OpenExtensionFolder()
        {
            try
            {
                string extensionPath = _browserExtensionDeploymentService.GetExtensionFolderPath();

                if (!Directory.Exists(extensionPath))
                {
                    Directory.CreateDirectory(extensionPath);
                }

                _operatingSystemFacade.WindowsProcessControlService.OpenExplorer(extensionPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                string messageFormat = _localizationService.GetString("Options_ExtensionFolderOpenError_Message");
                await _dialogService.ShowMessageAsync(
                    _localizationService.GetString("Options_ExtensionFolderOpenError_Title"),
                    string.Format(messageFormat, ex.Message),
                    DialogIcon.Error);
            }
        }

        /// <summary>Opens the application data folder in Windows Explorer using the configured base path.</summary>
        [RelayCommand]
        private void OpenSettingsDataFolder()
        {
            _operatingSystemFacade.WindowsProcessControlService.OpenExplorer(SystemPaths.ApplicationDataBasePath);
        }

        /// <summary>
        /// Downloads the update and starts the installer via <see cref="IUpdateService.DownloadAndInstallAsync"/>.
        /// Closes the application upon success.
        /// </summary>
        [RelayCommand]
        private async Task PerformUpdate()
        {
            await PerformUpdateCoreAsync();
        }

        /// <summary>
        /// Clears stored API username and key from config and shows a success message.
        /// Does not validate or revoke on the server; only local storage is cleared.
        /// </summary>
        [RelayCommand]
        private async Task RemoveAPICredentials()
        {
            _removeApiCredentialsUseCase.Execute();

            _currentConfig.Settings.ApiUsername = string.Empty;
            _currentConfig.Settings.ApiKey = string.Empty;

            WeakReferenceMessenger.Default.Send(new ApiCredentialsRemovedMessage());

            await _dialogService.ShowMessageAsync(
                _localizationService.GetString("Options_RemoveCreds_Title"),
                _localizationService.GetString("Options_RemoveCreds_Message"),
                DialogIcon.Success);
        }

        [RelayCommand]
        private async Task SaveExtensionConfig()
        {
            try
            {
                string extensionPath = _browserExtensionDeploymentService.GetExtensionFolderPath();
                string configPath = Path.Combine(extensionPath, ExtensionConfigFileName);

                var extensionConfigDto = new ExtensionConfigDto
                {
                    LANGUAGE = SelectedExtensionLanguage?.Index == 0 ? "DE" : "EN",
                    ZIEL_URL = $"{WebLinks.EVisitorSurflink}{_currentConfig.Username}",
                    WARTEZEIT_MS = (int)(ExtensionWaitTimeMinutes * MillisecondsPerMinute)
                };

                string jsonString = JsonSerializer.Serialize(extensionConfigDto, _jsonSerializerOptions);

                if (!Directory.Exists(extensionPath))
                {
                    Directory.CreateDirectory(extensionPath);
                }

                await File.WriteAllTextAsync(configPath, jsonString);

                ExtensionSaveStatus = _localizationService.GetString("Options_SavedSuccessfully") ?? string.Empty;

                await Task.Delay(ExtensionSaveStatusDisplayDurationMilliseconds);

                ExtensionSaveStatus = string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                await _dialogService.ShowMessageAsync(
                    _localizationService.GetString(GeneralErrorKey),
                    _localizationService.GetString("BrowserExtension_Config_Error_Message") + " " + ex.Message,
                    DialogIcon.Error);
            }
        }

        /// <summary>Applies the dark theme via <see cref="IThemeService"/> and persists the choice in config.</summary>
        [RelayCommand]
        private void SetDarkTheme()
        {
            _themeService.SetTheme("Dark");
            SaveThemeConfig("Dark");
        }

        /// <summary>Applies the light theme via <see cref="IThemeService"/> and persists the choice in config.</summary>
        [RelayCommand]
        private void SetLightTheme()
        {
            _themeService.SetTheme("Light");
            SaveThemeConfig("Light");
        }

        /// <summary>Opens the dialog to activate API credentials (username/key).</summary>
        [RelayCommand]
        private async Task ShowActivateApiDialog()
        {
            await _dialogService.ShowActivateApiDialogAsync();
        }

        // Method order: (1) ObservableProperty partials and OnNextRestartDateChanged, A–Z; (2) remaining private helpers, A–Z.
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

        private void OnNextRestartDateChanged(object? sender, DateTime? newDate)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                _currentConfig.Computer.NextRestartDate = newDate;
                UpdateRestartUiState();
            });
        }

        // ObservableProperty source-generated partials use 'value' as the parameter name (toolkit convention).
        partial void OnSelectedComputerRestartOptionChanged(ComputerRestartOption value)
        {
            if (value == null || _isInitializing)
            {
                UpdateRestartUiState();
                return;
            }

            _currentConfig.Computer.ComputerRestartIntervalDays = value.Days;

            RecalculateNextRestartDate();
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
                    _localizationService.GetString("Options_LanguageChanged_Restart_Title"),
                    _localizationService.GetString("Options_LanguageChanged_Restart_Message"),
                    _localizationService.GetString("General_Yes"),
                    _localizationService.GetString("General_No"));

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

        /// <summary>Syncs StartWithWindows with the OS startup manager and, if config says start-with-Windows but OS was off, enables it.</summary>
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

        /// <summary>Reads extension tab_restarter_config.json at startup and fills UI fields.</summary>
        private void LoadExtensionConfig()
        {
            try
            {
                string configPath = Path.Combine(_browserExtensionDeploymentService.GetExtensionFolderPath(), ExtensionConfigFileName);

                if (File.Exists(configPath))
                {
                    string jsonString = File.ReadAllText(configPath);

                    var extensionConfigDto = JsonSerializer.Deserialize(jsonString, ExtensionConfigJsonContext.Default.ExtensionConfigDto);

                    if (extensionConfigDto != null)
                    {
                        ExtensionUrl = extensionConfigDto.ZIEL_URL;
                        ExtensionWaitTimeMinutes = extensionConfigDto.WARTEZEIT_MS / (double)MillisecondsPerMinute;

                        LanguageOption? matchingExtensionLanguage = extensionConfigDto.LANGUAGE == "EN"
                            ? ExtensionLanguages.FirstOrDefault(languageOption => languageOption.Index == 1)
                            : ExtensionLanguages.FirstOrDefault(languageOption => languageOption.Index == 0);

                        if (matchingExtensionLanguage != null)
                        {
                            SelectedExtensionLanguage = matchingExtensionLanguage;
                        }
                        else
                        {
                            Debug.WriteLine(
                                "Extension tab_restarter_config.json LANGUAGE does not match any UI language option; keeping current selection.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not read existing tab_restarter_config.json: {ex}");
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

                string errorFormat = _localizationService.GetString("Options_UpdateFailed_Message");
                string errorMessage = string.Format(errorFormat, ex.Message);

                await _dialogService.ShowMessageAsync(
                    _localizationService.GetString("Options_UpdateFailed_Title"),
                    errorMessage,
                    DialogIcon.Error);
            }
            finally
            {
                IsCheckingForUpdates = false;
            }
        }

        /// <summary>Computes the next restart date from interval days and clock time and stores it in config.</summary>
        private void RecalculateNextRestartDate()
        {
            int days = _currentConfig.Computer.ComputerRestartIntervalDays;
            int hours = _currentConfig.Computer.RestartClockTime;

            _currentConfig.Computer.NextRestartDate = _computerRestartDateService.GetNextRestartDate(days, hours);
        }

        private void SaveSettings()
        {
            _eVisitorConfigService.SaveConfig(_currentConfig);
        }

        private void SaveThemeConfig(string theme)
        {
            var config = _eVisitorConfigService.LoadConfig();
            var newConfig = config with { Settings = config.Settings with { Theme = theme } };

            _eVisitorConfigService.SaveConfig(newConfig);
        }

        private async Task ToggleAutoStartAsync(bool enable)
        {
            await _toggleAppAutoStartUseCase.ToggleAsync(enable);

            _currentConfig.Settings.StartWithWindows = enable;
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
    }
}
