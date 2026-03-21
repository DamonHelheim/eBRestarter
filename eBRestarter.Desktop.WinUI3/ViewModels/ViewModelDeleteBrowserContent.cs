using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.UseCases.DeleteBrowserContent;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models.Records.Config;
using System;
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

        private readonly IDeleteBrowserContentUseCase _deleteBrowserContentUseCase;
        private readonly IBrowserFactory _browserFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IEVisitorConfigService _eVisitorConfigService;

        private CancellationTokenSource? _cts;
        private bool _isAutoMode = false;
        private BrowserType _selectedBrowserType;

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
            IDeleteBrowserContentUseCase deleteBrowserContentUseCase,
            IBrowserFactory browserFactory,
            IEVisitorConfigService eVisitorConfigService,
            ILocalizationService localizationService)
        {
            _deleteBrowserContentUseCase = deleteBrowserContentUseCase;
            _browserFactory = browserFactory;
            _eVisitorConfigService = eVisitorConfigService;
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
            if (IsBusy) return;
            await ExecuteCleaningLogic(false);
        }

        /// <summary>Force-closes the browser process, waits briefly, then runs cleanup if the process is gone.</summary>
        [RelayCommand]
        private async Task ForceCloseAndContinue()
        {
            IsProcessConflict = false;
            await ExecuteCleaningLogic(true);
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
                _selectedBrowserType = selectedBrowserType;
                var currentBrowser = _browserFactory.Create(selectedBrowserType);
                BrowserName = currentBrowser.DisplayName;
                BrowserIconPath = currentBrowser.IconPath;
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
        private async Task ExecuteCleaningLogic(bool forceClose)
        {
            IsBusy = true;
            _cts = new CancellationTokenSource();

            try
            {
                var request = new DeleteBrowserContentRequest(
                    BrowserType: _selectedBrowserType,
                    DeleteCookies: IsDeleteCookiesChecked,
                    DeleteCache: IsDeleteInternetCacheChecked,
                    ForceCloseProcess: forceClose
                );

                var progress = new Progress<DeleteBrowserContentProgress>(p =>
                {
                    StatusText = p.StatusMessage;
                    ProgressMaximum = p.TotalFiles > 0 ? p.TotalFiles : 1;
                    ProgressValue = p.CurrentFile;
                    if (p.TotalFiles > 0)
                    {
                        ProgressText = $"{(p.CurrentFile * 100 / p.TotalFiles)} %";
                    }
                });

                var response = await _deleteBrowserContentUseCase.ExecuteAsync(request, progress, _cts.Token);

                if (response.ProcessConflict)
                {
                    IsProcessConflict = true;
                }
                else if (!response.Success)
                {
                    StatusText = response.ErrorMessage;
                }
                else
                {
                    if (_isAutoMode)
                    {
                        await Task.Delay(1000);
                        RequestCloseDialog?.Invoke();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                StatusText = _localizationService.GetString("Cleanup_CanceledByUser");
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
    }
}
