using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Models.Records.Config;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement; // Wichtig: Referenz hinzufügen!
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelOptions : ObservableObject
    {
        private readonly AppConfig _currentConfig;
        private readonly IEVisitorConfigService _eVisitorConfigService;

        private readonly IDialogService _dialogService;
        private readonly IWindowsAutoLogonService _autoLogonService;
        private readonly IOperatingSystemFacade _os;

        [ObservableProperty] public partial bool SetStartUp { get; set; } = false;

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
        }

        [RelayCommand]
        private void ExecuteToggleSwitchStartWithWindows(object value)
        {
            bool activateStartWithWindows = (bool)value;

            if (activateStartWithWindows is true) { 

                _os.WindowsStartupManagerService.EnableAutoStart();
            }
            else
            {
                _os.WindowsStartupManagerService.DisableAutoStart();
            }
        }

        partial void OnSetStartUpChanged(bool value)
        {
            if (value is true)
            {
                _os.WindowsStartupManagerService.EnableAutoStart();
                _currentConfig.Browser.StartBrowserWithProgrammStart = value;
                SaveSettings();
            }
            else
            {
                _os.WindowsStartupManagerService.DisableAutoStart();
                _currentConfig.Browser.StartBrowserWithProgrammStart = value;
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


        private void SaveSettings()
        {
            _eVisitorConfigService.SaveConfig(_currentConfig);
        }
    }
}
