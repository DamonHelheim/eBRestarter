using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Core.Domain.Models.Records.Config;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelDeleteBrowserContent : ObservableObject
    {
        #region Fields

        private readonly IBrowserFactory _browserFactory;
        private readonly IEVisitorConfigService _iEVisitorConfigService;
        private readonly IDialogService _dialogService;
        private readonly IFileDeletionService _fileDeletionService;
        private readonly IWindowsProcessControlService _processService;
        private readonly ILocalizationService _localizationService; // <--- NEU: Service Feld

        private BrowserPaths? _browserPaths;
        private CancellationTokenSource? _cts;
        private IBrowser? _currentBrowser;
        private string _processName = "";

        #endregion

        #region Observable Properties

        [ObservableProperty] public partial string BrowserIconPath { get; set; } = string.Empty;

        // Initialwert entfernen wir hier, da wir ihn im Konstruktor übersetzt setzen
        [ObservableProperty] public partial string BrowserName { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelCleaningCommand))]
        public partial bool IsBusy { get; set; } = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
        public partial bool IsDeleteCookiesChecked { get; set; } = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
        public partial bool IsDeleteInternetCacheChecked { get; set; } = true;

        [ObservableProperty] public partial bool IsProcessConflict { get; set; } = false;
        [ObservableProperty] public partial double ProgressMaximum { get; set; } = 100;
        [ObservableProperty] public partial string ProgressText { get; set; } = "0 %";

        // Initialwert entfernen wir hier, da wir ihn im Konstruktor übersetzt setzen
        [ObservableProperty] public partial double ProgressValue { get; set; } = 0;
        [ObservableProperty] public partial string StatusText { get; set; }

        #endregion

        #region Properties

        private bool CanCancel() => IsBusy;
        private bool CanClean() => !IsBusy && (IsDeleteCookiesChecked || IsDeleteInternetCacheChecked);

        #endregion

        #region Constructors

        public ViewModelDeleteBrowserContent(
            IBrowserFactory browserFactory,
            IWindowsProcessControlService processService,
            IFileDeletionService fileDeletionService,
            IEVisitorConfigService iEVisitorConfigService,
            IDialogService dialogService,
            ILocalizationService localizationService) // <--- Injizieren
        {
            _browserFactory = browserFactory;
            _iEVisitorConfigService = iEVisitorConfigService;
            _processService = processService;
            _fileDeletionService = fileDeletionService;
            _dialogService = dialogService;
            _localizationService = localizationService; // <--- Zuweisen

            // Lokalisierte Standardwerte setzen
            BrowserName = _localizationService.GetString("Cleanup_Loading"); // "Lade..."
            StatusText = _localizationService.GetString("Cleanup_Ready");    // "Bereit."

            // 1. Config laden
            AppConfig config = iEVisitorConfigService.LoadConfig();

            // 2. Browser wählen
            string selectedBrowserString = config.Browser.Selected;

            if (Enum.TryParse(typeof(BrowserType), selectedBrowserString, true, out var result))
            {
                var browserType = (BrowserType)result;
                Initialize(browserType);
            }
            else
            {
                // Fehlertext lokalisiert formatieren
                string errorFormat = _localizationService.GetString("Cleanup_ConfigError");
                StatusText = string.Format(errorFormat, selectedBrowserString);
            }
        }

        #endregion

        #region Commands

        [RelayCommand(CanExecute = nameof(CanCancel))]
        private void CancelCleaning()
        {
            _cts?.Cancel();
            StatusText = _localizationService.GetString("Cleanup_Canceling"); // "Breche ab..."
        }

        [RelayCommand(CanExecute = nameof(CanClean))]
        private async Task StartCleaning()
        {
            if (IsBusy || _currentBrowser == null || _browserPaths == null) return;

            if (_processService.IsProcessAlive(_processName))
            {
                IsProcessConflict = true;
                StatusText = _localizationService.GetString("Cleanup_BrowserRunning"); // "Browser läuft noch..."
                return;
            }

            await ExecuteCleaningLogic();
        }

        [RelayCommand]
        private async Task ForceCloseAndContinue()
        {
            IsProcessConflict = false;

            _processService.CloseApplication(_processName);
            StatusText = _localizationService.GetString("Cleanup_ClosingBrowser"); // "Beende Browser..."
            await Task.Delay(1000);

            if (_processService.IsProcessAlive(_processName))
            {
                StatusText = _localizationService.GetString("Cleanup_CloseFailed"); // "Konnte nicht beenden..."
                return;
            }

            await ExecuteCleaningLogic();
        }

        [RelayCommand]
        private void CancelConflict()
        {
            IsProcessConflict = false;
            StatusText = _localizationService.GetString("Cleanup_CanceledByUser"); // "Abbruch durch Benutzer."
        }

        private async Task ExecuteCleaningLogic()
        {
            var directoriesToDelete = new List<string>();

            if (IsDeleteInternetCacheChecked && _browserPaths.CacheDirs != null && _browserPaths.CacheDirs.Count > 0)
                directoriesToDelete.AddRange(_browserPaths.CacheDirs);

            if (IsDeleteCookiesChecked && _browserPaths.CookiesDirs != null && _browserPaths.CookiesDirs.Count > 0)
                directoriesToDelete.AddRange(_browserPaths.CookiesDirs);

            if (directoriesToDelete.Count == 0)
            {
                StatusText = _localizationService.GetString("Cleanup_NoPaths"); // "Keine gültigen Pfade..."
                return;
            }

            IsBusy = true;
            _cts = new CancellationTokenSource();

            try
            {
                StatusText = _localizationService.GetString("Cleanup_Analyzing"); // "Analysiere Dateien..."

                int totalFiles = await _fileDeletionService.CountFilesAsync(directoriesToDelete);
                ProgressMaximum = totalFiles > 0 ? totalFiles : 1;
                ProgressValue = 0;

                var statusProgress = new Progress<string>(msg => StatusText = msg);
                var valueProgress = new Progress<int>(val =>
                {
                    ProgressValue = val;
                    if (totalFiles > 0) ProgressText = $"{(val * 100 / totalFiles)} %";
                });

                await _fileDeletionService.DeleteFilesAsync(directoriesToDelete, statusProgress, valueProgress, _cts.Token);

                StatusText = _localizationService.GetString("Cleanup_Finished"); // "Bereinigung abgeschlossen."
            }
            catch (Exception ex)
            {
                string errorFormat = _localizationService.GetString("General_ErrorPrefix");
                StatusText = string.Format(errorFormat, ex.Message);
            }
            finally
            {
                IsBusy = false;
                _cts = null;
            }
        }

        #endregion

        #region Methods

        public void Initialize(BrowserType selectedBrowserType)
        {
            try
            {
                _currentBrowser = _browserFactory.Create(selectedBrowserType);

                BrowserName = _currentBrowser.DisplayName;
                BrowserIconPath = _currentBrowser.IconPath;

                _browserPaths = _currentBrowser.GetPaths();
                _processName = GetProcessNameByType(selectedBrowserType);
            }
            catch (Exception ex)
            {
                string errorFormat = _localizationService.GetString("General_LoadErrorPrefix");
                StatusText = string.Format(errorFormat, ex.Message);
            }
        }

        private string GetProcessNameByType(BrowserType type)
        {
            return type switch
            {
                BrowserType.Chrome => "chrome",
                BrowserType.Firefox => "firefox",
                BrowserType.Edge => "msedge",
                BrowserType.Brave => "brave",
                _ => ""
            };
        }

        #endregion
    }
}