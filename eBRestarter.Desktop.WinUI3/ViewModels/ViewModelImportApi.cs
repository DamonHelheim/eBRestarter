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
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IApiAuthenticationService _authService;
        private readonly ICredentialStore _credentialStore;

        #endregion

        // =========================================================
        // 2. OBSERVABLE PROPERTIES (MVVM State)
        // =========================================================
        #region ObservableProperties

        [ObservableProperty] public partial string FileStatusIcon { get; set; } = "ms-appx:///Resources/Visuals/Icons/LightTheme/note_light_theme.png";
        [ObservableProperty] public partial string ImportedFileName { get; set; } = string.Empty;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
        public partial string ImportedFilePath { get; set; } = string.Empty;
        [ObservableProperty] public partial bool IsBusy { get; set; }
        [ObservableProperty] public partial string StatusColor { get; set; } = "Transparent";
        [ObservableProperty] public partial string StatusMessage { get; set; } = string.Empty;

        #endregion

        // =========================================================
        // 3. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        public ViewModelImportApi(
            IApiAuthenticationService authService,
            ICredentialStore credentialStore)
        {
            _authService = authService;
            _credentialStore = credentialStore;
        }

        #endregion

        // =========================================================
        // 4. COMMANDS (MVVM Actions)
        // =========================================================
        #region Commands

        [RelayCommand(CanExecute = nameof(CanImport))]
        private async Task Import()
        {
            IsBusy = true;
            StatusMessage = "Lese Datei und prüfe Zugangsdaten...";
            StatusColor = "{ThemeResource SystemFillColorCautionBrush}";

            var credentials = _credentialStore.ImportFromLegacyFile(ImportedFilePath);
            if (credentials == null)
            {
                StatusMessage = "Fehler beim Lesen der Datei (Format ungültig).";
                StatusColor = "#E40E87";
                IsBusy = false;
                return;
            }

            var result = await _authService.VerifyCredentialsAsync(credentials.Username, credentials.ApiKey);
            IsBusy = false;

            if (result.IsValid)
            {
                _credentialStore.SaveCredentials(credentials);
                StatusMessage = "Import und Aktivierung erfolgreich!";
                StatusColor = "#7ED422";
            }
            else
            {
                StatusMessage = $"Import fehlgeschlagen: {result.Message}";
                StatusColor = "#E40E87";
            }
        }

        #endregion

        // =========================================================
        // 5. PUBLIC & PROTECTED METHODS (API)
        // =========================================================
        #region PublicAndProtectedMethods

        public void HandleFileDrop(string filePath)
        {
            if (!filePath.EndsWith(".apiaf"))
            {
                StatusMessage = "Ungültiges Dateiformat. Bitte .apiaf Datei verwenden.";
                StatusColor = "#E40E87";
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
        }

        #endregion

        // =========================================================
        // 6. PRIVATE HELPER METHODS (Interne Hilfsmethoden)
        // =========================================================
        #region PrivateHelperMethods

        private bool CanImport => !string.IsNullOrEmpty(ImportedFilePath) && !IsBusy;

        #endregion
    }
}
