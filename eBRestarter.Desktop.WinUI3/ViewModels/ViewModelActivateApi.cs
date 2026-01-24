using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelActivateApi : ObservableObject
    {
        #region Fields (Private Felder OHNE [ObservableProperty])

        private readonly IApiAuthenticationService _authService;
        private readonly IEVisitorConfigService _configService;

        #endregion

        #region Observable Properties (Felder MIT [ObservableProperty])

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        public partial string ApiKey { get; set; } = string.Empty;
        [ObservableProperty] public partial bool IsBusy { get; set; }
        [ObservableProperty] public partial string StatusColor { get; set; } = "Transparent"; // Hex Code oder Resource Key
        [ObservableProperty] public partial string StatusMessage { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        public partial string Username { get; set; } = string.Empty;

        #endregion

        #region Properties (Explizite get; set; Eigenschaften)

        private bool CanSubmit => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(ApiKey) && !IsBusy;

        #endregion

        #region Constructors

        public ViewModelActivateApi(
            IApiAuthenticationService authService,
            IEVisitorConfigService configService)
        {
            _authService = authService;
            _configService = configService;

            // Bestehende Daten laden
            // LoadConfig entschlüsselt automatisch, wir bekommen also Klartext für die UI
            var config = _configService.LoadConfig();

            Username = config.Settings.ApiUsername;
            ApiKey = config.Settings.ApiKey;
        }

        #endregion

        #region Commands (Methoden MIT [RelayCommand])

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task Submit()
        {
            IsBusy = true;
            StatusMessage = "Prüfe Zugangsdaten...";

            var (IsValid, Message) = await _authService.VerifyCredentialsAsync(Username, ApiKey);

            IsBusy = false;

            if (IsValid)
            {
                // 1. Config laden (um den aktuellen Stand zu haben)
                var currentConfig = _configService.LoadConfig();

                // 2. Daten aktualisieren (wir nutzen 'with' um Records zu kopieren/ändern)
                var newConfig = currentConfig with
                {
                    Settings = currentConfig.Settings with
                    {
                        ApiUsername = Username,
                        ApiKey = ApiKey // Hier noch Klartext
                    }
                };

                // 3. Speichern (ConfigService übernimmt die Verschlüsselung intern)
                _configService.SaveConfig(newConfig);

                StatusMessage = "Erfolgreich aktiviert & verschlüsselt gespeichert!";
                StatusColor = "#7ED422";
            }
            else
            {
                StatusMessage = Message;
                StatusColor = "#E40E87";
            }
        }      

        #endregion

        #region Methods (Restliche Methoden)

        // Import-Logik (Legacy File Drag&Drop)
        public void ImportLegacyFile(string filePath)
        {
            // Check ob Datei existiert
            if (!System.IO.File.Exists(filePath))
            {
                StatusMessage = "Datei nicht gefunden.";
                StatusColor = "#E40E87"; // Rot
                return;
            }

            try
            {
                // Da der CredentialStore wegfällt, holen wir die Lese-Logik (BinaryReader) hier rein.
                // Wir öffnen die Datei nur lesend.
                using var stream = System.IO.File.Open(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                using var reader = new System.IO.BinaryReader(stream);

                // Die alte Struktur war: String Username, String ApiKey
                var importedUser = reader.ReadString();
                var importedKey = reader.ReadString();

                // 1. Daten in die UI-Properties laden
                Username = importedUser;
                ApiKey = importedKey;

                // 2. Status setzen
                // WICHTIG: Wir speichern noch NICHT. Der User soll auf "Aktivieren" klicken,
                // damit dein neuer Submit-Command die Validierung und Verschlüsselung macht.
                StatusMessage = "Daten importiert. Bitte jetzt 'Aktivieren' klicken.";
                StatusColor = "{ThemeResource SystemFillColorSuccessBrush}"; // Grün
            }
            catch (Exception)
            {
                StatusMessage = "Fehler: Die Datei hat ein falsches Format.";
                StatusColor = "#E40E87"; // Rot

                // Optional: Felder leeren bei Fehler
                Username = string.Empty;
                ApiKey = string.Empty;
            }
        }

        #endregion
    }
}
