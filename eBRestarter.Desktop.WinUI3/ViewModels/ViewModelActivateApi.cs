using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
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
        #region Fields

        private readonly IApiAuthenticationService _authService;
        private readonly IEVisitorConfigService _configService;
        private readonly ILocalizationService _localizationService; // <--- NEU: Service Feld

        #endregion

        #region Observable Properties

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        public partial string ApiKey { get; set; } = string.Empty;

        [ObservableProperty] public partial bool IsBusy { get; set; }
        [ObservableProperty] public partial string StatusColor { get; set; } = "Transparent";
        [ObservableProperty] public partial string StatusMessage { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        public partial string Username { get; set; } = string.Empty;

        #endregion

        #region Properties

        private bool CanSubmit => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(ApiKey) && !IsBusy;

        #endregion

        #region Constructors

        public ViewModelActivateApi(
            IApiAuthenticationService authService,
            IEVisitorConfigService configService,
            ILocalizationService localizationService) // <--- Injizieren
        {
            _authService = authService;
            _configService = configService;
            _localizationService = localizationService; // <--- Zuweisen

            // Bestehende Daten laden
            var config = _configService.LoadConfig();

            Username = config.Settings.ApiUsername;
            ApiKey = config.Settings.ApiKey;
        }

        #endregion

        #region Commands

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task Submit()
        {
            IsBusy = true;
            StatusMessage = _localizationService.GetString("ActivateApi_Checking"); // "Prüfe Zugangsdaten..."

            var (IsValid, Message) = await _authService.VerifyCredentialsAsync(Username, ApiKey);

            IsBusy = false;

            if (IsValid)
            {
                var currentConfig = _configService.LoadConfig();

                var newConfig = currentConfig with
                {
                    Settings = currentConfig.Settings with
                    {
                        ApiUsername = Username,
                        ApiKey = ApiKey
                    }
                };

                _configService.SaveConfig(newConfig);

                StatusMessage = _localizationService.GetString("ActivateApi_Success"); // "Erfolgreich aktiviert..."
                StatusColor = "#7ED422";
            }
            else
            {
                // Message kommt vom Service (API), ggf. dort auch lokalisieren oder hier mappen
                StatusMessage = Message;
                StatusColor = "#E40E87";
            }
        }

        #endregion

        #region Methods

        public void ImportLegacyFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                StatusMessage = _localizationService.GetString("ActivateApi_FileNotFound"); // "Datei nicht gefunden."
                StatusColor = "#E40E87";
                return;
            }

            try
            {
                using var stream = System.IO.File.Open(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                using var reader = new System.IO.BinaryReader(stream);

                var importedUser = reader.ReadString();
                var importedKey = reader.ReadString();

                Username = importedUser;
                ApiKey = importedKey;

                StatusMessage = _localizationService.GetString("ActivateApi_ImportSuccess"); // "Daten importiert..."
                StatusColor = "{ThemeResource SystemFillColorSuccessBrush}";
            }
            catch (Exception)
            {
                StatusMessage = _localizationService.GetString("ActivateApi_ImportError"); // "Fehler: Format..."
                StatusColor = "#E40E87";

                Username = string.Empty;
                ApiKey = string.Empty;
            }
        }

        #endregion
    }
}