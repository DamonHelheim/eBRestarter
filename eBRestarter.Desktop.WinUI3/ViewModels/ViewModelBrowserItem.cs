using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Extensions;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Desktop.WinUI3.Messages;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using eBRestarter.Core.Application.Ports.Inbound.UseCases;
using eBRestarter.Core.Application.Models;
using eBRestarter.Desktop.WinUI3.Models;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for a single browser entry on the "Installed Browsers" page. Handles display of
/// install state, version, download progress, and actions: choose as default or download/install
/// via <see cref="IHttpDownloadOutboundPort"/>. Sends <see cref="BrowserChangedMessage"/> when chosen.
/// </summary>
public sealed partial class ViewModelBrowserItem : ObservableObject
{
    private const string DownloadBrowserToggleButtonRedStyleKey = "DownloadBrowserToggleButtonRed";
    private const string DownloadBrowserToggleButtonStyleKey = "DownloadBrowserToggleButton";
    private const string InstalledIndicatorForegroundColorHex = "#7ED422";
    private const string InstallStateGlyphInstalledCharacter = "\uE73E";
    private const string InstallStateGlyphNotInstalledCharacter = "\uE711";
    private const string NotInstalledIndicatorForegroundColorHex = "#E40E87";

    private BrowserInfo _browserInfo;



    private readonly IDialogService _dialogService;

    private CancellationTokenSource? _downloadCancellationTokenSource;

    private readonly IDownloadBrowserUseCase _downloadBrowserUseCase;

    private readonly IEVisitorConfigRepositoryOutboundPort _EVRestarterConfigRepository;

    private readonly IInboundPortLocalizationProvider _localizationService;

    [ObservableProperty]
    public partial double DownloadProgressValue { get; set; }


    [ObservableProperty]
    public partial string BrowserVersionText { get; set; }

