using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Contstants;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Interfaces.Update;
using eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;
using eBRestarter.Core.Application.UseCases.ManageApplicationUpdates;
using eBRestarter.Core.Application.UseCases.RemoveApiCredentials;
using eBRestarter.Core.Application.UseCases.ToggleAppAutoStart;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Extensions;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Core.Domain.Models.Records.Config;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Infrastructure.Constants;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        private readonly IConfigureAutoLogonUseCase _configureAutoLogonUseCase;
        private readonly IToggleAppAutoStartUseCase _toggleAppAutoStartUseCase;
        private readonly IManageApplicationUpdatesUseCase _manageApplicationUpdatesUseCase;
        private readonly IRemoveApiCredentialsUseCase _removeApiCredentialsUseCase;
        private readonly AppConfig _currentConfig;
        private readonly IDialogService _dialogService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private bool _isInitializing = false;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IOperatingSystemFacade _os;
        private readonly IRestartCalculationService _restartCalculationService;
        private readonly IThemeService _themeService;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;
        private readonly IComputerRestartScheduler _computerRestartScheduler;
        private readonly IBrowserExtensionDeploymentService _browserExtensionDeploymentService;

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

        [ObservableProperty] public partial string ExtensionUrl { get; set; } = string.Empty;
        [ObservableProperty] public partial double ExtensionWaitTimeMinutes { get; set; } = 3;
        [ObservableProperty] public partial LanguageOption SelectedExtensionLanguage { get; set; }
        [ObservableProperty] public partial string ExtensionSaveStatus { get; set; } = string.Empty;

        // NEU: Steuert Ladekreis und Button-Zustand
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CheckForUpdatesCommand))]
        public partial bool IsCheckingForUpdates { get; set; } = false;

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

        public ReadOnlyCollection<LanguageOption> ExtensionLanguages { get; }

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
            IConfigureAutoLogonUseCase configureAutoLogonUseCase,
            IToggleAppAutoStartUseCase toggleAppAutoStartUseCase,
            IManageApplicationUpdatesUseCase manageApplicationUpdatesUseCase,
            IRemoveApiCredentialsUseCase removeApiCredentialsUseCase,
            IOperatingSystemFacade os,
            IThemeService themeService,
            IEVisitorConfigService eVisitorConfigService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IRestartCalculationService restartCalculationService,
            IComputerRestartScheduler computerRestartScheduler,
            IBrowserExtensionDeploymentService browserExtensionDeploymentService)
        {
            _isInitializing = true;

            _dialogService = dialogService;
            _configureAutoLogonUseCase = configureAutoLogonUseCase;
            _toggleAppAutoStartUseCase = toggleAppAutoStartUseCase;
            _manageApplicationUpdatesUseCase = manageApplicationUpdatesUseCase;
            _removeApiCredentialsUseCase = removeApiCredentialsUseCase;
            _os = os;
            _themeService = themeService;
            _eVisitorConfigService = eVisitorConfigService;
            _languageService = languageService;
            _localizationService = localizationService;
            _restartCalculationService = restartCalculationService;
            _computerRestartScheduler = computerRestartScheduler;
            _browserExtensionDeploymentService = browserExtensionDeploymentService;

            _browserExtensionDeploymentService.EnsureExtensionIsDeployed();
            ComputerRestartList = new ReadOnlyCollection<ComputerRestartOption>([.. localizationService.GetComputerRestartOptions()]);

            LanguageList = new ReadOnlyCollection<LanguageOption>([.. localizationService.GetAvailableLanguages()]);

            _currentConfig = _eVisitorConfigService.LoadConfig();

            ComputerRestartClockTimeMin = 1;
            ComputerRestartClockTimeMax = 23;
            ComputerRestartClockTime = _currentConfig.Computer.RestartClockTime;

            var configDays = _currentConfig.Computer.ComputerRestartIntervalDays;
            var configLanguageIndex = _currentConfig.Settings.Language;

            SelectedComputerRestartOption = ComputerRestartList.FirstOrDefault(option => option.Days == configDays) ?? ComputerRestartList[0];
            SelectedLanguageOption = LanguageList.FirstOrDefault(option => option.Index == configLanguageIndex) ?? LanguageList[0];

            _isInitializing = false;

            _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

            _computerRestartScheduler.OnNextRestartDateChanged += OnNextRestartDateChanged;

            // Erweiterungs-Sprachen initialisieren (Wir leihen uns einfach die normalen Languages)
            ExtensionLanguages = new ReadOnlyCollection<LanguageOption>([.. localizationService.GetAvailableLanguages()]);
            SelectedExtensionLanguage = ExtensionLanguages.FirstOrDefault()!;

            // Lade die Extension Config beim Start
            LoadExtensionConfig();

            InitializeAsync().Forget();


        }

        #endregion

        // =========================================================
        // 5. COMMANDS (MVVM Actions)
        // =========================================================
        #region Commands

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

                    // 1. Nachricht für die UI laden und formatieren (z.B. "Version 1.2.0 ist verfügbar")
                    string messageFormat = _localizationService.GetString("Options_UpdateAvailable");
                    UpdateMessage = string.Format(messageFormat, response.LatestVersion);

                    // 2. Titel für den Dialog laden
                    string title = _localizationService.GetString("Options_UpdateAvailable_Title");

                    // 3. Dialog-Nachricht mit dem Platzhalter {0} laden und befüllen
                    string promptFormat = _localizationService.GetString("Options_UpdatePrompt_Message");
                    string dialogMessage = string.Format(promptFormat, UpdateMessage);

                    // 4. Dialog anzeigen mit lokalisierten Ja/Nein Buttons
                    bool userWantsUpdate = await _dialogService.ShowConfirmationAsync(
                        title,
                        dialogMessage,
                        _localizationService.GetString("General_Yes"),
                        _localizationService.GetString("General_No")
                    );

                    if (userWantsUpdate)
                    {
                        await PerformUpdate();
                    }
                }
                else
                {
                    IsUpdateAvailable = false;
                    UpdateMessage = string.Empty;

                    // Meldung: Kein Update verfügbar
                    await _dialogService.ShowMessageAsync(
                        _localizationService.GetString("Options_UpdateNoUpdate_Title"),
                        _localizationService.GetString("Options_UpdateNoUpdate_Message"),
                        DialogIcon.Information
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                // Fehlermeldung formatieren (Platzhalter {0} wird mit ex.Message befüllt)
                string errorFormat = _localizationService.GetString("Options_UpdateCheckError_Message");
                string errorMessage = string.Format(errorFormat, ex.Message);

                await _dialogService.ShowMessageAsync(
                    _localizationService.GetString("Options_UpdateError_Title"),
                    errorMessage,
                    DialogIcon.Error
                );
            }
            finally
            {
                IsCheckingForUpdates = false;
            }
        }

        /// <summary>
        /// Downloads the update and starts the installer via <see cref="IUpdateService.DownloadAndInstallAsync"/>.
        /// Closes the application upon success.
        /// </summary>
        [RelayCommand]
        private async Task PerformUpdate()
        {
            IsCheckingForUpdates = true;

            try
            {
                await _manageApplicationUpdatesUseCase.PerformUpdateAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                // Fehlermeldung formatieren (Platzhalter {0} wird mit ex.Message befüllt)
                string errorFormat = _localizationService.GetString("Options_UpdateFailed_Message");
                string errorMessage = string.Format(errorFormat, ex.Message);

                await _dialogService.ShowMessageAsync(
                    _localizationService.GetString("Options_UpdateFailed_Title"),
                    errorMessage,
                    DialogIcon.Error
                );
            }
            finally
            {
                IsCheckingForUpdates = false;
            }
        }

        /// <summary>
        /// Öffnet den Ordner der Chrome Extension im Windows Explorer
        /// </summary>
        [RelayCommand]
        private void OpenExtensionFolder()
        {
            try
            {
                string extensionPath = _browserExtensionDeploymentService.GetExtensionFolderPath();

                if (!Directory.Exists(extensionPath))
                {
                    Directory.CreateDirectory(extensionPath);
                }

                _os.WindowsProcessControlService.OpenExplorer(extensionPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim Öffnen des Extension-Ordners: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SaveExtensionConfig()
        {
            try
            {
                string extensionPath = _browserExtensionDeploymentService.GetExtensionFolderPath();
                string configPath = Path.Combine(extensionPath, "tab_restarter_config.json");

                // Erstelle das Datenobjekt. Zeit: Minuten * 60.000 (ms)
                var configData = new ExtensionConfigDto
                {
                    LANGUAGE = SelectedExtensionLanguage?.Index == 0 ? "DE" : "EN", // Index 0 ist meist DE
                    ZIEL_URL = $"{WebLinks.EVisitorSurflink}{_currentConfig.Username}",
                    WARTEZEIT_MS = (int)(ExtensionWaitTimeMinutes * 60000)
                };

                //JSON Formatierung mit Source Generator (AOT-kompatibel)
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,

                    // Hier sagen wir dem Serializer, wo er die Struktur der Klasse findet:
                    TypeInfoResolver = ExtensionConfigJsonContext.Default
                };

                string jsonString = JsonSerializer.Serialize(configData, options);

                // In Datei schreiben
                if (!Directory.Exists(_browserExtensionDeploymentService.GetExtensionFolderPath()))
                {
                    Directory.CreateDirectory(_browserExtensionDeploymentService.GetExtensionFolderPath());
                }

                await File.WriteAllTextAsync(configPath, jsonString);

                // Erfolgsmeldung für 3 Sekunden anzeigen
                ExtensionSaveStatus = _localizationService.GetString("Options_SavedSuccessfully") ?? "Erfolgreich gespeichert!";

                await Task.Delay(3000);

                ExtensionSaveStatus = string.Empty;
            }
            catch (Exception ex)
            {

                await _dialogService.ShowMessageAsync(_localizationService.GetString("General_Error"), _localizationService.GetString("BrowserExtension_Config_Error_Message") + " " + ex.Message, DialogIcon.Error);
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
            _removeApiCredentialsUseCase.Execute();

            // Resync current config in memory
            _currentConfig.Settings.ApiUsername = string.Empty;
            _currentConfig.Settings.ApiKey = string.Empty;

            WeakReferenceMessenger.Default.Send(new ApiCredentialsRemovedMessage());

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

            var request = new ConfigureAutoLogonRequest(
                IsDeactivateAction: dialogResult.IsDeactivateAction,
                Username: dialogResult.Credentials?.Username,
                Domain: dialogResult.Credentials?.Domain,
                Password: dialogResult.Credentials?.Password
            );

            // Hier führt der Service die Prüfung durch, ob Windows Hello aktiv ist!
            var response = _configureAutoLogonUseCase.Execute(request);

            if (response.Success)
            {
                if (response.Status == AutoLogonResultStatus.Deactivated)
                {
                    await _dialogService.ShowMessageAsync("Info", _localizationService.GetString("Options_AutoLogon_Deactivated"));
                }
                else if (response.Status == AutoLogonResultStatus.Activated)
                {
                    await _dialogService.ShowMessageAsync(_localizationService.GetString("General_Success"), _localizationService.GetString("Options_AutoLogon_Success"));
                }
            }
            else
            {
                // =========================================================
                // NEU: Abfangen der Windows 11 "Passwordless" Blockade
                // =========================================================
                if (response.Status == AutoLogonResultStatus.WindowsHelloBlockActive)
                {
                    string title = _localizationService.GetString("Options_AutoLogon_WindowsHelloErrorTitle");
                    string message = _localizationService.GetString("Options_AutoLogon_WindowsHelloErrorMessage");

                    await _dialogService.ShowMessageAsync(title, message, DialogIcon.Error);
                }
                // =========================================================
                else if (response.Status == AutoLogonResultStatus.ValidationError)
                {
                    await _dialogService.ShowMessageAsync(_localizationService.GetString("General_Error"), _localizationService.GetString("Options_AutoLogon_ValidationError"));
                }
                else if (response.Status == AutoLogonResultStatus.DomainError)
                {
                    await _dialogService.ShowMessageAsync(_localizationService.GetString("General_Error"), _localizationService.GetString("Options_AutoLogon_DomainError"));
                }
                else
                {
                    string errorFormat = _localizationService.GetString("General_UnexpectedError");
                    await _dialogService.ShowMessageAsync(_localizationService.GetString("General_Error"), string.Format(errorFormat, response.ErrorMessage));
                }
            }
        }

        #endregion

        // =========================================================
        // 6. PROPERTY CHANGE HANDLERS (MVVM Hooks)
        // =========================================================
        #region PropertyChangeHandlers

        private void OnNextRestartDateChanged(object? sender, DateTime? newDate)
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                _currentConfig.Computer.NextRestartDate = newDate;
                UpdateRestartUiState();
            });
        }



        partial void OnSelectedComputerRestartOptionChanged(ComputerRestartOption value)
        {
            // LÖSUNG: Wenn das ViewModel gerade startet oder der Wert null ist -> sofort abbrechen!
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
            ToggleAutoStartAsync(value).Forget();
        }

        #endregion

        // =========================================================
        // 7. PRIVATE HELPER METHODS (Interne Hilfsmethoden)
        // =========================================================
        #region PrivateHelperMethods

        /// <summary>
        /// Sucht den Ordner "Extension" neben der ausführbaren .exe Datei.
        /// </summary>
        /// <summary>
        /// Sucht den Ordner "Extension" je nach Build-Modus (Debug vs Release).
        /// </summary>
        /// <summary>
        /// Sucht den Ordner "Extension" je nach Build-Modus (Debug vs Release).
        /// </summary>
        //        private string GetExtensionFolderPath()
        //        {
        //#if DEBUG
        //            // Im Debug-Modus gehen wir 6 Ebenen nach oben in den Solution-Root-Ordner.
        //            // Von: ...\eBRestarter\eBRestarter.Desktop.WinUI3\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\
        //            // Nach: ...\eBRestarter\
        //            string solutionDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\..\"));

        //            // Nun navigieren wir in dein JavaScript-Projekt
        //            return Path.Combine(solutionDirectory, "eBRestarter.TabRestarterExtension", "TabRestarterExtension");
        //#else
        //    // Im Release-Modus (fertig publizierte App) liegt der Ordner
        //    // idealerweise direkt neben der ausführbaren .exe Datei.
        //    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RedirectExtension");
        //#endif
        //        }

        /// <summary>
        /// Liest die config.json beim Programmstart aus und füllt die UI-Felder
        /// </summary>
        //[RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
        private void LoadExtensionConfig()
        {
            try
            {
                string configPath = Path.Combine(_browserExtensionDeploymentService.GetExtensionFolderPath(), "tab_restarter_config.json");

                if (File.Exists(configPath))
                {
                    string jsonString = File.ReadAllText(configPath);

                    var configData = JsonSerializer.Deserialize(jsonString, ExtensionConfigJsonContext.Default.ExtensionConfigDto);

                    if (configData != null)
                    {
                        ExtensionUrl = configData.ZIEL_URL;
                        ExtensionWaitTimeMinutes = configData.WARTEZEIT_MS / 60000.0; // MS zurück in Minuten

                        // Sprache setzen
                        if (configData.LANGUAGE == "EN")
                            SelectedExtensionLanguage = ExtensionLanguages.FirstOrDefault(l => l.Index == 1)!; // 1 = Englisch
                        else
                            SelectedExtensionLanguage = ExtensionLanguages.FirstOrDefault(l => l.Index == 0)!; // 0 = Deutsch
                    }
                }
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Konnte existierende config.json nicht lesen: {ex.Message}");
            }
        }

        // Generiert den Code für das JSON-Mapping beim Kompilieren!
        [JsonSerializable(typeof(ExtensionConfigDto))]
        public partial class ExtensionConfigJsonContext : JsonSerializerContext
        {
        }

        // NEU: Verhindert Mehrfachklicks während der Suche
        private bool CanCheckForUpdates() => !IsCheckingForUpdates;

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
                StartWithWindows = await _toggleAppAutoStartUseCase.InitializeAndGetStateAsync();
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

        private async Task ToggleAutoStartAsync(bool enable)
        {
            await _toggleAppAutoStartUseCase.ToggleAsync(enable);

            // Sync current config view
            _currentConfig.Settings.StartWithWindows = enable;
        }

        private void SaveSettings()
        {
            _eVisitorConfigService.SaveConfig(_currentConfig);
        }

        #endregion
    }
}
