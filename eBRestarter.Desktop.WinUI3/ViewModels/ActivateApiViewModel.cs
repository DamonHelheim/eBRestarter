using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ActivateApiViewModel : ObservableObject
    {
        private readonly IApiAuthenticationService _authService;
        private readonly ICredentialStore _credentialStore;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        public partial string Username { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        public partial string ApiKey { get; set; } = string.Empty;

        // UI Status
        [ObservableProperty] public partial bool IsBusy { get; set; }
        [ObservableProperty] public partial string StatusMessage { get; set; } = string.Empty;
        [ObservableProperty] public partial string StatusColor { get; set; } = "Transparent"; // Hex Code oder Resource Key

        public ActivateApiViewModel(
            IApiAuthenticationService authService,
            ICredentialStore credentialStore)
        {
            _authService = authService;
            _credentialStore = credentialStore;

            // Bestehende Daten laden
            var saved = _credentialStore.LoadCredentials();
            if (saved != null)
            {
                Username = saved.Username;
                ApiKey = saved.ApiKey;
            }
        }

        private bool CanSubmit => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(ApiKey) && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task Submit()
        {
            IsBusy = true;
            StatusMessage = "Prüfe Zugangsdaten...";
            StatusColor = "{ThemeResource SystemFillColorCautionBrush}"; // Gelb/Orange

            var result = await _authService.VerifyCredentialsAsync(Username, ApiKey);

            IsBusy = false;

            if (result.IsValid)
            {
                // Speichern
                _credentialStore.SaveCredentials(new ApiCredentials(Username, ApiKey));

                StatusMessage = "Erfolgreich aktiviert!";
                StatusColor = "#7ED422"; // Grün
            }
            else
            {
                StatusMessage = result.Message;
                StatusColor = "#E40E87"; // Rot
            }
        }

        // Import-Logik (Legacy File Drag&Drop)
        public void ImportLegacyFile(string filePath)
        {
            var imported = _credentialStore.ImportFromLegacyFile(filePath);
            if (imported != null)
            {
                Username = imported.Username;
                ApiKey = imported.ApiKey;
                StatusMessage = "Daten aus Datei importiert. Bitte 'Aktivieren' klicken.";
                StatusColor = "{ThemeResource SystemFillColorSuccessBrush}";
            }
            else
            {
                StatusMessage = "Fehler beim Lesen der Datei.";
                StatusColor = "#E40E87";
            }
        }
    }
}
