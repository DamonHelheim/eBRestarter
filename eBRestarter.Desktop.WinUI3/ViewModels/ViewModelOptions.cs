using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Contstants;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Interfaces.Update;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Core.Domain.Models.Records.Config;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Infrastructure.Constants;
using Microsoft.Windows.ApplicationModel.Resources; // <--- HIER wichtig
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement; // Wichtig: Referenz hinzufügen!
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelOptions : ObservableObject
    {
        // =========================================================
        // 1. FIELDS (Private Felder & Services)
        // =========================================================

        private bool _isInitializing = false; // Sperre-Flag

        private readonly AppConfig _currentConfig;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly IDialogService _dialogService;
        private readonly IWindowsAutoLogonService _autoLogonService;
        private readonly IOperatingSystemFacade _os;
        private readonly IUpdateService _updateService;
        private readonly IThemeService _themeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;


        // =========================================================
        // 2. PROPERTIES (Öffentliche Eigenschaften)
        // =========================================================

        // Konstante Listen & Limits
        //public ReadOnlyCollection<ComputerRestartOption> ComputerRestartList => ComputerRestartConstants.Options;
        //public ReadOnlyCollection<LanguageOption> LanguageList => LanguageSelectionConstants.Options;
        public ReadOnlyCollection<LanguageOption> LanguageList { get; private set; }
        public ReadOnlyCollection<ComputerRestartOption> ComputerRestartList { get; }
        public int ComputerRestartClockTimeMin { get; init; }
        public int ComputerRestartClockTimeMax { get; init; }

        // Observable Properties (UI-Bindings)
        [ObservableProperty] public partial ComputerRestartOption SelectedComputerRestartOption { get; set; }
        [ObservableProperty] public partial LanguageOption SelectedLanguageOption { get; set; }
        [ObservableProperty] public partial int ComputerRestartClockTime { get; set; }
        [ObservableProperty] public partial bool StartWithWindows { get; set; } = false;
        // UI-State Properties (Sichtbarkeit & Text)
        [ObservableProperty] public partial bool IsRestartSliderVisible { get; set; }
        [ObservableProperty] public partial string RestartStatusText { get; set; } = string.Empty;

        // UI Properties
        [ObservableProperty] public partial bool IsUpdateAvailable { get; set; }
        [ObservableProperty] public partial string UpdateMessage { get; set; } = string.Empty;

        // =========================================================
        // 3. CONSTRUCTOR
        // =========================================================

        public ViewModelOptions(
            IDialogService dialogService,
            IWindowsAutoLogonService autoLogonService,
            IOperatingSystemFacade os,
            IUpdateService updateService,
            IThemeService themeService,
            IEVisitorConfigService eVisitorConfigService,
            ILanguageService languageService,
            ILocalizationService localizationService)
        {
            // 1. Initialisierungsschutz aktivieren
            _isInitializing = true;

            _dialogService = dialogService;
            _autoLogonService = autoLogonService;
            _os = os;
            _updateService = updateService;
            _themeService = themeService;
            _eVisitorConfigService = eVisitorConfigService;
            _languageService = languageService;
            _localizationService = localizationService;

            ComputerRestartList = new ReadOnlyCollection<ComputerRestartOption>(
            [.. localizationService.GetComputerRestartOptions()]
        );
            // 1. Liste direkt vom Service holen (eine Zeile!)
            // Das ViewModel muss nicht wissen, woher die Texte kommen (Resx, Datenbank, API...)
            LanguageList = new ReadOnlyCollection<LanguageOption>(
                [.. localizationService.GetAvailableLanguages()]
            );

            // Config laden
            _currentConfig = _eVisitorConfigService.LoadConfig();

            // Limits setzen
            ComputerRestartClockTimeMin = 0;
            ComputerRestartClockTimeMax = 23;
            ComputerRestartClockTime = _currentConfig.Computer.RestartClockTime;

            // ComboBox Vorbelegung
            var configDays = _currentConfig.Browser.DeleteBrowserCacheIntervalDays;
            var configLanguageIndex = _currentConfig.Settings.Language;

            SelectedComputerRestartOption = ComputerRestartList.FirstOrDefault(x => x.Days == configDays) ?? ComputerRestartList[0];

            // HIER passierte der Fehler: Das Setzen feuert das Event. 
            // Durch _isInitializing = true wird der Code im Handler jetzt aber übersprungen.
            SelectedLanguageOption = LanguageList.FirstOrDefault(x => x.Index == configLanguageIndex) ?? LanguageList[0];

            // 2. Initialisierungsschutz deaktivieren (WICHTIG!)
            _isInitializing = false;

            // Async Initialisierung starten (Fire & Forget)
            // Hinweis: InitializeAsync setzt _isInitializing intern ggf. wieder kurz auf true, das ist okay.
            _ = InitializeAsync();

        }

        // =========================================================
        // 4. COMMANDS
        // =========================================================

        [RelayCommand]
        private async Task CheckForUpdates()
        {
            try
            {
                var info = await _updateService.CheckForUpdateAsync();

                if (info.IsUpdateAvailable)
                {
                    IsUpdateAvailable = true;
                    UpdateMessage = $"Version {info.LatestVersion} verfügbar!";

                    // HIER: Trigger für einen Dialog in der View
                    // In MVVM nutzt man hierfür oft einen Messenger oder einen DialogService.
                    // Simples Beispiel: Property setzen, UI blendet Button ein.
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
            // Info nochmal holen oder cachen
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

        private void SaveThemeConfig(string theme)
        {
            var config = _eVisitorConfigService.LoadConfig();
            var newConfig = config with { Settings = config.Settings with { Theme = theme } };
            _eVisitorConfigService.SaveConfig(newConfig);
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
            // 1. Config laden
            var currentConfig = _eVisitorConfigService.LoadConfig();
            // 2. Daten entfernen
            var newConfig = currentConfig with
            {
                Settings = currentConfig.Settings with
                {
                    ApiUsername = string.Empty,
                    ApiKey = string.Empty
                }
            };
            // 3. Speichern
            _eVisitorConfigService.SaveConfig(newConfig);

            await _dialogService.ShowMessageAsync("API Zugang", "Deine API Zugangsdaten wurden entfernt.", DialogIcon.Success);
        }

        [RelayCommand]
        private async Task ConfigureAutoLogon()
        {
            // 1. Aktuellen Windows-Nutzer auslesen
            string currentUser = Environment.UserName;
            string currentDomain = Environment.UserDomainName;

            // 2. Dialog anzeigen
            var dialogResult = await _dialogService.ShowAutoLogonDialogAsync(currentUser, currentDomain);

            if (dialogResult == null) return; // Abgebrochen

            try
            {
                // FALL A: Deaktivieren
                if (dialogResult.IsDeactivateAction)
                {
                    _autoLogonService.DisableAutoLogon();
                    await _dialogService.ShowMessageAsync("Info", "Die automatische Anmeldung wurde deaktiviert.");
                }
                // FALL B: Aktivieren / Speichern
                else if (dialogResult.Credentials != null)
                {
                    var user = dialogResult.Credentials.Username;
                    var domain = dialogResult.Credentials.Domain;
                    var pass = dialogResult.Credentials.Password;

                    // Validierung
                    bool isValid = ValidateCredentials(user, domain, pass);

                    if (!isValid)
                    {
                        await _dialogService.ShowMessageAsync("Fehler", "Benutzername oder Passwort sind nicht korrekt. Bitte prüfen Sie die Eingaben.");
                        return;
                    }

                    _autoLogonService.EnableAutoLogon(user, domain, pass);
                    await _dialogService.ShowMessageAsync("Erfolg", "Die automatische Anmeldung wurde eingerichtet.");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync("Fehler", $"Ein unerwarteter Fehler ist aufgetreten: {ex.Message}");
            }
        }

        // =========================================================
        // 5. PROPERTY CHANGE HANDLERS (Partial Methods)
        // =========================================================

        partial void OnSelectedComputerRestartOptionChanged(ComputerRestartOption value)
        {
            _currentConfig.Computer.ComputerRestartIntervalDays = value.Days;

            RecalculateNextRestartDate();
            UpdateRestartUiState();
            SaveSettings();
        }

        partial void OnComputerRestartClockTimeChanged(int value)
        {
            // Clamping & UI Korrektur
            int clampedValue = Math.Clamp(value, ComputerRestartClockTimeMin, ComputerRestartClockTimeMax);
            if (value != clampedValue)
            {
                ComputerRestartClockTime = clampedValue;
                return;
            }

            // Speichern
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

            // 1. Config speichern
            if (_currentConfig.Settings.Language != value.Index)
            {
                _currentConfig.Settings.Language = value.Index;
                SaveSettings();
            }

            // 2. Mapping: Index zu Sprachcode
            string newLanguageCode = value.Index == 0 ? "de-DE" : "en-US";

            // 3. Wenn sich die Sprache geändert hat
            if (_languageService.CurrentLanguageCode != newLanguageCode)
            {
                _languageService.SetLanguage(newLanguageCode);

                // 4. FRAGE STELLEN: Neustart?
                bool restartNow = await _dialogService.ShowConfirmationAsync(
                    "Neustart erforderlich / Restart required",
                    "Die Sprache wurde geändert. Damit alle Texte aktualisiert werden, muss die Anwendung neu gestartet werden.\n\nMöchten Sie die Anwendung jetzt neustarten?\n\n(The language has been changed. Restart now to apply all changes?)",
                    "Ja / Yes",   // Button für Ja
                    "Nein / No"   // Button für Nein
                );

                // 5. NEUSTART DURCHFÜHREN
                if (restartNow)
                {
                    // Startet die App neu und registriert sie für den Neustart
                    AppInstance.Restart(string.Empty);
                }
            }
        }

        partial void OnStartWithWindowsChanged(bool value)
        {
            if (_isInitializing) return;
            ToggleAutoStartAsync(value);
        }

        // =========================================================
        // 6. PRIVATE HELPER METHODS
        // =========================================================

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
                RestartStatusText = "Computer wird nicht neugestartet";
            }
            else
            {
                DateTime targetDate = _currentConfig.Computer.NextRestartDate ?? DateTime.MinValue;

                if (targetDate == DateTime.MinValue)
                {
                    targetDate = DateTime.Today.AddDays(days).AddHours(ComputerRestartClockTime);
                }

                RestartStatusText = $"Computer wird am {targetDate:dd.MM.yyyy} um {targetDate:HH} Uhr neugestartet";
            }
        }

        private void RecalculateNextRestartDate()
        {
            int days = _currentConfig.Computer.ComputerRestartIntervalDays;
            int hours = _currentConfig.Computer.RestartClockTime;

            if (days > 0)
            {
                _currentConfig.Computer.NextRestartDate = DateTime.Today.AddDays(days).AddHours(hours);
            }
            else
            {
                _currentConfig.Computer.NextRestartDate = DateTime.MinValue;
            }
        }

        private async void ToggleAutoStartAsync(bool enable)
        {
            if (enable)
            {
                await _os.WindowsStartupManagerService.EnableAutoStartAsync();
            }
            else
            {
                await _os.WindowsStartupManagerService.DisableAutoStartAsync();
            }

            _currentConfig.Settings.StartWithWindows = enable;
            SaveSettings();
        }

        private bool ValidateCredentials(string username, string domain, string password)
        {
            try
            {
                ContextType contextType = ContextType.Machine;

                if (!string.Equals(domain, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
                {
                    contextType = ContextType.Domain;
                }

                using (var context = new PrincipalContext(contextType, domain))
                {
                    return context.ValidateCredentials(username, password);
                }
            }
            catch (PrincipalServerDownException)
            {
                throw new Exception("Der Domänen-Controller konnte zur Überprüfung nicht erreicht werden.");
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SaveSettings()
        {
            _eVisitorConfigService.SaveConfig(_currentConfig);
        }
    }
}
