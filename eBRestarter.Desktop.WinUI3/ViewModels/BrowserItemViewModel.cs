using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Helpers;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.Models;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Core.Domain.Models.Records.Config;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class BrowserItemViewModel : ObservableObject
    {
        private readonly BrowserInfo _browserInfo;

        private readonly AppConfig _currentConfig;

        private readonly IBrowserDownloadService _downloadService;

        private readonly IOperatingSystemFacade _os;

        private readonly IEVisitorConfigService _eVisitorConfigService;

        private CancellationTokenSource? _cts;
        public BrowserItemViewModel(
            BrowserInfo info,
            IBrowserDownloadService downloadService,
            IOperatingSystemFacade os,
            IEVisitorConfigService eVisitorConfigService)
        {
            _browserInfo = info;
            _eVisitorConfigService = eVisitorConfigService;
            _downloadService = downloadService;
            _os = os;

            _currentConfig = _eVisitorConfigService.LoadConfig();
        }
        private const string SetForegroundColorGreen = "#7ED422";
        private const string SetForegroundColorRed = "#E40E87";
        public string HeaderTitleBrowser => _browserInfo.Name;
        public string HeaderImageBrowser => _browserInfo.IconPath;

        public string ImageSizeHeightBrowser => _browserInfo.IconHeight;
        public string ImageSizeWidthBrowser => _browserInfo.IconWidth;

        // UI-Logik: Textformatierung
        public string BrowserVersion => _browserInfo.IsInstalled ? $"Version: {_browserInfo.Version}" : "Nicht installiert";

        // UI-Logik: Farben & Icons über Properties (besser als Converter für einfache Logik)
        public SolidColorBrush BrowserExistTextForground => _browserInfo.IsInstalled ?
            new SolidColorBrush(CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(SetForegroundColorGreen)) :
            new SolidColorBrush(CommunityToolkit.WinUI.Helpers.ColorHelper.ToColor(SetForegroundColorRed));
        public string BrowserExist => _browserInfo.IsInstalled ? "✓" : "✘";

        // Sichtbarkeiten (Boolesche Werte für x:Bind oder Converter)
        public bool IsDownloadButtonVisible => !_browserInfo.IsInstalled;
        public bool IsChooseButtonVisible => _browserInfo.IsInstalled;

        // Commands
        [RelayCommand]
        private void Download() { /* ... */ }

        [RelayCommand]
        private void ChooseBrowser() {

            _currentConfig.Browser.Selected = HeaderTitleBrowser;
            SaveSettings();

        }

        // --- Neue Properties für den Download ---

        // 1. Die Property mit dem Attribut
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DownloadButtonContent))]
        [NotifyPropertyChangedFor(nameof(DownloadButtonStyleKey))] // Auch hier automatisch triggern!
        public partial bool IsDownloadActive { get; set; }

        // 2. Die berechneten Properties (Keine Änderung nötig)
        public string DownloadButtonContent => IsDownloadActive ? "Abbrechen" : "Download";

        public bool IsDownloading => IsDownloadActive; // Für Bindings (Sichtbarkeiten)
        public bool IsNotDownloading => !IsDownloadActive; // Gegenpart

        [ObservableProperty]
        public partial double DownloadProgressValue { get; set; }

        [ObservableProperty]
        public partial string DownloadSizeText { get; set; } = string.Empty;

        // Button Text ändert sich: "Download" <-> "Abbrechen"



        // Style Name für den Button (muss in XAML Resources definiert sein)
        public string DownloadButtonStyleKey => IsDownloadActive ? "DownloadBrowserToggleButtonRed" : "DownloadBrowserToggleButton";


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
            ResetDownloadState();
        }

        private async Task StartDownloadAsync()
        {
            _cts = new CancellationTokenSource();

            // UI Status setzen
            IsDownloadActive = true;

            var fileName = $"{_browserInfo.Name}_Installer.exe"; // Oder aus BrowserInfo holen
            var downloadPath = _os.WindowsFileSystemService.CombinePaths(
                _os.WindowsFileSystemService.GetEnvironmentPath("UserProfile"), "Downloads", fileName);

            var progressHandler = new Progress<DownloadProgressStatus>(status =>
            {
                DownloadProgressValue = status.Percentage;
                DownloadSizeText = $"{status.BytesReceived / 1024d / 1024d:0.00} MB / {status.TotalBytes / 1024d / 1024d:0.00} MB";
            });

            try
            {
                await _downloadService.DownloadFileAsync(_browserInfo.DownloadUrl, downloadPath, progressHandler, _cts.Token);

                // Erfolg!
                ResetDownloadState();
                AskToInstall(downloadPath);
            }
            catch (OperationCanceledException)
            {
                // Abbruch durch User - nichts tun, State wird im finally resettet
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung
                // _dialogService.ShowError("Download fehlgeschlagen...");
                ResetDownloadState();
            }
        }

        private void ResetDownloadState()
        {
            IsDownloadActive = false;
            DownloadProgressValue = 0;
            DownloadSizeText = "";
            _cts = null;
        }

        private void AskToInstall(string path)
        {
            // Hier deinen MessageBox Service oder Facade nutzen
            // if (ShowMessage("Installieren?")) _os.WindowsProcessControlService.StartExecutable(path);
        }

        private void SaveSettings()
        {
            _eVisitorConfigService.SaveConfig(_currentConfig);
        }
    }
}
