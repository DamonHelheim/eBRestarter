using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.Models;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Core.Domain.Models.Records.Config;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelBrowserItem : ObservableObject
    {
        #region Constants
        private const string SetForegroundColorGreen = "#7ED422";
        private const string SetForegroundColorRed = "#E40E87";
        #endregion

        #region Fields
        // Services und Dependencies
        private readonly BrowserInfo _browserInfo;
        private readonly AppConfig _currentConfig;
        private readonly IBrowserDownloadService _downloadService;
        private readonly IOperatingSystemFacade _os;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly IDialogService _dialogService;

        // Interner State
        private CancellationTokenSource? _cts;
        #endregion

        #region Constructor
        public ViewModelBrowserItem(
            BrowserInfo info,
            IBrowserDownloadService downloadService,
            IOperatingSystemFacade os,
            IEVisitorConfigService eVisitorConfigService,
            IDialogService dialogService)
        {
            _browserInfo = info;
            _downloadService = downloadService;
            _os = os;
            _eVisitorConfigService = eVisitorConfigService;
            _dialogService = dialogService;

            // Config laden (falls nötig direkt im Ctor)
            _currentConfig = _eVisitorConfigService.LoadConfig();
        }
        #endregion

        #region Observable Properties (State)
        // Diese Properties speichern den aktuellen Zustand.
        // Sie lösen PropertyChanged aus und triggern abhängige Properties.

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DownloadButtonContent))]
        [NotifyPropertyChangedFor(nameof(DownloadButtonStyleKey))]
        [NotifyPropertyChangedFor(nameof(IsDownloading))] // Optional, falls Bindings darauf hören
        [NotifyPropertyChangedFor(nameof(IsNotDownloading))]
        public partial bool IsDownloadActive { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotInstalling))]
        public partial bool IsInstalling { get; set; }

        [ObservableProperty]
        public partial double DownloadProgressValue { get; set; }

        [ObservableProperty]
        public partial string DownloadSizeText { get; set; } = string.Empty;
        #endregion

        #region Computed Properties (View Logic)
        // Diese Properties berechnen Werte "on the fly" basierend auf Feldern oder State.

        // --- Header & Info ---
        public string HeaderTitleBrowser => _browserInfo.Name;
        public string HeaderImageBrowser => _browserInfo.IconPath;
        public string ImageSizeHeightBrowser => _browserInfo.IconHeight;
        public string ImageSizeWidthBrowser => _browserInfo.IconWidth;

        // --- Status Anzeige (Installiert / Nicht Installiert) ---
        public string BrowserVersion => _browserInfo.IsInstalled ? $"Version: {_browserInfo.Version}" : "Nicht installiert";

        public string BrowserExist => _browserInfo.IsInstalled ? "✓" : "✘";

        public SolidColorBrush BrowserExistTextForground => _browserInfo.IsInstalled
            ? new SolidColorBrush(CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(SetForegroundColorGreen))
            : new SolidColorBrush(CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(SetForegroundColorRed));

        // --- Sichtbarkeiten (Visibility) ---
        public bool IsDownloadButtonVisible => !_browserInfo.IsInstalled;
        public bool IsChooseButtonVisible => _browserInfo.IsInstalled;

        public bool IsBrowserVersionVisible => _browserInfo.IsInstalled;

        public bool IsDownloadSizeTextVisible => !_browserInfo.IsInstalled;

        // --- Download & Install State Helpers ---
        public bool IsDownloading => IsDownloadActive;
        public bool IsNotDownloading => !IsDownloadActive;
        public bool IsNotInstalling => !IsInstalling;

        // --- Button Styling & Text ---
        public string DownloadButtonContent => IsDownloadActive ? "Abbrechen" : "Download";

        public string DownloadButtonStyleKey => IsDownloadActive
            ? "DownloadBrowserToggleButtonRed"
            : "DownloadBrowserToggleButton";
        #endregion

        #region Commands
        // Hier würden deine [RelayCommand] Methoden stehen (ToggleDownload, InstallBrowser etc.)
        #endregion

        #region Private Methods
        // Hier deine Hilfsmethoden (StartDownloadAsync, CancelDownload etc.)
        #endregion


        [RelayCommand]
        private void ChooseBrowser() {

            _currentConfig.Browser.Selected = HeaderTitleBrowser;

            // Senden des reinen Records
            WeakReferenceMessenger.Default.Send(new BrowserChangedMessage(HeaderTitleBrowser));

            SaveSettings();

        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task ToggleDownload()
        {
            if (IsDownloadActive)
            {
                // FALL A: Download läuft -> Abbrechen
                CancelDownload();
            }
            else
            {
                // FALL B: Download startet
                await StartDownloadAsync();
            }
        }

        private void CancelDownload()
        {
            _cts?.Cancel();
            _ = ResetDownloadState();
        }

        private async Task StartDownloadAsync()
        {
            _cts = new CancellationTokenSource();

            IsDownloadActive = true;

            // Dateinamen und Pfad zusammenbauen
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
            catch (Exception ex)
            {
                CleanupPartialFile(downloadPath);
                await ResetDownloadState();
            }
        }

        private void CleanupPartialFile(string path)
        {
            try
            {
                if (_os.WindowsFileSystemService.FileExists(path))
                {
                    _os.WindowsFileSystemService.DeleteFile(path);
                }
            }
            catch (Exception)
            {
            }
        }

        private async Task ResetDownloadState()
        {
            IsDownloadActive = false;
            DownloadProgressValue = 0;
            DownloadSizeText = "";
            _cts = null;
        }

        private async Task AskToInstall(string path)
        {
            // Hier deinen MessageBox Service oder Facade nutzen
            // if (ShowMessage("Installieren?")) _os.WindowsProcessControlService.StartExecutable(path);

            bool installNow = await _dialogService.ShowYesNoDialogAsync(
               "Installation",
               "Möchtest du installieren?");

            if (installNow)
            {
                await _os.WindowsProcessControlService.StartExecutableAsync(path);

                // 3. Wenn wir hier sind, ist der Installer fertig/geschlossen!
                await _dialogService.ShowMessageAsync("Installation beendet", "Der Browser wurde installiert.");
            }
        }

        private void SaveSettings()
        {
            _eVisitorConfigService.SaveConfig(_currentConfig);
        }
    }
}
