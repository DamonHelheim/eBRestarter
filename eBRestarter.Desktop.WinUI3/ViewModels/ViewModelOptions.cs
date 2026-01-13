using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Contstants;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Core.Domain.Models.Records.Config;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
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
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using eBRestarter.Core.Application.Interfaces.Update;
    using eBRestarter.Infrastructure.Constants;
    using System.Collections.ObjectModel;
    using System.DirectoryServices.AccountManagement;

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


        // =========================================================
        // 2. PROPERTIES (Öffentliche Eigenschaften)
        // =========================================================

        // Konstante Listen & Limits
        public ReadOnlyCollection<ComputerRestartOption> ComputerRestartList => ComputerRestartConstants.Options;
        public ReadOnlyCollection<LanguageOption> LanguageList => LanguageSelectionConstants.Options;

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
            IEVisitorConfigService eVisitorConfigService)
        {
            _dialogService = dialogService;
            _autoLogonService = autoLogonService;
            _os = os;
            _updateService = updateService;
            _eVisitorConfigService = eVisitorConfigService;

            // Config laden
            _currentConfig = _eVisitorConfigService.LoadConfig();

            // Limits setzen
            ComputerRestartClockTimeMin = 0;
            ComputerRestartClockTimeMax = 23;
            ComputerRestartClockTime = _currentConfig.Computer.RestartClockTime;

            // ComboBox Vorbelegung (WICHTIG!)
            var configDays = _currentConfig.Browser.DeleteBrowserCacheIntervalDays;
            var configLanguageIndex = _currentConfig.Settings.Language;

            SelectedComputerRestartOption = ComputerRestartList.FirstOrDefault(x => x.Days == configDays) ?? ComputerRestartList[0];
            SelectedLanguageOption = LanguageList.FirstOrDefault(x => x.Index == configLanguageIndex) ?? LanguageList[0];

            // UI-Status initial berechnen
            //UpdateRestartUiState();

            // Async Initialisierung starten (Fire & Forget)
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

        partial void OnSelectedLanguageOptionChanged(LanguageOption value)
        {
            _currentConfig.Settings.Language = value.Index;
            SaveSettings();
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
