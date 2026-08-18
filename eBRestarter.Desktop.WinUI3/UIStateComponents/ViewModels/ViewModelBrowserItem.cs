using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.UI.Xaml.Media;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using eBRestarter.Core.Application.BehavioralComponents.Extensions;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.ObjectArchetypes.Models;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.SignalDTO.Messages;
using Microsoft.Extensions.Logging;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for a single browser entry on the "Installed Browsers" page. Handles display of
/// install state, version, download progress, and actions: choose as default or download/install
/// via <see cref="IHttpDownloadOutboundPort"/>. Sends <see cref="BrowserChangedMessage"/> when chosen.
/// </summary>
public sealed partial class ViewModelBrowserItem : ObservableObject
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const string DownloadBrowserToggleButtonRedStyleKey = "DownloadBrowserToggleButtonRed";
    private const string DownloadBrowserToggleButtonStyleKey = "DownloadBrowserToggleButton";
    private const string ImageSizeHeightBrowserValue = "32";
    private const string ImageSizeWidthBrowserValue = "32";
    private const string InstalledIndicatorForegroundColorHex = "#7ED422";
    private const string InstallStateGlyphInstalledCharacter = "\uE73E";
    private const string InstallStateGlyphNotInstalledCharacter = "\uE711";
    private const string KeyBrowserNotInstalled = "Browser_NotInstalled";
    private const string KeyBrowserVersionPrefix = "Browser_VersionPrefix";
    private const string KeyGeneralCancel = "General_Cancel";
    private const string KeyGeneralDownload = "General_Download";
    private const string KeyInstallDialogQuestion = "Install_DialogQuestion";
    private const string KeyInstallDialogTitle = "Install_DialogTitle";
    private const string KeyInstallFinishedMessage = "Install_FinishedMessage";
    private const string KeyInstallFinishedTitle = "Install_FinishedTitle";
    private const string NotInstalledIndicatorForegroundColorHex = "#E40E87";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies (alphabetical A–Z) ──
    private readonly IDialogService _dialogService;
    private readonly ILogger<ViewModelBrowserItem> _logger;
    private readonly IUseCaseDownloadBrowser _downloadBrowserUseCase;
    private readonly IOutboundPortEVisitorConfigRepository _evRestarterConfigRepository;
    private readonly IInboundPortLocalizationProvider _localizationService;

    // ── Block 4: Complex types / Repositories / Models / Objects (alphabetical A–Z) ──
    private BrowserInfo _browserInfo;
    private CancellationTokenSource? _downloadCancellationTokenSource;
    private static readonly SolidColorBrush InstalledBrush = new(CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(InstalledIndicatorForegroundColorHex));
    private static readonly SolidColorBrush NotInstalledBrush = new(CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(NotInstalledIndicatorForegroundColorHex));


    // ═══════════════════════════════════════════════════════
    //  3. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes the item with a snapshot of browser info and services; loads config for
    /// selected-browser persistence and refreshes the version text for display.
    /// </summary>
    public ViewModelBrowserItem(
        BrowserInfo browserInfo,
        IUseCaseDownloadBrowser downloadBrowserUseCase,
        IOutboundPortEVisitorConfigRepository evRestarterConfigRepository,
        IDialogService dialogService,
        IInboundPortLocalizationProvider localizationService,
        ILogger<ViewModelBrowserItem> logger)
    {
        ArgumentNullException.ThrowIfNull(browserInfo);
        ArgumentNullException.ThrowIfNull(downloadBrowserUseCase);
        ArgumentNullException.ThrowIfNull(evRestarterConfigRepository);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

        _browserInfo = browserInfo;
        _downloadBrowserUseCase = downloadBrowserUseCase;
        _evRestarterConfigRepository = evRestarterConfigRepository;
        _dialogService = dialogService;
        _localizationService = localizationService;

        BrowserVersionText = string.Empty;
        DownloadSizeText = string.Empty;
        RefreshBrowserVersionText();
    }


    // ═══════════════════════════════════════════════════════
    //  6. Properties
    // ═══════════════════════════════════════════════════════
    /// <summary>Browser type (Chrome, Firefox, Edge, Brave) for this item.</summary>
    public BrowserType BrowserType => _browserInfo.Type;

    /// <summary>Gets or sets the browser version display text.</summary>
    [ObservableProperty]
    public partial string BrowserVersionText { get; set; }

    /// <summary>Localized "Cancel" while downloading, "Download" otherwise.</summary>
    public string DownloadButtonContent => IsDownloadActive
        ? _localizationService.RetrieveString(KeyGeneralCancel)
        : _localizationService.RetrieveString(KeyGeneralDownload);

    /// <summary>Style key for the download button (red when active for cancel).</summary>
    public string DownloadButtonStyleKey => IsDownloadActive
        ? DownloadBrowserToggleButtonRedStyleKey
        : DownloadBrowserToggleButtonStyleKey;

    /// <summary>Gets or sets the current download progress value (0 to 100).</summary>
    [ObservableProperty]
    public partial double DownloadProgressValue { get; set; }

    /// <summary>Gets or sets the download size progress display text.</summary>
    [ObservableProperty]
    public partial string DownloadSizeText { get; set; }

    /// <summary>Path to the browser icon asset.</summary>
    public string HeaderImageBrowser => _browserInfo.IconPath;

    /// <summary>Display name of the browser.</summary>
    public string HeaderTitleBrowser => _browserInfo.Name;

    /// <summary>Icon height for layout.</summary>
    public static string ImageSizeHeightBrowser => ImageSizeHeightBrowserValue;

    /// <summary>Icon width for layout.</summary>
    public static string ImageSizeWidthBrowser => ImageSizeWidthBrowserValue;

    /// <summary>Display symbol for install state: check mark if installed, ballot X otherwise.</summary>
    public string InstallStateGlyph => _browserInfo.IsInstalled
        ? InstallStateGlyphInstalledCharacter
        : InstallStateGlyphNotInstalledCharacter;

    /// <summary>Foreground brush for the install-state glyph: green when installed, red when not.</summary>
    public SolidColorBrush InstallStateGlyphForeground => _browserInfo.IsInstalled ? InstalledBrush : NotInstalledBrush;

    /// <summary>True when the browser is installed so the user can choose it as default.</summary>
    public bool IsChooseButtonVisible => _browserInfo.IsInstalled;

    /// <summary>Gets or sets a value indicating whether a download operation is actively running.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonContent))]
    [NotifyPropertyChangedFor(nameof(DownloadButtonStyleKey))]
    [NotifyPropertyChangedFor(nameof(IsDownloading))]
    [NotifyPropertyChangedFor(nameof(IsNotDownloading))]
    public partial bool IsDownloadActive { get; set; }

    /// <summary>True when the browser is not installed so the user can start a download.</summary>
    public bool IsDownloadButtonVisible => !_browserInfo.IsInstalled;

    /// <summary>True while a download is in progress.</summary>
    public bool IsDownloading => IsDownloadActive;

    /// <summary>True when not installed, to show download size/progress.</summary>
    public bool IsDownloadSizeTextVisible => !_browserInfo.IsInstalled;

    /// <summary>Gets or sets a value indicating whether the browser is currently installing.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotInstalling))]
    public partial bool IsInstalling { get; set; }

    /// <summary>True when no download is in progress.</summary>
    public bool IsNotDownloading => !IsDownloadActive;

    /// <summary>True when not in the post-download install phase.</summary>
    public bool IsNotInstalling => !IsInstalling;


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>Asks the user whether to run the installer now; if yes, starts the executable and shows a finished message.</summary>
    private async Task AskToInstallAsync(string installerFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(installerFilePath);

        bool installNow = await _dialogService.ShowYesNoDialogAsync(
            _localizationService.RetrieveString(KeyInstallDialogTitle),
            _localizationService.RetrieveString(KeyInstallDialogQuestion));

        if (installNow)
        {
            await _downloadBrowserUseCase.StartInstallerAsync(installerFilePath);
            await _dialogService.ShowMessageAsync(
                _localizationService.RetrieveString(KeyInstallFinishedTitle),
                _localizationService.RetrieveString(KeyInstallFinishedMessage));
        }
    }

    /// <summary>
    /// Cancels the active download operation and resets UI progress state.
    /// </summary>
    private void CancelDownload()
    {
        _downloadCancellationTokenSource?.Cancel();
        ResetDownloadState();
    }

    /// <summary>
    /// Sets this browser as the selected one in config, sends <see cref="BrowserChangedMessage"/>
    /// so the restarter and other pages update, and persists settings.
    /// </summary>
    [RelayCommand]
    private void ChooseBrowser()
    {
        var selectedBrowserName = HeaderTitleBrowser ?? string.Empty;
        var config = _evRestarterConfigRepository.LoadConfig();
        if (config?.Browser != null)
        {
            config.Browser.Selected = selectedBrowserName;
            _evRestarterConfigRepository.SaveConfig(config);
        }

        WeakReferenceMessenger.Default.Send(new BrowserChangedMessage(selectedBrowserName));
    }

    /// <summary>
    /// Converts a raw byte count into megabytes (MB).
    /// </summary>
    /// <param name="byteCount">The size in bytes.</param>
    /// <returns>The size in megabytes.</returns>
    private static double FormatMegabytesFromBytes(long byteCount) =>
        byteCount / (1024d * 1024d);

    /// <summary>Raises property change for UI bound to <see cref="BrowserInfo"/> install snapshot.</summary>
    private void NotifyBrowserInstallSnapshotChanged()
    {
        OnPropertyChanged(nameof(InstallStateGlyph));
        OnPropertyChanged(nameof(InstallStateGlyphForeground));
        RefreshBrowserVersionText();
        OnPropertyChanged(nameof(IsChooseButtonVisible));
        OnPropertyChanged(nameof(IsDownloadButtonVisible));
        OnPropertyChanged(nameof(IsDownloadSizeTextVisible));
        OnPropertyChanged(nameof(DownloadButtonContent));
    }

    /// <summary>Sets BrowserVersionText to localized version string, "not installed", or empty while downloading.</summary>
    private void RefreshBrowserVersionText()
    {
        if (_browserInfo.IsInstalled && !IsDownloading)
        {
            string versionDisplayPrefix = _localizationService.RetrieveString(KeyBrowserVersionPrefix);
            BrowserVersionText = $"{versionDisplayPrefix} {_browserInfo.Version}";
        }
        else if (IsDownloading)
        {
            BrowserVersionText = string.Empty;
        }
        else
        {
            BrowserVersionText = _localizationService.RetrieveString(KeyBrowserNotInstalled);
        }
    }

    /// <summary>
    /// Resets download progress properties and releases cancellation token resources.
    /// </summary>
    private void ResetDownloadState()
    {
        IsDownloadActive = false;
        DownloadProgressValue = 0;
        DownloadSizeText = string.Empty;
        RefreshBrowserVersionText();
        _downloadCancellationTokenSource = null;
    }

    /// <summary>Downloads the browser installer to the user's Downloads folder, reports progress, then prompts for install. On cancel or error, cleans up and resets state.</summary>
    private async Task StartDownloadAsync()
    {
        _downloadCancellationTokenSource?.Dispose();
        _downloadCancellationTokenSource = new CancellationTokenSource();

        if (string.IsNullOrWhiteSpace(_browserInfo.DownloadUrl))
        {
            _logger.LogWarning(
                LogEventIds.Update.UpdateDownloadUrlMissing,
                "No download URL is configured for {BrowserName}; the download was not started.",
                _browserInfo.Name);

            _downloadCancellationTokenSource.Dispose();
            _downloadCancellationTokenSource = null;

            return;
        }

        IsDownloadActive = true;
        RefreshBrowserVersionText();

        var progressHandler = new Progress<DownloadProgressStatus>(downloadProgressStatus =>
        {
            DownloadProgressValue = downloadProgressStatus.Percentage;
            DownloadSizeText = $"{FormatMegabytesFromBytes(downloadProgressStatus.BytesReceived):0.00} MB / {FormatMegabytesFromBytes(downloadProgressStatus.TotalBytes):0.00} MB";
        });

        try
        {
            var downloadPath = await _downloadBrowserUseCase.DownloadInstallerAsync(
                _browserInfo.Name,
                _browserInfo.DownloadUrl,
                progressHandler,
                _downloadCancellationTokenSource.Token);

            await AskToInstallAsync(downloadPath);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug(
                LogEventIds.Update.BrowserInstallerCleanupFailed,
                "Download of {BrowserName} was cancelled by the user; cleaning up the partial file.",
                _browserInfo.Name);

            _downloadBrowserUseCase.CleanupPartialDownload(_browserInfo.Name);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or UnauthorizedAccessException)
        {
            _logger.LogError(
                LogEventIds.Update.BrowserInstallerCleanupFailed,
                ex,
                "Downloading or installing {BrowserName} failed.",
                _browserInfo.Name);
            _downloadBrowserUseCase.CleanupPartialDownload(_browserInfo.Name);
        }
        finally
        {
            ResetDownloadState();
            _downloadCancellationTokenSource?.Dispose();
            _downloadCancellationTokenSource = null;
        }
    }

    /// <summary>
    /// If a download is active, cancels it and resets UI state. Otherwise starts the download
    /// and, when done, prompts to run the installer. Allows concurrent executions so rapid clicks don't block.
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task ToggleDownloadAsync()
    {
        if (IsDownloadActive)
        {
            CancelDownload();
        }
        else
        {
            await StartDownloadAsync();
        }
    }

    /// <summary>
    /// Updates this item when the underlying <see cref="BrowserInfo"/> changes (e.g. after install
    /// or version check). Only applies when install state or version actually changed; then
    /// refreshes version text and notifies dependent properties so the UI updates.
    /// </summary>
    /// <param name="updatedBrowserInfo">New snapshot from <see cref="IOutboundPortBrowserDiscoveryProvider"/>.</param>
    public void Update(BrowserInfo updatedBrowserInfo)
    {
        ArgumentNullException.ThrowIfNull(updatedBrowserInfo);

        if (_browserInfo.IsInstalled == updatedBrowserInfo.IsInstalled && _browserInfo.Version == updatedBrowserInfo.Version)
        {
            return;
        }

        _browserInfo = updatedBrowserInfo;
        NotifyBrowserInstallSnapshotChanged();
    }
}
