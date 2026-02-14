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
        private readonly AppConfig _currentConfig;
        private readonly IDialogService _dialogService;
        private readonly IBrowserDownloadService _downloadService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly IOperatingSystemFacade _os;
        private readonly ILocalizationService _localizationService;
        private CancellationTokenSource? _cts;

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

        public BrowserType BrowserType => _browserInfo.Type;
        public string BrowserExist => _browserInfo.IsInstalled ? "✓" : "✘";
        public SolidColorBrush BrowserExistTextForground => _browserInfo.IsInstalled
            ? new SolidColorBrush(CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(SetForegroundColorGreen))
            : new SolidColorBrush(CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(SetForegroundColorRed));
        public string DownloadButtonContent => IsDownloadActive
            ? _localizationService.GetString("General_Cancel")
            : _localizationService.GetString("General_Download");
        public string DownloadButtonStyleKey => IsDownloadActive
            ? "DownloadBrowserToggleButtonRed"
            : "DownloadBrowserToggleButton";
        public string HeaderImageBrowser => _browserInfo.IconPath;
        public string HeaderTitleBrowser => _browserInfo.Name;
        public string ImageSizeHeightBrowser => _browserInfo.IconHeight;
        public string ImageSizeWidthBrowser => _browserInfo.IconWidth;
        public bool IsChooseButtonVisible => _browserInfo.IsInstalled;
        public bool IsDownloadButtonVisible => !_browserInfo.IsInstalled;
        public bool IsDownloadSizeTextVisible => !_browserInfo.IsInstalled;
        public bool IsDownloading => IsDownloadActive;
        public bool IsNotDownloading => !IsDownloadActive;
        public bool IsNotInstalling => !IsInstalling;

        #endregion

        // =========================================================
        // 5. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        public ViewModelBrowserItem(
            BrowserInfo info,
            IBrowserDownloadService downloadService,
            IOperatingSystemFacade os,
            IEVisitorConfigService eVisitorConfigService,
            IDialogService dialogService,
            ILocalizationService localizationService)
        {
            _browserInfo = info;
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

        [RelayCommand]
        private void ChooseBrowser()
        {
            _currentConfig.Browser.Selected = HeaderTitleBrowser;
            WeakReferenceMessenger.Default.Send(new BrowserChangedMessage(HeaderTitleBrowser));
            SaveSettings();
        }

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

        public void Update(BrowserInfo newInfo)
        {
            if (_browserInfo.IsInstalled == newInfo.IsInstalled && _browserInfo.Version == newInfo.Version)
                return;

            _browserInfo = newInfo;
            OnPropertyChanged(nameof(BrowserExist));
            OnPropertyChanged(nameof(BrowserExistTextForground));
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
