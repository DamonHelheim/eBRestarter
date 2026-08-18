using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.BehavioralComponents.Handlers.Interfaces;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Services;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Validators;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Logging;
using eBRestarter.Core.Domain.Handlers;

namespace eBRestarter.Core.Application.BehavioralComponents.Services;

/// <summary>
/// Orchestrates the execution cycle of browser sessions, delays, progress tracking, and scheduled cleanups.
/// </summary>
/// <param name="browserCleanupScheduleHandler">The handler used to evaluate and calculate browser cache cleanup schedules.</param>
/// <param name="browserFactory">The factory for creating target browser instances.</param>
/// <param name="configService">The repository provider for application configuration.</param>
/// <param name="delayPhaseHandler">The handler executing timed delay and cooldown phases.</param>
/// <param name="localizationService">The provider for localized user interface string resources.</param>
/// <param name="logger">The application logger for recording restarter cycle events.</param>
/// <param name="runBrowserPhaseHandler">The handler managing the active browser runtime phase.</param>
/// <param name="timeProvider">The time provider for time abstraction and delay timing.</param>
/// <param name="validator">The validator for cycle request parameters.</param>
public sealed class RestarterCycleService(
    IBrowserCleanupScheduleHandler browserCleanupScheduleHandler,
    IOutboundPortBrowserFactory browserFactory,
    IOutboundPortEVisitorConfigRepository configService,
    IDelayPhaseHandler delayPhaseHandler,
    IInboundPortLocalizationProvider localizationService,
    IOutboundPortApplicationLogger<RestarterCycleService> logger,
    IRunBrowserPhaseHandler runBrowserPhaseHandler,
    TimeProvider timeProvider,
    IInboundPortApplicationValidator<ManageRestarterCycleRequest> validator) : IInboundPortRestarterCycleService
{
    private const string BaseUrl = "https://www.ebesucher.de/surfbar/";
    private const int CleanupDelayMilliseconds = 1000;
    private const string BrowserStartFailedMessage = "Der Browser konnte nicht gestartet werden.";
    private const string BrowserStartRetryFormat = "Browser nicht startbar. Neuer Versuch in {0}...";

    private const string CycleScopePropertyName = "RestarterCycleId";
    private const int EmergencyCooldownSeconds = 5;
    private const int InitialDelaySeconds = 5;
    private const string LocalizationKeyErrorPrefix = "General_ErrorPrefix";
    private const string LocalizationKeyStatusRestartIn = "Task_StatusRestartIn";
    private const string LocalizationKeyStatusRunning = "Task_StatusRunning";
    private const string LocalizationKeyStatusStartIn = "Task_StatusStartIn";
    private const string LocalizationKeyStatusStopped = "Task_StatusStopped";
    private const int SecondsPerHour = 3600;

    private readonly IBrowserCleanupScheduleHandler _browserCleanupScheduleHandler = browserCleanupScheduleHandler ?? throw new ArgumentNullException(nameof(browserCleanupScheduleHandler));
    private readonly IOutboundPortBrowserFactory _browserFactory = browserFactory ?? throw new ArgumentNullException(nameof(browserFactory));
    private readonly IOutboundPortEVisitorConfigRepository _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    private readonly IDelayPhaseHandler _delayPhaseHandler = delayPhaseHandler ?? throw new ArgumentNullException(nameof(delayPhaseHandler));
    private readonly IInboundPortLocalizationProvider _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    private readonly IOutboundPortApplicationLogger<RestarterCycleService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IRunBrowserPhaseHandler _runBrowserPhaseHandler = runBrowserPhaseHandler ?? throw new ArgumentNullException(nameof(runBrowserPhaseHandler));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly IInboundPortApplicationValidator<ManageRestarterCycleRequest> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    private CancellationTokenSource? _cts;
    private IOutboundPortBrowser? _currentBrowser;

    // CompositeFormat avoids boxing value type parameters and avoids re-parsing format strings on every progress tick.
    private CompositeFormat? _statusRestartInFormat;
    private CompositeFormat? _statusStartInFormat;

    private readonly Lock _syncLock = new();

    /// <inheritdoc />
    public event EventHandler<RestarterCycleProgress>? ProgressChanged;

    /// <inheritdoc />
    public async Task StartAsync(
        ManageRestarterCycleRequest request,
        Func<Task> performCleanupCallback)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(performCleanupCallback);

        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            ReportProgress(RestartTaskState.Idle, 0, validationResult.Errors[0].ErrorMessage);
            return;
        }

        Stop();

        CancellationTokenSource cts;
        lock (_syncLock)
        {
            _cts = new CancellationTokenSource();
            cts = _cts;
        }

        var cancellationToken = cts.Token;

        // Attaches a correlation ID to all log entries produced during this restarter cycle execution.
        using var cycleScope = _logger.BeginScope(CycleScopePropertyName, Guid.NewGuid().ToString("N"));

        try
        {
            await RunCycleAsync(request, performCleanupCallback, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Expected cancellation via Stop(); logged at debug level.
            _logger.LogDebug(
                LogEventIds.RestarterCycle.RestartSchedulerStopped,
                "Restarter cycle was cancelled by an explicit stop request.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                LogEventIds.RestarterCycle.CycleFaulted,
                ex,
                "The restarter cycle aborted with an unhandled fault.");
            ReportProgress(RestartTaskState.Idle, 0, $"Fehler: {ex.Message}");
        }
        finally
        {
            CloseCurrentBrowser();
            ReportProgress(RestartTaskState.Idle, 0);
        }
    }

    /// <inheritdoc />
    public void Stop()
    {
        CancellationTokenSource? cts;
        lock (_syncLock)
        {
            cts = _cts;
            _cts = null;
        }

        if (cts is not null)
        {
            // Chain CTS disposal after cancellation completes to prevent a race condition with pending cancellation callbacks without blocking the caller thread.
            _ = cts.CancelAsync().ContinueWith(
                _ => cts.Dispose(),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Runs the main restarter cycle execution loop including browser phases, delays, and scheduled cleanups.
    /// </summary>
    private async Task RunCycleAsync(
        ManageRestarterCycleRequest request,
        Func<Task> performCleanupCallback,
        CancellationToken cancellationToken)
    {
        await _delayPhaseHandler.ExecuteAsync(RestartTaskState.InitialDelay, InitialDelaySeconds, ReportProgress, cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            var appConfig = _configService.LoadConfig();

            request = request with
            {
                BrowserType = appConfig.Browser != null
                    && !string.IsNullOrWhiteSpace(appConfig.Browser.Selected)
                    && Enum.TryParse<BrowserType>(appConfig.Browser.Selected, true, out var parsedType)
                        ? parsedType
                        : request.BrowserType,
                Username = string.IsNullOrWhiteSpace(appConfig.Username) ? request.Username : appConfig.Username,
                RuntimeSeconds = appConfig.Browser is not null ? appConfig.Browser.RuntimeHours * SecondsPerHour : request.RuntimeSeconds,
                PauseSeconds = appConfig.Browser?.RuntimePauseSeconds ?? request.PauseSeconds,
                CheckBrowserAliveRoutine = appConfig.Browser?.CheckBrowserAliveRoutine ?? request.CheckBrowserAliveRoutine
            };

            if (!LaunchBrowser(request))
            {
                // If browser launch fails, retry after a short cooldown to allow browser installation/recovery without hanging in the runtime phase.
                await _delayPhaseHandler.ExecuteAsync(
                    RestartTaskState.Cooldown,
                    EmergencyCooldownSeconds,
                    (state, secondsRemaining, _) => ReportProgress(
                        state,
                        secondsRemaining,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            BrowserStartRetryFormat,
                            secondsRemaining)),
                    cancellationToken);

                continue;
            }

            var runPhaseResult = await _runBrowserPhaseHandler.ExecuteAsync(request, _currentBrowser, ReportProgress, cancellationToken);

            if (runPhaseResult == BrowserPhaseResult.BrowserClosed)
            {
                CloseCurrentBrowser();

                await _delayPhaseHandler.ExecuteAsync(
                    RestartTaskState.Cooldown,
                    EmergencyCooldownSeconds,
                    (state, secondsRemaining, _) =>
                    {
                        string msg = $"Der Browser wurde geschlossen, Starte Restarter in {secondsRemaining}...";
                        ReportProgress(state, secondsRemaining, msg);
                    },
                    cancellationToken);

                continue;
            }

            await CheckAndExecuteBrowserCleanupAsync(performCleanupCallback, cancellationToken);
            CloseCurrentBrowser();
            await _delayPhaseHandler.ExecuteAsync(RestartTaskState.Cooldown, request.PauseSeconds, ReportProgress, cancellationToken);
        }
    }

    /// <summary>
    /// Creates and starts the target browser instance for the configured surfbar session.
    /// </summary>
    /// <remarks>
    /// The username is percent-encoded before constructing the surfbar URL to prevent command-line argument injection into the browser process.
    /// </remarks>
    private bool LaunchBrowser(ManageRestarterCycleRequest request)
    {
        var browserType = request.BrowserType;

        _currentBrowser = _browserFactory.Create(browserType);
        string url = $"{BaseUrl}{Uri.EscapeDataString(request.Username)}";

        if (_currentBrowser.Start(url))
        {
            return true;
        }


        var errorFormat = _localizationService.RetrieveString(LocalizationKeyErrorPrefix);
        var errorMsg = string.Format(CultureInfo.CurrentCulture, errorFormat, BrowserStartFailedMessage);

        ReportProgress(RestartTaskState.Idle, 0, errorMsg);

        CloseCurrentBrowser();

        return false;
    }

    /// <summary>
    /// Closes the currently active browser instance if one exists and resets the field reference.
    /// </summary>
    private void CloseCurrentBrowser()
    {
        _currentBrowser?.Close();
        _currentBrowser = null;
    }

    /// <summary>
    /// Evaluates if a browser cache cleanup is due and executes the cleanup delegate and schedule updates.
    /// </summary>
    private async Task CheckAndExecuteBrowserCleanupAsync(
        Func<Task> performCleanupCallback,
        CancellationToken cancellationToken)
    {
        var appConfig = _configService.LoadConfig();
        var browser = appConfig.Browser;

        if (browser is null || !_browserCleanupScheduleHandler.ShouldRunCleanupNow(browser.DeleteBrowserCacheIntervalDays, browser.NextBrowserDeleteCacheDate))
            return;

        CloseCurrentBrowser();

        await Task.Delay(TimeSpan.FromMilliseconds(CleanupDelayMilliseconds), _timeProvider, cancellationToken);

        await performCleanupCallback();

        var today = _timeProvider.GetLocalNow().Date;
        var newDate = _browserCleanupScheduleHandler.CalculateNextCleanupDateAfterRun(today, appConfig.Browser.DeleteBrowserCacheIntervalDays);

        appConfig.Browser.SetNextCleanupDate(newDate);
        _configService.SaveConfig(appConfig);
    }

    /// <summary>
    /// Raises the <see cref="ProgressChanged"/> event with formatted status messages.
    /// </summary>
    private void ReportProgress(RestartTaskState state, int secondsRemaining, string? customMessage = null)
    {
        string statusMessage = customMessage ?? RetrieveStatusMessageForState(state, secondsRemaining);

        ProgressChanged?.Invoke(this, new RestarterCycleProgress(state, secondsRemaining, statusMessage));
    }

    /// <summary>
    /// Retrieves localized and formatted status message string for a given restart task state.
    /// </summary>
    private string RetrieveStatusMessageForState(RestartTaskState state, int secondsRemaining)
    {
        return state switch
        {
            RestartTaskState.Idle => _localizationService.RetrieveString(LocalizationKeyStatusStopped),
            RestartTaskState.InitialDelay => string.Format(
                CultureInfo.CurrentCulture,
                _statusStartInFormat ??= CompositeFormat.Parse(_localizationService.RetrieveString(LocalizationKeyStatusStartIn)),
                secondsRemaining),
            RestartTaskState.Running => _localizationService.RetrieveString(LocalizationKeyStatusRunning),
            RestartTaskState.Cooldown => string.Format(
                CultureInfo.CurrentCulture,
                _statusRestartInFormat ??= CompositeFormat.Parse(_localizationService.RetrieveString(LocalizationKeyStatusRestartIn)),
                secondsRemaining),
            _ => string.Empty
        };
    }
}