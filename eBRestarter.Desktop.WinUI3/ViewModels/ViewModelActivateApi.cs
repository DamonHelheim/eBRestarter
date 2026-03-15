using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Core.Application.Interfaces.Config;
using System;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    /// <summary>
    /// View model for the "Activate API" dialog. Lets the user enter or import eBesucher API
    /// credentials; validates them via <see cref="IApiAuthenticationService"/> and persists
    /// to config when valid.
    /// </summary>
    public partial class ViewModelActivateApi : ObservableObject
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IApiAuthenticationService _authService;
        private readonly IEVisitorConfigService _configService;
        private readonly ILocalizationService _localizationService;

        #endregion

        // =========================================================
        // 2. OBSERVABLE PROPERTIES (MVVM State)
        // =========================================================
        #region ObservableProperties

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

        // =========================================================
        // 3. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        /// <summary>
        /// Builds the VM with auth, config, and localization services and pre-fills
        /// <see cref="Username"/> and <see cref="ApiKey"/> from saved config if present.
        /// </summary>
        public ViewModelActivateApi(
            IApiAuthenticationService authService,
            IEVisitorConfigService configService,
            ILocalizationService localizationService)
        {
            _authService = authService;
            _configService = configService;
            _localizationService = localizationService;

            var config = _configService.LoadConfig();
            Username = config.Settings.ApiUsername;
            ApiKey = config.Settings.ApiKey;
        }

        #endregion

        // =========================================================
        // 4. COMMANDS (MVVM Actions)
        // =========================================================
        #region Commands

        /// <summary>
        /// Verifies the current <see cref="Username"/> and <see cref="ApiKey"/> with the API.
        /// On success, saves them to config and sets a green success message; on failure, sets
        /// a red status message without changing config.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task Submit()
        {
            IsBusy = true;
            StatusMessage = _localizationService.GetString("ActivateApi_Checking");

            var (IsValid, Message) = await _authService.VerifyCredentialsAsync(Username, ApiKey);

            IsBusy = false;

            if (IsValid)
            {
                var currentConfig = _configService.LoadConfig();
                var newConfig = currentConfig with
                {
                    Settings = currentConfig.Settings with { ApiUsername = Username, ApiKey = ApiKey }
                };
                _configService.SaveConfig(newConfig);
                StatusMessage = _localizationService.GetString("ActivateApi_Success");
                StatusColor = "#7ED422";
            }
            else
            {
                StatusMessage = Message;
                StatusColor = "#E40E87";
            }
        }

        #endregion

        // =========================================================
        // 5. PUBLIC & PROTECTED METHODS (API)
        // =========================================================
        #region PublicAndProtectedMethods

        /// <summary>
        /// Imports username and API key from a legacy binary file (e.g. old eBesucher format).
        /// Expects two length-prefixed strings. On success, sets Username and ApiKey and a success message;
        /// on missing file or read error, sets an error message and clears credentials.
        /// </summary>
        /// <param name="filePath">Full path to the import file. If null or missing, sets file-not-found message.</param>
        public void ImportLegacyFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                StatusMessage = _localizationService.GetString("ActivateApi_FileNotFound");
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
                StatusMessage = _localizationService.GetString("ActivateApi_ImportSuccess");
                StatusColor = "{ThemeResource SystemFillColorSuccessBrush}";
            }
            catch (Exception)
            {
                StatusMessage = _localizationService.GetString("ActivateApi_ImportError");
                StatusColor = "#E40E87";
                Username = string.Empty;
                ApiKey = string.Empty;
            }
        }

        #endregion

        // =========================================================
        // 6. PRIVATE HELPER METHODS (Interne Hilfsmethoden)
        // =========================================================
        #region PrivateHelperMethods

        /// <summary>Submit is allowed only when both username and API key are non-empty and the VM is not busy.</summary>
        private bool CanSubmit => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(ApiKey) && !IsBusy;

        #endregion
    }
}
