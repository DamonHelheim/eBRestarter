using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.UI.Dispatching;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using eBRestarter.Desktop.WinUI3.BehavioralComponents.Extensions;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using Microsoft.Extensions.Logging;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for a single browser's row in the "Install Add-on" dialog. Shows whether the
/// browser is installed and whether the eBesucher extension is present; exposes a command to
/// open the store/extension page. Status is updated by the parent via <see cref="RefreshStatusAsync"/>.
/// </summary>
public sealed partial class ViewModelBrowserAddonStatus : ObservableObject
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const string AddonPlaceholderDash = "-";
    private const string AddonStatusColorExtensionInstalledHex = "#7ED422";
    private const string AddonStatusColorExtensionNotInstalledHex = "#E40E87";
    private const string AddonStatusIconDefault = "?";
    private const string AddonStatusIconEmpty = "";
    private const string AddonStatusIconExtensionInstalled = "\u2713";
    private const string AddonStatusIconExtensionNotInstalled = "\u2717";
    private const string InstallStatusColorBrowserInstalledHex = "#2e7d32";
    private const string InstallStatusColorBrowserNotInstalledHex = "#BA224D";
    private const string KeyAddonBrowserInstalled = "Addon_BrowserInstalled";
    private const string KeyAddonBrowserNotInstalled = "Addon_BrowserNotInstalled";
    private const string KeyAddonBtnBrowserMissing = "Addon_BtnBrowserMissing";
    private const string KeyAddonBtnInstall = "Addon_BtnInstall";
    private const string KeyAddonBtnManage = "Addon_BtnManage";
    private const string KeyAddonExtensionInstalled = "Addon_ExtensionInstalled";
    private const string KeyAddonExtensionNotInstalled = "Addon_ExtensionNotInstalled";
    private const string KeyAddonLoading = "Addon_Loading";
    private const string KeyAddonStatusChecking = "Addon_StatusChecking";
    private const string NeutralForegroundHex = "#000000";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies (alphabetical A–Z) ──
    private readonly IOutboundPortBrowser _browser;
    private readonly ILogger<ViewModelBrowserAddonStatus> _logger;
    private readonly IInboundPortLocalizationProvider _localizationService;

    // ── Block 4: Complex types / Framework objects (alphabetical A–Z) ──
    private readonly DispatcherQueue _dispatcherQueue;


    // ═══════════════════════════════════════════════════════
    //  3. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes the row with a browser instance and localization, sets display name and icon path
    /// (adjusted for WinUI asset path), and kicks off an initial <see cref="RefreshStatusAsync"/> so the first
    /// paint shows install/extension state.
    /// </summary>
    public ViewModelBrowserAddonStatus(
        IOutboundPortBrowser browser,
        IInboundPortLocalizationProvider localizationService,
        ILogger<ViewModelBrowserAddonStatus> logger)
    {
        ArgumentNullException.ThrowIfNull(browser);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(logger);

        _browser = browser;
        _localizationService = localizationService;
        _logger = logger;

        _dispatcherQueue = DispatcherQueue.GetForCurrentThread()
            ?? throw new InvalidOperationException($"{nameof(ViewModelBrowserAddonStatus)} must be constructed on a thread with a WinUI DispatcherQueue (UI thread).");

        BrowserName = _browser.DisplayName;
        IconPath = _browser.IconPath;
        AddonStatusColor = NeutralForegroundHex;
        AddonStatusIcon = AddonStatusIconDefault;
        InstallStatusColor = NeutralForegroundHex;
        AddonStatusText = _localizationService.RetrieveString(KeyAddonStatusChecking);
        InstallStatusText = _localizationService.RetrieveString(KeyAddonStatusChecking);
        ButtonText = _localizationService.RetrieveString(KeyAddonLoading);

        // Initial probe is asynchronous; UI displays "Checking" placeholder until complete.
        RefreshStatusAsync().Forget(_logger, nameof(RefreshStatusAsync));
    }


    // ═══════════════════════════════════════════════════════
    //  6. Properties
    // ═══════════════════════════════════════════════════════
    /// <summary>Gets or sets the hex color code for the extension status text.</summary>
    [ObservableProperty]
    public partial string AddonStatusColor { get; set; }

    /// <summary>Gets or sets the glyph icon representing extension status.</summary>
    [ObservableProperty]
    public partial string AddonStatusIcon { get; set; }

    /// <summary>Gets or sets the localized extension status text.</summary>
    [ObservableProperty]
    public partial string AddonStatusText { get; set; }

    /// <summary>Gets or sets the browser display name.</summary>
    [ObservableProperty]
    public partial string BrowserName { get; set; }

    /// <summary>Gets or sets the action button display text.</summary>
    [ObservableProperty]
    public partial string ButtonText { get; set; }

    /// <summary>Gets or sets the browser icon asset file path.</summary>
    [ObservableProperty]
    public partial string IconPath { get; set; }

    /// <summary>Gets or sets the hex color code for browser install status text.</summary>
    [ObservableProperty]
    public partial string InstallStatusColor { get; set; }

    /// <summary>Gets or sets the localized browser install status text.</summary>
    [ObservableProperty]
    public partial string InstallStatusText { get; set; }

    /// <summary>Gets or sets a value indicating whether the action button is enabled.</summary>
    [ObservableProperty]
    public partial bool IsButtonEnabled { get; set; }


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>Opens the browser's extension/store URL in the browser if the URL is set (e.g. Chrome Web Store).</summary>
    [RelayCommand]
    private void OpenStore()
    {
        if (string.IsNullOrWhiteSpace(_browser.ExtensionInstallUrl))
        {
            return;
        }

        // ⚠️ Exception guideline: Start() returns false on launch failure instead of swallowing exceptions.
        if (!_browser.Start(_browser.ExtensionInstallUrl))
        {
            _logger.LogWarning(
                LogEventIds.Browser.BrowserStartFailed,
                "Could not open the extension store for {BrowserType}; the browser did not start.",
                _browser.Type);
        }
    }

    /// <summary>
    /// Probes install and extension state on a background thread, then applies the result to the
    /// bindable properties on the UI thread.
    /// </summary>
    /// <remarks>
    /// ⚡ WinUI 3 threading architecture note: Ensures non-blocking UI thread execution. File I/O
    /// and registry operations (such as checking installed browser paths and reading profile configuration files)
    /// run asynchronously on a background thread via <see cref="Task.Run"/>. Only UI binding updates are dispatched
    /// back to the UI thread via <see cref="DispatcherQueue"/>.
    /// </remarks>
    public async Task RefreshStatusAsync()
    {
        BrowserAddonProbeResult probeResult;

        try
        {
            probeResult = await Task.Run(Probe).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            // Probe failed (e.g. access denied): Leave state unchanged; next timer tick will retry.
            // Logging guideline: Debug level used instead of Warning since retries occur periodically.
            _logger.LogDebug(
                LogEventIds.Browser.BrowserProfileDiscoveryFailed,
                "Probing add-on status for {BrowserType} failed: {Reason}",
                _browser.Type,
                exception.Message);

            return;
        }

        _dispatcherQueue.TryEnqueue(() => ApplyProbeResult(probeResult));
    }

    /// <summary>Writes the probe result into the bindable properties. Must run on the UI thread.</summary>
    private void ApplyProbeResult(BrowserAddonProbeResult probeResult)
    {
        if (!probeResult.IsInstalled)
        {
            string notInstalledFormat = _localizationService.RetrieveString(KeyAddonBrowserNotInstalled);

            InstallStatusText = string.Format(notInstalledFormat, BrowserName);
            InstallStatusColor = InstallStatusColorBrowserNotInstalledHex;
            AddonStatusText = AddonPlaceholderDash;
            AddonStatusIcon = AddonStatusIconEmpty;
            IsButtonEnabled = false;
            ButtonText = _localizationService.RetrieveString(KeyAddonBtnBrowserMissing);

            return;
        }

        string installedFormat = _localizationService.RetrieveString(KeyAddonBrowserInstalled);

        InstallStatusText = string.Format(installedFormat, BrowserName);
        InstallStatusColor = InstallStatusColorBrowserInstalledHex;
        IsButtonEnabled = true;

        if (probeResult.IsExtensionInstalled)
        {
            AddonStatusText = _localizationService.RetrieveString(KeyAddonExtensionInstalled);
            AddonStatusIcon = AddonStatusIconExtensionInstalled;
            AddonStatusColor = AddonStatusColorExtensionInstalledHex;
            ButtonText = _localizationService.RetrieveString(KeyAddonBtnManage);
        }
        else
        {
            AddonStatusText = _localizationService.RetrieveString(KeyAddonExtensionNotInstalled);
            AddonStatusIcon = AddonStatusIconExtensionNotInstalled;
            AddonStatusColor = AddonStatusColorExtensionNotInstalledHex;
            ButtonText = _localizationService.RetrieveString(KeyAddonBtnInstall);
        }
    }

    /// <summary>Performs the blocking registry and file system probe. Runs on a background thread.</summary>
    private BrowserAddonProbeResult Probe()
    {
        bool isInstalled = _browser.IsInstalled;

        // Extension discovery is expensive and only executed if the browser is installed.
        bool isExtensionInstalled = isInstalled && _browser.IsExtensionInstalled(string.Empty);

        return new BrowserAddonProbeResult(isInstalled, isExtensionInstalled);
    }


    // ═══════════════════════════════════════════════════════
    //  9. Nested Types
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Represents the result of a single background browser add-on probe. Uses a <see langword="readonly record struct"/>
    /// to eliminate heap allocations for lightweight boolean status values.
    /// </summary>
    private readonly record struct BrowserAddonProbeResult(bool IsInstalled, bool IsExtensionInstalled);
}
