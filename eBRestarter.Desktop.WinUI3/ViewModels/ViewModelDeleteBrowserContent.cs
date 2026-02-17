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
    /// <summary>
    /// View model for the "Delete browser content" (cache/cookies cleanup) dialog. Resolves the
    /// selected browser via <see cref="IBrowserFactory"/>, collects paths from the browser implementation,
    /// and runs file deletion through <see cref="IFileDeletionService"/> with progress reporting.
    /// Can be run manually or as an auto-step from the restart task when cleanup is due.
    /// </summary>
    public partial class ViewModelDeleteBrowserContent : ObservableObject
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IBrowserFactory _browserFactory;
        private readonly IDialogService _dialogService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly IFileDeletionService _fileDeletionService;
        private readonly ILocalizationService _localizationService;
        private readonly IWindowsProcessControlService _processService;
        private BrowserPaths? _browserPaths;
        private CancellationTokenSource? _cts;
        private IBrowser? _currentBrowser;
        private bool _isAutoMode = false;
        private string _processName = "";

        #endregion

        // =========================================================
        // 2. OBSERVABLE PROPERTIES (MVVM State)
        // =========================================================
        #region ObservableProperties

        [ObservableProperty] public partial string BrowserIconPath { get; set; } = string.Empty;
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
        [ObservableProperty] public partial double ProgressValue { get; set; } = 0;
        [ObservableProperty] public partial string StatusText { get; set; }

        #endregion

        // =========================================================
        // 3. PUBLIC PROPERTIES (Data & State)
        // =========================================================
        #region PublicProperties

        /// <summary>Raised when the dialog should close (e.g. after successful auto-run cleanup). Subscribers typically close the window.</summary>
        public event Action? RequestCloseDialog;

        #endregion

        // =========================================================
        // 4. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        /// <summary>
        /// Initializes the VM with factory and services, loads the selected browser from config,
        /// and calls <see cref="Initialize"/> with the parsed browser type so paths and display name are ready.
        /// If the config value is not a valid <see cref="BrowserType"/>, sets an error message instead.
        /// </summary>
        public ViewModelDeleteBrowserContent(
            IBrowserFactory browserFactory,
            IWindowsProcessControlService processService,
            IFileDeletionService fileDeletionService,
            IEVisitorConfigService eVisitorConfigService,
            IDialogService dialogService,
            ILocalizationService localizationService)
        {
            _browserFactory = browserFactory;
            _eVisitorConfigService = eVisitorConfigService;
            _processService = processService;
            _fileDeletionService = fileDeletionService;
            _dialogService = dialogService;
            _localizationService = localizationService;

            BrowserName = _localizationService.GetString("Cleanup_Loading");
            StatusText = _localizationService.GetString("Cleanup_Ready");

            AppConfig appConfig = eVisitorConfigService.LoadConfig();
            string selectedBrowserString = appConfig.Browser.Selected;

            if (Enum.TryParse(typeof(BrowserType), selectedBrowserString, true, out var parsedResult))
            {
                var browserType = (BrowserType)parsedResult;
                Initialize(browserType);
            }
            else
            {
                string errorFormat = _localizationService.GetString("Cleanup_ConfigError");
                StatusText = string.Format(errorFormat, selectedBrowserString);
            }
        }

        #endregion

        // =========================================================
        // 5. COMMANDS (MVVM Actions)
        // =========================================================
        #region Commands

        /// <summary>Cancels the current cleanup run and sets status to "Canceling".</summary>
        [RelayCommand(CanExecute = nameof(CanCancel))]
        private void CancelCleaning()
        {
            _cts?.Cancel();
            StatusText = _localizationService.GetString("Cleanup_Canceling");
        }

        /// <summary>
        /// Starts cleanup if not busy and browser paths are loaded. If the browser process is still running,
        /// sets <see cref="IsProcessConflict"/> and asks the user to close it or force-close; otherwise runs deletion.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanClean))]
        private async Task StartCleaning()
        {
            if (IsBusy || _currentBrowser == null || _browserPaths == null) return;
            if (_processService.IsProcessAlive(_processName))
            {
                IsProcessConflict = true;
                StatusText = _localizationService.GetString("Cleanup_BrowserRunning");
                return;
            }
            await ExecuteCleaningLogic();
        }

        /// <summary>Force-closes the browser process, waits briefly, then runs cleanup if the process is gone.</summary>
        [RelayCommand]
        private async Task ForceCloseAndContinue()
        {
            IsProcessConflict = false;
            _processService.CloseApplication(_processName);
            StatusText = _localizationService.GetString("Cleanup_ClosingBrowser");
            await Task.Delay(1000);
            if (_processService.IsProcessAlive(_processName))
            {
                StatusText = _localizationService.GetString("Cleanup_CloseFailed");
                return;
            }
            await ExecuteCleaningLogic();
        }

        /// <summary>Dismisses the process-conflict state and sets status to user-canceled.</summary>
        [RelayCommand]
        private void CancelConflict()
        {
            IsProcessConflict = false;
            StatusText = _localizationService.GetString("Cleanup_CanceledByUser");
        }

        #endregion

        // =========================================================
        // 6. PUBLIC & PROTECTED METHODS (API)
        // =========================================================
        #region PublicAndProtectedMethods

        /// <summary>
        /// Used when the dialog is opened in auto mode (e.g. from the restart task). Sets internal flag
        /// so that on successful cleanup the dialog can request to close itself.
        /// </summary>
        public async Task RunAutoSequenceAsync()
        {
            _isAutoMode = true;
            await Task.Delay(500);
            await StartCleaning();
        }

        /// <summary>
        /// Loads browser instance and paths for the given type, and sets display name and icon.
        /// On failure (e.g. browser not found), sets a localized error message in StatusText.
        /// </summary>
        /// <param name="selectedBrowserType">Which browser to clean (Chrome, Firefox, Edge, Brave).</param>
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

        #endregion

        // =========================================================
        // 7. PRIVATE HELPER METHODS (Interne Hilfsmethoden)
        // =========================================================
        #region PrivateHelperMethods

        private bool CanCancel() => IsBusy;
        private bool CanClean() => !IsBusy && (IsDeleteCookiesChecked || IsDeleteInternetCacheChecked);

        /// <summary>
        /// Builds the list of directories to delete from cache/cookie paths based on checkboxes,
        /// counts files, then runs <see cref="IFileDeletionService.DeleteFilesAsync"/> with progress.
        /// In auto mode, invokes RequestCloseDialog after a short delay on success.
        /// </summary>
        private async Task ExecuteCleaningLogic()
        {
            var directoriesToDelete = new List<string>();
            if (IsDeleteInternetCacheChecked && _browserPaths!.CacheDirs != null && _browserPaths.CacheDirs.Count > 0)
                directoriesToDelete.AddRange(_browserPaths.CacheDirs);
            if (IsDeleteCookiesChecked && _browserPaths!.CookiesDirs != null && _browserPaths.CookiesDirs.Count > 0)
                directoriesToDelete.AddRange(_browserPaths.CookiesDirs);

            if (directoriesToDelete.Count == 0)
            {
                StatusText = _localizationService.GetString("Cleanup_NoPaths");
                return;
            }

            IsBusy = true;
            _cts = new CancellationTokenSource();
            bool success = false;

            try
            {
                StatusText = _localizationService.GetString("Cleanup_Analyzing");
                int totalFiles = await _fileDeletionService.CountFilesAsync(directoriesToDelete);
                ProgressMaximum = totalFiles > 0 ? totalFiles : 1;
                ProgressValue = 0;

                var statusProgress = new Progress<string>(statusMessage => StatusText = statusMessage);
                var valueProgress = new Progress<int>(completedFileCount =>
                {
                    ProgressValue = completedFileCount;
                    if (totalFiles > 0) ProgressText = $"{(completedFileCount * 100 / totalFiles)} %";
                });

                await _fileDeletionService.DeleteFilesAsync(directoriesToDelete, statusProgress, valueProgress, _cts.Token);
                StatusText = _localizationService.GetString("Cleanup_Finished");
                success = true;
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
                if (_isAutoMode && success && !IsProcessConflict)
                {
                    await Task.Delay(1000);
                    RequestCloseDialog?.Invoke();
                }
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
