using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the Infocenter / system info page. Loads hardware and OS information via
/// <see cref="IInboundPortSystemInformationProvider"/> and exposes localized text for display.
/// Also provides commands to open the support website and the About dialog.
/// </summary>
public sealed partial class ViewModelInfocenter : ObservableObject
{
    private const string InfocenterLoadFailedResourceKey = "Infocenter_LoadFailed";

    private const string InfocenterLoadingResourceKey = "Infocenter_Loading";

    private readonly IDialogService _dialogService;

    private readonly IInboundPortSystemInformationProvider _systemInformationProvider;

    private readonly IInboundPortLocalizationProvider _localizationService;

    [ObservableProperty]
    public partial string BrowserText { get; set; }

    [ObservableProperty]
    public partial string GraphicsText { get; set; }

    [ObservableProperty]
    public partial string OsBuildText { get; set; }

    [ObservableProperty]
    public partial string OsEditionText { get; set; }

    [ObservableProperty]
    public partial string OsVersionText { get; set; }

    [ObservableProperty]
    public partial string ProcessorText { get; set; }

    [ObservableProperty]
    public partial string RamText { get; set; }

    /// <summary>
    /// Initializes the VM with the system-information use case and dialog service; sets all info
    /// fields to a loading placeholder and starts async load so the page shows data as it becomes available.
    /// </summary>
    public ViewModelInfocenter(
        IInboundPortSystemInformationProvider systemInformationProvider,
        IDialogService dialogService,
        IInboundPortLocalizationProvider LocalizationProvider)
    {
        ArgumentNullException.ThrowIfNull(systemInformationProvider);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(LocalizationProvider);

        _systemInformationProvider = systemInformationProvider;
        _dialogService = dialogService;
        _localizationService = LocalizationProvider;

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

    /// <summary>
    /// Opens the given URL in the default browser via the shell. No-op if url is null or whitespace.
    /// </summary>
    /// <param name="url">Full URL to open (e.g. support or registration). If null or empty, nothing happens.</param>
    [RelayCommand]
    public static void OpenSupportWebsite(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    /// <summary>Opens the About dialog (version and credits) via the dialog service.</summary>
    [RelayCommand]
    private async Task ShowAboutInfo()
    {
        await _dialogService.ShowAboutDialogAsync();
    }

    /// <summary>Loads hardware and OS info from services and assigns localized strings to the observable properties.</summary>
    private async Task LoadDataAsync()
    {
        try
        {
            var systemInformation = await _systemInformationProvider.RetrieveAsync();

            ProcessorText = $"{_localizationService.RetrieveString("Infocenter_ProcessorPrefix")} {systemInformation.ProcessorName}";
            GraphicsText = $"{_localizationService.RetrieveString("Infocenter_GraphicsPrefix")} {systemInformation.GraphicsCardName}";
            RamText = $"{_localizationService.RetrieveString("Infocenter_RamPrefix")} {systemInformation.InstalledRam}";
            OsEditionText = $"{_localizationService.RetrieveString("Infocenter_EditionPrefix")} {systemInformation.OsEdition}";
            OsVersionText = $"{_localizationService.RetrieveString("Infocenter_VersionPrefix")} {systemInformation.OsDisplayVersion}";
            OsBuildText = $"{_localizationService.RetrieveString("Infocenter_BuildPrefix")} {systemInformation.OsBuildVersion}";
            BrowserText = $"{_localizationService.RetrieveString("Infocenter_BrowserPrefix")} {systemInformation.StandardBrowserName}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
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






