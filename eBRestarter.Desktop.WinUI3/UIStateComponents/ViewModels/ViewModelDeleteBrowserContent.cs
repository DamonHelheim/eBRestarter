using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using eBRestarter.Core.Application.Common.TypedError;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the "Delete browser content" (cache/cookies cleanup) dialog. Resolves the
/// selected browser via <see cref="IOutboundPortBrowserFactory"/>, collects paths from the browser implementation,
/// and runs cleanup through <see cref="IUseCaseDeleteBrowserContent"/> with progress reporting.
/// Can be run manually or as an auto-step from the restart task when cleanup is due.
/// </summary>
public sealed partial class ViewModelDeleteBrowserContent : ObservableObject
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const int AutoCloseAfterSuccessDelayMilliseconds = 1000;
    private const int AutoSequenceStartupDelayMilliseconds = 500;
    private const int DefaultProgressMaximumValue = 100;
    private const string InitialProgressTextDisplay = "0 %";
    private const string KeyCleanupCanceledByUser = "Cleanup_CanceledByUser";
    private const string KeyCleanupCanceling = "Cleanup_Canceling";
    private const string KeyCleanupConfigError = "Cleanup_ConfigError";
    private const string KeyCleanupLoading = "Cleanup_Loading";
    private const string KeyCleanupReady = "Cleanup_Ready";
    private const string KeyGeneralErrorPrefix = "General_ErrorPrefix";
    private const string KeyGeneralLoadErrorPrefix = "General_LoadErrorPrefix";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies (alphabetical A–Z) ──
    private readonly IOutboundPortBrowserFactory _browserFactory;
    private readonly ILogger<ViewModelDeleteBrowserContent> _logger;
    private readonly IUseCaseDeleteBrowserContent _deleteBrowserContentUseCase;
    private readonly IOutboundPortEVisitorConfigRepository _evRestarterConfigRepository;
    private readonly IInboundPortLocalizationProvider _localizationService;

    // ── Block 2: Primitives / Primitive wrappers (alphabetical A–Z) ──
    private bool _isAutoMode;

    // ── Block 3: Enums (alphabetical A–Z) ──
    private BrowserType _selectedBrowserType;

    // ── Block 4: Complex types / Repositories / Objects (alphabetical A–Z) ──
    private CancellationTokenSource? _deleteBrowserContentCancellationTokenSource;


    // ═══════════════════════════════════════════════════════
    //  3. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes the VM with factory and services, loads the selected browser from config,
    /// and calls <see cref="Initialize"/> with the parsed browser type so paths and display name are ready.
    /// If the config value is not a valid <see cref="BrowserType"/>, sets an error message instead.
    /// </summary>
    public ViewModelDeleteBrowserContent(
        IUseCaseDeleteBrowserContent deleteBrowserContentUseCase,
        IOutboundPortBrowserFactory browserFactory,
        IOutboundPortEVisitorConfigRepository evRestarterConfigRepository,
        IInboundPortLocalizationProvider localizationService,
        ILogger<ViewModelDeleteBrowserContent> logger)
    {
        ArgumentNullException.ThrowIfNull(deleteBrowserContentUseCase);
        ArgumentNullException.ThrowIfNull(browserFactory);
        ArgumentNullException.ThrowIfNull(evRestarterConfigRepository);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _deleteBrowserContentUseCase = deleteBrowserContentUseCase;
        _browserFactory = browserFactory;
        _evRestarterConfigRepository = evRestarterConfigRepository;
        _localizationService = localizationService;

        BrowserIconPath = string.Empty;
        BrowserName = _localizationService.RetrieveString(KeyCleanupLoading);
        ProgressText = InitialProgressTextDisplay;
        StatusText = _localizationService.RetrieveString(KeyCleanupReady);
        ProgressMaximum = DefaultProgressMaximumValue;
        ProgressValue = 0;
        IsDeleteCookiesChecked = true;
        IsDeleteInternetCacheChecked = true;

        AppConfig appConfig = _evRestarterConfigRepository.LoadConfig();
        string selectedBrowserString = appConfig?.Browser?.Selected ?? string.Empty;

        if (Enum.TryParse<BrowserType>(selectedBrowserString, ignoreCase: true, out BrowserType parsedBrowserType))
        {
            Initialize(parsedBrowserType);
        }
        else
        {
            string errorFormat = _localizationService.RetrieveString(KeyCleanupConfigError);
            StatusText = string.Format(errorFormat, selectedBrowserString);
        }
    }


    // ═══════════════════════════════════════════════════════
    //  5. Events
    // ═══════════════════════════════════════════════════════
    /// <summary>Raised when the dialog should close (e.g. after successful auto-run cleanup). Subscribers typically close the window.</summary>
    public event Action? RequestCloseDialog;


    // ═══════════════════════════════════════════════════════
    //  6. Properties
    // ═══════════════════════════════════════════════════════
    /// <summary>Gets or sets the browser icon asset file path.</summary>
    [ObservableProperty]
    public partial string BrowserIconPath { get; set; }

    /// <summary>Gets or sets the browser display name.</summary>
    [ObservableProperty]
    public partial string BrowserName { get; set; }

    /// <summary>Gets or sets a value indicating whether a cleanup operation is currently active.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCleaningCommand))]
    public partial bool IsBusy { get; set; }

    /// <summary>Gets or sets a value indicating whether cookies should be deleted.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
    public partial bool IsDeleteCookiesChecked { get; set; }

    /// <summary>Gets or sets a value indicating whether internet cache should be deleted.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCleaningCommand))]
    public partial bool IsDeleteInternetCacheChecked { get; set; }

    /// <summary>Gets or sets a value indicating whether a running browser process blocks cleanup.</summary>
    [ObservableProperty]
    public partial bool IsProcessConflict { get; set; }

    /// <summary>Gets or sets the maximum value for the cleanup progress bar.</summary>
    [ObservableProperty]
    public partial double ProgressMaximum { get; set; }

    /// <summary>Gets or sets the progress percentage display text.</summary>
    [ObservableProperty]
    public partial string ProgressText { get; set; }

    /// <summary>Gets or sets the current value for the cleanup progress bar.</summary>
    [ObservableProperty]
    public partial double ProgressValue { get; set; }

    /// <summary>Gets or sets the status feedback text.</summary>
    [ObservableProperty]
    public partial string StatusText { get; set; }


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>Cancels the current cleanup run and sets status to "Canceling".</summary>
    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void CancelCleaning()
    {
        _deleteBrowserContentCancellationTokenSource?.Cancel();
        StatusText = _localizationService.RetrieveString(KeyCleanupCanceling);
    }

    /// <summary>Dismisses the process-conflict state and sets status to user-canceled.</summary>
    [RelayCommand]
    private void CancelConflict()
    {
        IsProcessConflict = false;
        StatusText = _localizationService.RetrieveString(KeyCleanupCanceledByUser);
    }

    /// <summary>
    /// Gets a value indicating whether the cleanup operation can be cancelled.
    /// </summary>
    /// <returns><see langword="true"/> if a cleanup operation is currently active; otherwise, <see langword="false"/>.</returns>
    private bool CanCancel() => IsBusy;

    /// <summary>
    /// Gets a value indicating whether browser cleanup can be initiated.
    /// </summary>
    /// <returns><see langword="true"/> if not busy and at least one deletion target is selected; otherwise, <see langword="false"/>.</returns>
    private bool CanClean() => !IsBusy && (IsDeleteCookiesChecked || IsDeleteInternetCacheChecked);

    /// <summary>
    /// Builds the list of directories to delete from cache/cookie paths based on checkboxes,
    /// and runs the use case with progress.
    /// In auto mode, invokes RequestCloseDialog after a short delay on success.
    /// </summary>
    private async Task ExecuteCleaningLogicAsync(bool forceClose)
    {
        IsBusy = true;
        _deleteBrowserContentCancellationTokenSource?.Dispose();
        _deleteBrowserContentCancellationTokenSource = new CancellationTokenSource();

        try
        {
            var request = new DeleteBrowserContentRequest(
                BrowserType: _selectedBrowserType,
                DeleteCookies: IsDeleteCookiesChecked,
                DeleteCache: IsDeleteInternetCacheChecked,
                ForceCloseProcess: forceClose
            );

            var progress = new Progress<DeleteBrowserContentProgress>(cleanupProgress =>
            {
                StatusText = cleanupProgress.StatusMessage;

                // Each progress step corresponds to a directory rather than an individual file,
                // eliminating the initial cache tree traversal phase.
                ProgressMaximum = cleanupProgress.TotalSteps > 0 ? cleanupProgress.TotalSteps : 1;
                ProgressValue = cleanupProgress.CompletedSteps;

                if (cleanupProgress.TotalSteps > 0)
                {
                    ProgressText = $"{cleanupProgress.CompletedSteps * 100 / cleanupProgress.TotalSteps} %";
                }
            });

            var result = await _deleteBrowserContentUseCase.ExecuteAsync(
                request,
                progress,
                _deleteBrowserContentCancellationTokenSource.Token);

            if (result.HasError<ProcessConflictError>())
            {
                IsProcessConflict = true;
            }
            else if (result.IsFailed)
            {
                StatusText = result.Errors[0].Message;
            }
            else if (_isAutoMode)
            {
                await Task.Delay(AutoCloseAfterSuccessDelayMilliseconds);
                RequestCloseDialog?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            StatusText = _localizationService.RetrieveString(KeyCleanupCanceledByUser);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Logging guideline: Exception type and stack trace are logged here to prevent silent failures.
            _logger.LogError(
                LogEventIds.Browser.BrowserCacheFileDeletionFailed,
                ex,
                "Browser cleanup failed with a file system error.");

            string errorFormat = _localizationService.RetrieveString(KeyGeneralErrorPrefix);
            StatusText = string.Format(errorFormat, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                LogEventIds.Browser.BrowserCacheFileDeletionFailed,
                ex,
                "Browser cleanup failed unexpectedly.");

            string errorFormat = _localizationService.RetrieveString(KeyGeneralErrorPrefix);
            StatusText = string.Format(errorFormat, ex.Message);
        }
        finally
        {
            IsBusy = false;
            _deleteBrowserContentCancellationTokenSource?.Dispose();
            _deleteBrowserContentCancellationTokenSource = null;
        }
    }

    /// <summary>Force-closes the browser process, waits briefly, then runs cleanup if the process is gone.</summary>
    [RelayCommand]
    private async Task ForceCloseAndContinueAsync()
    {
        IsProcessConflict = false;
        await ExecuteCleaningLogicAsync(true);
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
            _logger.LogError(
                LogEventIds.UserInterface.ViewModelOperationFailed,
                ex,
                "Loading the browser cleanup dialog state failed.");

            string errorFormat = _localizationService.RetrieveString(KeyGeneralLoadErrorPrefix);
            StatusText = string.Format(errorFormat, ex.Message);
        }
    }

    /// <summary>
    /// Used when the dialog is opened in auto mode (e.g. from the restart task). Sets internal flag
    /// so that on successful cleanup the dialog can request to close itself.
    /// </summary>
    public async Task RunAutoSequenceAsync()
    {
        _isAutoMode = true;
        await Task.Delay(AutoSequenceStartupDelayMilliseconds);
        await StartCleaningAsync();
    }

    /// <summary>
    /// Starts cleanup if not busy and browser paths are loaded. If the browser process is still running,
    /// sets <see cref="IsProcessConflict"/> and asks the user to close it or force-close; otherwise runs deletion.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanClean))]
    private async Task StartCleaningAsync()
    {
        if (IsBusy)
        {
            return;
        }

        await ExecuteCleaningLogicAsync(false);
    }
}
