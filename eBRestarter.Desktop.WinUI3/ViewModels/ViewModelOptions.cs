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
    public partial class ViewModelOptions : ObservableObject
    {

        private bool _isInitializing = false; // Sperre flag

        private readonly AppConfig _currentConfig;
        private readonly IEVisitorConfigService _eVisitorConfigService;

        private readonly IDialogService _dialogService;
        private readonly IWindowsAutoLogonService _autoLogonService;
        private readonly IOperatingSystemFacade _os;

        public ReadOnlyCollection<ComputerRestartOption> ComputerRestartList => ComputerRestartConstants.Options;

        public ReadOnlyCollection<LanguageOption> LanguageList => LanguageSelectionConstants.Options;

        [ObservableProperty] public partial ComputerRestartOption SelectedComputerRestartOption { get; set; }

        [ObservableProperty] public partial LanguageOption SelectedLanguageOption { get; set; }

        partial void OnSelectedLanguageOptionChanged(LanguageOption value)
        {
            _currentConfig.Settings.Language = value.Index;

            SaveSettings();
        }

        [ObservableProperty] public partial int ComputerRestartClockTime { get; set; }
        [ObservableProperty] public partial bool StartWithWindows { get; set; } = false;

        // NEUE PROPERTIES FÜR DIE UI
        [ObservableProperty]
        public partial bool IsRestartSliderVisible { get; set; }

        [ObservableProperty]
        public partial string RestartStatusText { get; set; } = "";

        public int ComputerRestartClockTimeMin { get; init; }
        public int ComputerRestartClockTimeMax { get; init; }



        public ViewModelOptions(
            IDialogService dialogService,
            IWindowsAutoLogonService autoLogonService,
            IOperatingSystemFacade os,
            IEVisitorConfigService eVisitorConfigService)
        {
            _dialogService = dialogService;
            _autoLogonService = autoLogonService;
            _os = os;
            _eVisitorConfigService = eVisitorConfigService;
            _currentConfig = _eVisitorConfigService.LoadConfig();

            ComputerRestartClockTimeMin = 0;
            ComputerRestartClockTimeMax = 23;
            ComputerRestartClockTime = _currentConfig.Computer.RestartClockTime;

            // ComboBox (WICHTIG!)
            // Wir suchen den Eintrag in der Liste, der den Tagen aus der Config entspricht.
            // Fallback auf Index 0, falls nichts gefunden (z.B. bei neuer Config).
            var configDays = _currentConfig.Browser.DeleteBrowserCacheIntervalDays;
            var configLanguageIndex = _currentConfig.Settings.Language;

            SelectedComputerRestartOption = ComputerRestartList.FirstOrDefault(x => x.Days == configDays) ?? ComputerRestartList[0];

            SelectedLanguageOption = LanguageList.FirstOrDefault(x => x.Index == configLanguageIndex) ?? LanguageList[0];

            _ = InitializeAsync();
        }

        // --- NEUE LOGIK-METHODE ---
        private void UpdateRestartUiState()
        {
            int days = SelectedComputerRestartOption?.Days ?? 0;

            // 1. Sichtbarkeit des Sliders steuern
            IsRestartSliderVisible = days > 0;

            // 2. Text generieren
            if (days == 0)
            {
                RestartStatusText = "Computer wird nicht neugestartet";
            }
            else
            {
                // HIER DIE KORREKTUR:
                // Wir holen das Datum. Ist es 'null', nutzen wir MinValue als Platzhalter.
                DateTime targetDate = _currentConfig.Computer.NextRestartDate ?? DateTime.MinValue;

                // Wenn es MinValue ist (weil es null war oder noch nicht gesetzt),
                // berechnen wir es hier "on the fly" für die Anzeige.
                if (targetDate == DateTime.MinValue)
                {
                    targetDate = DateTime.Today.AddDays(days).AddHours(ComputerRestartClockTime);
                }

                // Text formatieren
                RestartStatusText = $"Computer wird am {targetDate:dd.MM.yyyy} um {targetDate:HH} Uhr neugestartet";
            }
        }

        // Wenn sich die Auswahl ändert, kannst du hier reagieren
        partial void OnSelectedComputerRestartOptionChanged(ComputerRestartOption value)
        {
            _currentConfig.Computer.ComputerRestartIntervalDays = value.Days;

            RecalculateNextRestartDate(); // Berechnet das Datum in der Config
            UpdateRestartUiState();       // <--- NEU: Aktualisiert Text & Sichtbarkeit für UI

            SaveSettings();
        }

        partial void OnComputerRestartClockTimeChanged(int value)
        {
            // 1. Clamping
            int clampedValue = Math.Clamp(value, ComputerRestartClockTimeMin, ComputerRestartClockTimeMax);

            // 2. Auto-Korrektur in der UI
            if (value != clampedValue)
            {
                // Das setzt die Property neu. 
                // WICHTIG: Da es eine partial Property ist, funktioniert der Setter hier rekursiv sicher.
                ComputerRestartClockTime = clampedValue;
                return;
            }

            // 3. Speichern
            if (_currentConfig.Computer.RestartClockTime != value)
            {
                _currentConfig.Computer.RestartClockTime = value;

                RecalculateNextRestartDate(); // Berechnet das Datum in der Config
                UpdateRestartUiState();       // <--- NEU: Aktualisiert Text

                SaveSettings();
            }
        }

        private async Task InitializeAsync()
        {
            _isInitializing = true; // Sperre aktivieren

            try
            {
                StartWithWindows = await _os.WindowsStartupManagerService.IsAutoStartEnabledAsync();

                if (_currentConfig.Settings.StartWithWindows is true && StartWithWindows is false)
                {
                    await _os.WindowsStartupManagerService.EnableAutoStartAsync();
                }
            }
            finally
            {
                _isInitializing = false; // Sperre aufheben
            }
        }


        partial void OnStartWithWindowsChanged(bool value)
        {
            if (_isInitializing) return; // Abbrechen, wenn wir nur den Startwert laden

            // Jetzt wirklich ändern
            ToggleAutoStartAsync(value);
        }

        private async void ToggleAutoStartAsync(bool enable)
        {
            if (enable) { 

                await _os.WindowsStartupManagerService.EnableAutoStartAsync();
                _currentConfig.Settings.StartWithWindows = enable;
                SaveSettings();
            }
            else
            {
                await _os.WindowsStartupManagerService.DisableAutoStartAsync();
                _currentConfig.Settings.StartWithWindows = enable;
                SaveSettings();
            }
        }

        [RelayCommand]
        private async Task ConfigureAutoLogon()
        {
            // 1. Aktuellen Windows-Nutzer auslesen
            string currentUser = Environment.UserName;
            string currentDomain = Environment.UserDomainName;

            // 2. Dialog anzeigen und Defaults übergeben
            // Hinweis: Du musst deine Methode ShowAutoLogonDialogAsync so anpassen, 
            // dass sie diese Parameter akzeptiert und ins Textfeld schreibt.
            var dialogResult = await _dialogService.ShowAutoLogonDialogAsync(currentUser, currentDomain);

            // Wenn null, wurde abgebrochen -> Nichts tun
            if (dialogResult == null) return;

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

                    // --- NEU: Validierung der Zugangsdaten ---
                    bool isValid = ValidateCredentials(user, domain, pass);

                    if (!isValid)
                    {
                        await _dialogService.ShowMessageAsync("Fehler", "Benutzername oder Passwort sind nicht korrekt. Bitte prüfen Sie die Eingaben.");
                        return; // Abbruch, nicht speichern
                    }
                    // -----------------------------------------

                    _autoLogonService.EnableAutoLogon(user, domain, pass);
                    await _dialogService.ShowMessageAsync("Erfolg", "Die automatische Anmeldung wurde eingerichtet.");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync("Fehler", $"Ein unerwarteter Fehler ist aufgetreten: {ex.Message}");
            }
        }

        // Hilfsmethode zur Überprüfung der Anmeldedaten
        private bool ValidateCredentials(string username, string domain, string password)
        {
            try
            {
                // Entscheidung: Ist es ein Domain-Account oder ein lokaler Account?
                ContextType contextType = ContextType.Machine;

                // Wenn die angegebene Domain ungleich dem Computernamen ist, versuchen wir Domain-Auth
                if (!string.Equals(domain, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
                {
                    contextType = ContextType.Domain;
                }

                // PrincipalContext erstellen und validieren
                using (var context = new PrincipalContext(contextType, domain))
                {
                    return context.ValidateCredentials(username, password);
                }
            }
            catch (PrincipalServerDownException)
            {
                // Fallback: Wenn Domain-Controller nicht erreichbar, kann man evtl. nicht prüfen.
                // Hier entscheiden: Trotzdem erlauben oder Fehler werfen?
                throw new Exception("Der Domänen-Controller konnte zur Überprüfung nicht erreicht werden.");
            }
            catch (Exception)
            {
                // Bei anderen Fehlern (z.B. Domain existiert nicht) ist die Validierung fehlgeschlagen
                return false;
            }
        }

        // Diese Methode rufst du in OnComputerRestartClockTimeChanged UND 
        // in OnSelectedComputerRestartOptionChanged auf.
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
        private void SaveSettings()
        {
            _eVisitorConfigService.SaveConfig(_currentConfig);
        }
    }
}
