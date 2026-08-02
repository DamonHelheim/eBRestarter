using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the Infocenter / system info page. Loads hardware and OS information via
/// <see cref="IInboundPortSystemInformationProvider"/> and exposes localized text for display.
/// Also provides commands to open the support website and the About dialog.
/// </summary>
public sealed partial class ViewModelInfocenter : ObservableObject
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const string InfocenterBrowserPrefixResourceKey = "Infocenter_BrowserPrefix";
    private const string InfocenterBuildPrefixResourceKey = "Infocenter_BuildPrefix";
    private const string InfocenterEditionPrefixResourceKey = "Infocenter_EditionPrefix";
    private const string InfocenterGraphicsPrefixResourceKey = "Infocenter_GraphicsPrefix";
    private const string InfocenterLoadFailedResourceKey = "Infocenter_LoadFailed";
    private const string InfocenterLoadingResourceKey = "Infocenter_Loading";
    private const string InfocenterProcessorPrefixResourceKey = "Infocenter_ProcessorPrefix";
    private const string InfocenterRamPrefixResourceKey = "Infocenter_RamPrefix";
    private const string InfocenterVersionPrefixResourceKey = "Infocenter_VersionPrefix";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    private readonly IDialogService _dialogService;
    private readonly IInboundPortLocalizationProvider _localizationService;
    private readonly IInboundPortSystemInformationProvider _systemInformationProvider;

    // ═══════════════════════════════════════════════════════
    //  3. Observable Properties
    // ═══════════════════════════════════════════════════════
    [ObservableProperty] public partial string BrowserText { get; set; } = string.Empty;
    [ObservableProperty] public partial string GraphicsText { get; set; } = string.Empty;
    [ObservableProperty] public partial string OsBuildText { get; set; } = string.Empty;
    [ObservableProperty] public partial string OsEditionText { get; set; } = string.Empty;
    [ObservableProperty] public partial string OsVersionText { get; set; } = string.Empty;
    [ObservableProperty] public partial string ProcessorText { get; set; } = string.Empty;
    [ObservableProperty] public partial string RamText { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes the VM with the system-information use case and dialog service; sets all info
    /// fields to a loading placeholder and starts async load so the page shows data as it becomes available.
    /// </summary>
    public ViewModelInfocenter(
        IDialogService dialogService,
        IInboundPortLocalizationProvider localizationService,
        IInboundPortSystemInformationProvider systemInformationProvider)
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(systemInformationProvider);

        _dialogService = dialogService;
        _localizationService = localizationService;
        _systemInformationProvider = systemInformationProvider;

        string loadingPlaceholder = _localizationService.RetrieveString(InfocenterLoadingResourceKey);

        BrowserText = loadingPlaceholder;
        GraphicsText = loadingPlaceholder;
        OsBuildText = loadingPlaceholder;
        OsEditionText = loadingPlaceholder;
        OsVersionText = loadingPlaceholder;
        ProcessorText = loadingPlaceholder;
        RamText = loadingPlaceholder;

        _ = LoadDataAsync();
    }

    // ═══════════════════════════════════════════════════════
    //  7. Commands
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Opens the given URL in the default browser via the shell. No-op if url is null or whitespace.
    /// </summary>
    /// <param name="url">Full URL to open (e.g. support or registration). If null or empty, nothing happens.</param>
    [RelayCommand]
    public static void OpenSupportWebsite(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    /// <summary>Opens the About dialog (version and credits) via the dialog service.</summary>
    [RelayCommand]
    private Task ShowAboutInfoAsync() => _dialogService.ShowAboutDialogAsync();

    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>Loads hardware and OS info from services and assigns localized strings to the observable properties.</summary>
    private async Task LoadDataAsync()
    {
        try
        {
            var systemInformation = await _systemInformationProvider.RetrieveAsync();

            ProcessorText = $"{_localizationService.RetrieveString(InfocenterProcessorPrefixResourceKey)} {systemInformation.ProcessorName}";
            GraphicsText = $"{_localizationService.RetrieveString(InfocenterGraphicsPrefixResourceKey)} {systemInformation.GraphicsCardName}";
            RamText = $"{_localizationService.RetrieveString(InfocenterRamPrefixResourceKey)} {systemInformation.InstalledRam}";
            OsEditionText = $"{_localizationService.RetrieveString(InfocenterEditionPrefixResourceKey)} {systemInformation.OsEdition}";
            OsVersionText = $"{_localizationService.RetrieveString(InfocenterVersionPrefixResourceKey)} {systemInformation.OsDisplayVersion}";
            OsBuildText = $"{_localizationService.RetrieveString(InfocenterBuildPrefixResourceKey)} {systemInformation.OsBuildVersion}";
            BrowserText = $"{_localizationService.RetrieveString(InfocenterBrowserPrefixResourceKey)} {systemInformation.StandardBrowserName}";
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);

            string failureMessage = _localizationService.RetrieveString(InfocenterLoadFailedResourceKey);

            BrowserText = failureMessage;
            GraphicsText = failureMessage;
            OsBuildText = failureMessage;
            OsEditionText = failureMessage;
            OsVersionText = failureMessage;
            ProcessorText = failureMessage;
            RamText = failureMessage;
        }
    }
}