    [ObservableProperty]
    public partial string DownloadSizeText { get; set; }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonContent))]
    [NotifyPropertyChangedFor(nameof(DownloadButtonStyleKey))]
    [NotifyPropertyChangedFor(nameof(IsDownloading))]
    [NotifyPropertyChangedFor(nameof(IsNotDownloading))]
    public partial bool IsDownloadActive { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotInstalling))]
    public partial bool IsInstalling { get; set; }


    /// <summary>Foreground brush for the install-state glyph: green when installed, red when not.</summary>
    public SolidColorBrush InstallStateGlyphForeground => _browserInfo.IsInstalled
        ? new SolidColorBrush(CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(InstalledIndicatorForegroundColorHex))
        : new SolidColorBrush(CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(NotInstalledIndicatorForegroundColorHex));


    /// <summary>Localized "Cancel" while downloading, "Download" otherwise.</summary>
    public string DownloadButtonContent => IsDownloadActive
        ? _localizationService.RetrieveString("General_Cancel")
        : _localizationService.RetrieveString("General_Download");

    /// <summary>Style key for the download button (red when active for cancel).</summary>
    public string DownloadButtonStyleKey => IsDownloadActive
        ? DownloadBrowserToggleButtonRedStyleKey
        : DownloadBrowserToggleButtonStyleKey;

    /// <summary>Path to the browser icon asset.</summary>
    public string HeaderImageBrowser => _browserInfo.IconPath;

    /// <summary>Display name of the browser.</summary>
    public string HeaderTitleBrowser => _browserInfo.Name;

    /// <summary>Icon height for layout.</summary>
    public string ImageSizeHeightBrowser => "32";

    /// <summary>Icon width for layout.</summary>
    public string ImageSizeWidthBrowser => "32";

    /// <summary>Display symbol for install state: check mark if installed, ballot X otherwise.</summary>
    public string InstallStateGlyph => _browserInfo.IsInstalled
        ? InstallStateGlyphInstalledCharacter
        : InstallStateGlyphNotInstalledCharacter;


    /// <summary>True when the browser is installed so the user can choose it as default.</summary>
    public bool IsChooseButtonVisible => _browserInfo.IsInstalled;

    /// <summary>True when the browser is not installed so the user can start a download.</summary>
    public bool IsDownloadButtonVisible => !_browserInfo.IsInstalled;

    /// <summary>True when not installed, to show download size/progress.</summary>
    public bool IsDownloadSizeTextVisible => !_browserInfo.IsInstalled;

    /// <summary>True while a download is in progress.</summary>
    public bool IsDownloading => IsDownloadActive;

    /// <summary>True when no download is in progress.</summary>
    public bool IsNotDownloading => !IsDownloadActive;

    /// <summary>True when not in the post-download install phase.</summary>
    public bool IsNotInstalling => !IsInstalling;


    /// <summary>Browser type (Chrome, Firefox, Edge, Brave) for this item.</summary>
    public BrowserType BrowserType => _browserInfo.Type;

    /// <summary>
    /// Initializes the item with a snapshot of browser info and services; loads config for
    /// selected-browser persistence and refreshes the version text for display.
    /// </summary>
    public ViewModelBrowserItem(
        BrowserInfo browserInfo,
        IDownloadBrowserUseCase downloadBrowserUseCase,
        IEVisitorConfigRepositoryOutboundPort EVRestarterConfigRepository,
        IDialogService dialogService,
        IInboundPortLocalizationProvider LocalizationProvider)
    {
        ArgumentNullException.ThrowIfNull(browserInfo);
        ArgumentNullException.ThrowIfNull(downloadBrowserUseCase);
        ArgumentNullException.ThrowIfNull(EVRestarterConfigRepository);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(LocalizationProvider);

        _browserInfo = browserInfo;
        _downloadBrowserUseCase = downloadBrowserUseCase;
        _EVRestarterConfigRepository = EVRestarterConfigRepository;
        _dialogService = dialogService;
        _localizationService = LocalizationProvider;

        BrowserVersionText = string.Empty;
        DownloadSizeText = string.Empty;
        RefreshBrowserVersionText();
    }

    /// <summary>
    /// Sets this browser as the selected one in config, sends <see cref="BrowserChangedMessage"/>
    /// so the restarter and other pages update, and persists settings.
    /// </summary>
    [RelayCommand]
    private void ChooseBrowser()
    {
        var selectedBrowserName = HeaderTitleBrowser ?? string.Empty;
        var config = _EVRestarterConfigRepository.LoadConfig();
        config.Browser.Selected = selectedBrowserName;
        _EVRestarterConfigRepository.SaveConfig(config);

        WeakReferenceMessenger.Default.Send(new BrowserChangedMessage(selectedBrowserName));
    }

    /// <summary>
    /// If a download is active, cancels it and resets UI state. Otherwise starts the download
    /// and, when done, prompts to run the installer. Allows concurrent executions so rapid clicks don't block.
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task ToggleDownload()
    {
        if (IsDownloadActive)
            CancelDownload();
        else
            await StartDownloadAsync();
    }

    /// <summary>
    /// Updates this item when the underlying <see cref="BrowserInfo"/> changes (e.g. after install
    /// or version check). Only applies when install state or version actually changed; then
    /// refreshes version text and notifies dependent properties so the UI updates.
    /// </summary>
    /// <param name="updatedBrowserInfo">New snapshot from <see cref="IBrowserDiscoveryProviderOutboundPort"/>.</param>
    public void Update(BrowserInfo updatedBrowserInfo)
    {
        ArgumentNullException.ThrowIfNull(updatedBrowserInfo);

        if (_browserInfo.IsInstalled == updatedBrowserInfo.IsInstalled && _browserInfo.Version == updatedBrowserInfo.Version)
            return;

        _browserInfo = updatedBrowserInfo;
        NotifyBrowserInstallSnapshotChanged();
    }

    /// <summary>Asks the user whether to run the installer now; if yes, starts the executable and shows a finished message.</summary>
    private async Task AskToInstall(string installerFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(installerFilePath);

        bool installNow = await _dialogService.ShowYesNoDialogAsync(
            _localizationService.RetrieveString("Install_DialogTitle"),
            _localizationService.RetrieveString("Install_DialogQuestion"));

        if (installNow)
        {
            await _downloadBrowserUseCase.StartInstallerAsync(installerFilePath);
            await _dialogService.ShowMessageAsync(
                _localizationService.RetrieveString("Install_FinishedTitle"),
                _localizationService.RetrieveString("Install_FinishedMessage"));
        }
    }

    private void CancelDownload()
    {
        _downloadCancellationTokenSource?.Cancel();
        ResetDownloadState().Forget();
    }

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
            string versionDisplayPrefix = _localizationService.RetrieveString("Browser_VersionPrefix");
            BrowserVersionText = $"{versionDisplayPrefix} {_browserInfo.Version}";
        }
        else if (IsDownloading)
        {
            BrowserVersionText = string.Empty;
        }
        else
        {
            BrowserVersionText = _localizationService.RetrieveString("Browser_NotInstalled");
        }
    }

    private async Task ResetDownloadState()
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
            Debug.WriteLine($"{nameof(ViewModelBrowserItem)}: {nameof(BrowserInfo.DownloadUrl)} is missing; download was not started.");

            _downloadCancellationTokenSource.Dispose();
            _downloadCancellationTokenSource = null;

            return;
        }

        IsDownloadActive = true;
        RefreshBrowserVersionText();

        var progressHandler = new Progress<DownloadProgressStatus>(downloadProgressStatus =>
        {
            DownloadProgressValue = downloadProgressStatus.Percentage;
            DownloadSizeText =
                $"{FormatMegabytesFromBytes(downloadProgressStatus.BytesReceived):0.00} MB / {FormatMegabytesFromBytes(downloadProgressStatus.TotalBytes):0.00} MB";
        });

        try
        {
            var downloadPath = await _downloadBrowserUseCase.DownloadInstallerAsync(
                _browserInfo.Name,
                _browserInfo.DownloadUrl,
                progressHandler,
                _downloadCancellationTokenSource.Token);

            await AskToInstall(downloadPath);
        }
        catch (OperationCanceledException)
        {
            _downloadBrowserUseCase.CleanupPartialDownload(_browserInfo.Name);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or UnauthorizedAccessException)
        {
            Debug.WriteLine(ex);
            _downloadBrowserUseCase.CleanupPartialDownload(_browserInfo.Name);
        }
        finally
        {
            await ResetDownloadState();
            _downloadCancellationTokenSource?.Dispose();
            _downloadCancellationTokenSource = null;
        }
    }
}
















