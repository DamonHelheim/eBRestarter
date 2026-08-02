using System;
using System.IO;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Authentication;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.SignalDTO.Messages;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the "Activate API" dialog. Lets the user enter or import eBesucher API
/// credentials; validates them via <see cref="IOutboundPortApiAuthenticationProvider"/> and persists
/// to config when valid.
/// </summary>
public sealed partial class ViewModelActivateApi : ObservableObject
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const string HexColorError = "#E40E87";
    private const string HexColorSuccess = "#7ED422";
    private const string KeyActivateApiChecking = "ActivateApi_Checking";
    private const string KeyActivateApiFileNotFound = "ActivateApi_FileNotFound";
    private const string KeyActivateApiImportError = "ActivateApi_ImportError";
    private const string KeyActivateApiImportSuccess = "ActivateApi_ImportSuccess";
    private const string KeyActivateApiSuccess = "ActivateApi_Success";
    private const string SystemColorSuccessBrush = "{ThemeResource SystemFillColorSuccessBrush}";
    private const string TransparentColor = "Transparent";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch A–Z) ──
    private readonly IOutboundPortApiAuthenticationProvider _apiAuthenticationService;
    private readonly IOutboundPortEVisitorConfigRepository _configService;
    private readonly IInboundPortLocalizationProvider _localizationService;


    // ═══════════════════════════════════════════════════════
    //  3. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Builds the VM with API authentication, config, and localization services and pre-fills
    /// <see cref="Username"/> and <see cref="ApiKey"/> from saved config if present.
    /// </summary>
    public ViewModelActivateApi(
        IOutboundPortApiAuthenticationProvider apiAuthenticationService,
        IOutboundPortEVisitorConfigRepository configService,
        IInboundPortLocalizationProvider localizationService)
    {
        ArgumentNullException.ThrowIfNull(apiAuthenticationService);
        ArgumentNullException.ThrowIfNull(configService);
        ArgumentNullException.ThrowIfNull(localizationService);

        _apiAuthenticationService = apiAuthenticationService;
        _configService = configService;
        _localizationService = localizationService;

        var config = _configService.LoadConfig();
        Username = config?.Settings?.ApiUsername ?? string.Empty;
        ApiKey = config?.Settings?.ApiKey ?? string.Empty;
    }


    // ═══════════════════════════════════════════════════════
    //  6. Properties
    // ═══════════════════════════════════════════════════════
    /// <summary>Gets or sets the eBesucher API key.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial string ApiKey { get; set; } = string.Empty;

    /// <summary>Submit is allowed only when both username and API key are non-empty and the VM is not busy.</summary>
    private bool CanSubmit => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(ApiKey) && !IsBusy;

    /// <summary>Gets or sets a value indicating whether an async operation is currently executing.</summary>
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    /// <summary>Gets or sets the status message text color or brush.</summary>
    [ObservableProperty]
    public partial string StatusColor { get; set; } = TransparentColor;

    /// <summary>Gets or sets the status or feedback message displayed to the user.</summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    /// <summary>Gets or sets the eBesucher username.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial string Username { get; set; } = string.Empty;


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Imports username and API key from a legacy binary file (e.g. old eBesucher format).
    /// Expects two length-prefixed strings. On success, sets Username and ApiKey and a success message;
    /// on missing file or read error, sets an error message and clears credentials.
    /// </summary>
    /// <param name="filePath">Full path to the import file. If null or missing, sets file-not-found message.</param>
    public void ImportLegacyFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            StatusMessage = _localizationService.RetrieveString(KeyActivateApiFileNotFound);
            StatusColor = HexColorError;
            return;
        }

        try
        {
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(stream);
            var importedUser = reader.ReadString();
            var importedKey = reader.ReadString();
            Username = importedUser;
            ApiKey = importedKey;
            StatusMessage = _localizationService.RetrieveString(KeyActivateApiImportSuccess);
            StatusColor = SystemColorSuccessBrush;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ObjectDisposedException)
        {
            StatusMessage = _localizationService.RetrieveString(KeyActivateApiImportError);
            StatusColor = HexColorError;
            Username = string.Empty;
            ApiKey = string.Empty;
        }
    }

    /// <summary>
    /// Verifies the current <see cref="Username"/> and <see cref="ApiKey"/> with the API.
    /// On success, saves them to config and sets a green success message; on failure, sets
    /// a red status message without changing config.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        IsBusy = true;
        StatusMessage = _localizationService.RetrieveString(KeyActivateApiChecking);

        try
        {
            var (isValid, verificationDetail) = await _apiAuthenticationService.VerifyCredentialsAsync(
                Username ?? string.Empty,
                ApiKey ?? string.Empty);

            if (!isValid)
            {
                StatusMessage = verificationDetail ?? string.Empty;
                StatusColor = HexColorError;
                return;
            }

            var currentConfig = _configService.LoadConfig();
            if (currentConfig?.Settings != null)
            {
                currentConfig.Settings.ApiUsername = Username ?? string.Empty;
                currentConfig.Settings.ApiKey = ApiKey ?? string.Empty;
                _configService.SaveConfig(currentConfig);
            }

            StatusMessage = _localizationService.RetrieveString(KeyActivateApiSuccess);
            StatusColor = HexColorSuccess;

            WeakReferenceMessenger.Default.Send(new ApiCredentialsUpdatedMessage());
        }
        finally
        {
            IsBusy = false;
        }
    }
}
