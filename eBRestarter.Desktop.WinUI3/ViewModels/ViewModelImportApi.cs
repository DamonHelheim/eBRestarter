using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelImportApi : ObservableObject
    {
        private readonly IApiAuthenticationService _authService;
        private readonly ICredentialStore _credentialStore;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
        public partial string ImportedFilePath { get; set; } =  string.Empty;
        [ObservableProperty] public partial string ImportedFileName { get; set; } = string.Empty; // Für die Anzeige (z.B. "config.apiaf")
        [ObservableProperty] public partial string FileStatusIcon { get; set; } = "ms-appx:///Resources/Visuals/Icons/LightTheme/note_light_theme.png"; // Default Icon
        [ObservableProperty] public partial bool IsBusy { get; set; }
        [ObservableProperty] public partial string StatusMessage { get; set; } = string.Empty;
        [ObservableProperty] public partial string StatusColor { get; set; } = "Transparent";

        public ViewModelImportApi(
            IApiAuthenticationService authService,
            ICredentialStore credentialStore)
        {
            _authService = authService;
            _credentialStore = credentialStore;
        }

        // Wird vom Drop-Event der View aufgerufen
        public void HandleFileDrop(string filePath)
        {
            if (!filePath.EndsWith(".apiaf"))
            {
                StatusMessage = "Ungültiges Dateiformat. Bitte .apiaf Datei verwenden.";
                StatusColor = "#E40E87"; // Rot
                FileStatusIcon = "ms-appx:///Resources/Visuals/Icons/Intersection/wrong_document.png";
                ImportedFilePath = "";
                ImportedFileName = "";
                return;
            }

            ImportedFilePath = filePath;
            ImportedFileName = System.IO.Path.GetFileName(filePath);
            StatusMessage = "Datei erkannt. Bereit zum Import.";
            StatusColor = "{ThemeResource TextFillColorPrimaryBrush}";
            FileStatusIcon = "ms-appx:///Resources/Visuals/Icons/Intersection/approval.png";
        }

        public void HandleFileSelect()
        {
            // Hinweis: FilePicker muss eigentlich in der View / Code-Behind aufgerufen werden, 
            // da er Window-Handle braucht. Das ViewModel verarbeitet dann nur das Ergebnis.
            // Wir lassen diese Methode hier leer oder delegieren an einen IFilePickerService.
        }

        private bool CanImport => !string.IsNullOrEmpty(ImportedFilePath) && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanImport))]
        private async Task Import()
        {
            IsBusy = true;
            StatusMessage = "Lese Datei und prüfe Zugangsdaten...";
            StatusColor = "{ThemeResource SystemFillColorCautionBrush}"; // Gelb

            // 1. Daten aus Datei lesen (Legacy Format)
            var credentials = _credentialStore.ImportFromLegacyFile(ImportedFilePath);

            if (credentials == null)
            {
                StatusMessage = "Fehler beim Lesen der Datei (Format ungültig).";
                StatusColor = "#E40E87";
                IsBusy = false;
                return;
            }

            // 2. Prüfen (API Call)
            var result = await _authService.VerifyCredentialsAsync(credentials.Username, credentials.ApiKey);

            IsBusy = false;

            if (result.IsValid)
            {
                // 3. Speichern (in den neuen Secure Store)
                _credentialStore.SaveCredentials(credentials);

                StatusMessage = "Import und Aktivierung erfolgreich!";
                StatusColor = "#7ED422"; // Grün
            }
            else
            {
                StatusMessage = $"Import fehlgeschlagen: {result.Message}";
                StatusColor = "#E40E87";
            }
        }
    }
}
