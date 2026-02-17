using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Core.Domain.Models.Records.Config;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    /// <summary>
    /// View model for a single browser entry on the "Installed Browsers" page. Handles display of
    /// install state, version, download progress, and actions: choose as default or download/install
    /// via <see cref="IBrowserDownloadService"/>. Sends <see cref="BrowserChangedMessage"/> when chosen.
    /// </summary>
    public partial class ViewModelBrowserItem : ObservableObject
    {
        // =========================================================
        // 1. CONSTANTS & STATICS (Konstanten)
        // =========================================================
        #region ConstantsAndStatics

        private const string SetForegroundColorGreen = "#7ED422";
        private const string SetForegroundColorRed = "#E40E87";

        #endregion

        // =========================================================
        // 2. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private BrowserInfo _browserInfo;
        private CancellationTokenSource? _cts;
        private readonly AppConfig _currentConfig;
        private readonly IDialogService _dialogService;
        private readonly IBrowserDownloadService _downloadService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly ILocalizationService _localizationService;
        private readonly IOperatingSystemFacade _os;

        #endregion

        // =========================================================
        // 3. OBSERVABLE PROPERTIES (MVVM State)
        // =========================================================
        #region ObservableProperties

        [ObservableProperty] public partial double DownloadProgressValue { get; set; }
        [ObservableProperty] public partial string DownloadSizeText { get; set; } = string.Empty;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DownloadButtonContent))]
        [NotifyPropertyChangedFor(nameof(DownloadButtonStyleKey))]
        [NotifyPropertyChangedFor(nameof(IsDownloading))]
        [NotifyPropertyChangedFor(nameof(IsNotDownloading))]
        public partial bool IsDownloadActive { get; set; }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotInstalling))]
        public partial bool IsInstalling { get; set; }
        [ObservableProperty] public partial string BrowserVersionText { get; set; } = string.Empty;

        #endregion

        // =========================================================
        // 4. PUBLIC PROPERTIES (Data & State)
        // =========================================================
        #region PublicProperties

        /// <summary>Browser type (Chrome, Firefox, Edge, Brave) for this item.</summary>
        public BrowserType BrowserType => _browserInfo.Type;
        /// <summary>Display symbol for install state: ✓ if installed, ✘ otherwise.</summary>
        public string BrowserExist => _browserInfo.IsInstalled ? "✓" : "✘";
        /// <summary>Green brush when installed, red when not; used for the install-state indicator.</summary>
        public SolidColorBrush BrowserExistTextForeground => _browserInfo.IsInstalled
            ? new SolidColorBrush(CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(SetForegroundColorGreen))
            : new SolidColorBrush(CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(SetForegroundColorRed));
        /// <summary>Localized "Cancel" while downloading, "Download" otherwise.</summary>
        public string DownloadButtonContent => IsDownloadActive
            ? _localizationService.GetString("General_Cancel")
            : _localizationService.GetString("General_Download");
        /// <summary>Style key for the download button (red when active for cancel).</summary>
        public string DownloadButtonStyleKey => IsDownloadActive
            ? "DownloadBrowserToggleButtonRed"
            : "DownloadBrowserToggleButton";
        /// <summary>Path to the browser icon asset.</summary>
        public string HeaderImageBrowser => _browserInfo.IconPath;
        /// <summary>Display name of the browser.</summary>
        public string HeaderTitleBrowser => _browserInfo.Name;
        /// <summary>Icon height for layout.</summary>
        public string ImageSizeHeightBrowser => _browserInfo.IconHeight;
        /// <summary>Icon width for layout.</summary>
        public string ImageSizeWidthBrowser => _browserInfo.IconWidth;
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

        #endregion

        // =========================================================
        // 5. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        /// <summary>
        /// Initializes the item with a snapshot of browser info and services; loads config for
        /// selected-browser persistence and refreshes the version text for display.
        /// </summary>
        public ViewModelBrowserItem(
            BrowserInfo browserInfo,
            IBrowserDownloadService downloadService,
            IOperatingSystemFacade os,
            IEVisitorConfigService eVisitorConfigService,
            IDialogService dialogService,
            ILocalizationService localizationService)
        {
            _browserInfo = browserInfo;
            _downloadService = downloadService;
            _os = os;
            _eVisitorConfigService = eVisitorConfigService;
            _dialogService = dialogService;
            _localizationService = localizationService;
            _currentConfig = _eVisitorConfigService.LoadConfig();
            RefreshBrowserVersionText();
        }

        #endregion

        // =========================================================
        // 6. COMMANDS (MVVM Actions)
        // =========================================================
        #region Commands

        /// <summary>
        /// Sets this browser as the selected one in config, sends <see cref="BrowserChangedMessage"/>
        /// so the restarter and other pages update, and persists settings.
        /// </summary>
        [RelayCommand]
        private void ChooseBrowser()
        {
            _currentConfig.Browser.Selected = HeaderTitleBrowser;
            WeakReferenceMessenger.Default.Send(new BrowserChangedMessage(HeaderTitleBrowser));
            SaveSettings();
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

        #endregion

        // =========================================================
        // 7. PUBLIC & PROTECTED METHODS (API)
        // =========================================================
        #region PublicAndProtectedMethods

        /// <summary>
        /// Updates this item when the underlying <see cref="BrowserInfo"/> changes (e.g. after install
        /// or version check). Only applies when install state or version actually changed; then
        /// refreshes version text and notifies dependent properties so the UI updates.
        /// </summary>
        /// <param name="newInfo">New snapshot from <see cref="IBrowserService"/>. If null, behavior is undefined.</param>
        public void Update(BrowserInfo newInfo)
        {
            if (_browserInfo.IsInstalled == newInfo.IsInstalled && _browserInfo.Version == newInfo.Version)
                return;

            _browserInfo = newInfo;
            OnPropertyChanged(nameof(BrowserExist));
            OnPropertyChanged(nameof(BrowserExistTextForeground));
            RefreshBrowserVersionText();
            OnPropertyChanged(nameof(IsChooseButtonVisible));
            OnPropertyChanged(nameof(IsDownloadButtonVisible));
            OnPropertyChanged(nameof(IsDownloadSizeTextVisible));
            OnPropertyChanged(nameof(DownloadButtonContent));
        }

        #endregion

        // =========================================================
        // 8. PRIVATE HELPER METHODS (Interne Hilfsmethoden)
        // =========================================================
        #region PrivateHelperMethods

        /// <summary>Sets BrowserVersionText to localized version string, "not installed", or empty while downloading.</summary>
        private void RefreshBrowserVersionText()
        {
            if (_browserInfo.IsInstalled && IsDownloading is false)
            {
                string prefix = _localizationService.GetString("Browser_VersionPrefix");
                BrowserVersionText = $"{prefix} {_browserInfo.Version}";
            }
            else if (IsDownloading is true)
            {
                BrowserVersionText = string.Empty;
            }
            else
            {
                BrowserVersionText = _localizationService.GetString("Browser_NotInstalled");
            }
        }

        /// <summary>Asks the user whether to run the installer now; if yes, starts the executable and shows a finished message.</summary>
        private async Task AskToInstall(string path)
        {
            bool installNow = await _dialogService.ShowYesNoDialogAsync(
                _localizationService.GetString("Install_DialogTitle"),
                _localizationService.GetString("Install_DialogQuestion"));

            if (installNow)
            {
                await _os.WindowsProcessControlService.StartExecutableAsync(path);
                await _dialogService.ShowMessageAsync(
                    _localizationService.GetString("Install_FinishedTitle"),
                    _localizationService.GetString("Install_FinishedMessage"));
            }
        }

        private void CancelDownload()
        {
            _cts?.Cancel();
            _ = ResetDownloadState();
        }

        /// <summary>Removes a partially downloaded file so a retry starts clean. Swallows errors to avoid breaking the flow.</summary>
        private void CleanupPartialFile(string path)
        {
            try
            {
                if (_os.WindowsFileSystemService.FileExists(path))
                    _os.WindowsFileSystemService.DeleteFile(path);
            }
            catch (Exception) { }
        }

        private async Task ResetDownloadState()
        {
            IsDownloadActive = false;
            DownloadProgressValue = 0;
            DownloadSizeText = "";
            RefreshBrowserVersionText();
            _cts = null;
        }

        private void SaveSettings()
        {
            _eVisitorConfigService.SaveConfig(_currentConfig);
        }

        /// <summary>Downloads the browser installer to the user's Downloads folder, reports progress, then prompts for install. On cancel or error, cleans up and resets state.</summary>
        private async Task StartDownloadAsync()
        {
            _cts = new CancellationTokenSource();
            IsDownloadActive = true;
            RefreshBrowserVersionText();

            var fileName = $"{_browserInfo.Name}_Installer.exe";
            var userProfile = _os.WindowsFileSystemService.GetEnvironmentPath("UserProfile");
            var downloadPath = _os.WindowsFileSystemService.CombinePaths(userProfile, "Downloads", fileName);

            var progressHandler = new Progress<DownloadProgressStatus>(status =>
            {
                DownloadProgressValue = status.Percentage;
                DownloadSizeText = $"{status.BytesReceived / 1024d / 1024d:0.00} MB / {status.TotalBytes / 1024d / 1024d:0.00} MB";
            });

            try
            {
                await _downloadService.DownloadFileAsync(_browserInfo.DownloadUrl, downloadPath, progressHandler, _cts.Token);
                await AskToInstall(downloadPath);
                await ResetDownloadState();
            }
            catch (OperationCanceledException)
            {
                CleanupPartialFile(downloadPath);
                await ResetDownloadState();
            }
            catch (Exception)
            {
                CleanupPartialFile(downloadPath);
                await ResetDownloadState();
            }
        }

        #endregion
    }
}
