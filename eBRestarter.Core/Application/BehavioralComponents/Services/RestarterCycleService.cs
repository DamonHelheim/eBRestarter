using System;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.BehavioralComponents.Handlers.Interfaces;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Services;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Validators;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Domain.Handlers;

namespace eBRestarter.Core.Application.BehavioralComponents.Services;

/// <summary>
/// Orchestrates the execution cycle of browser sessions, delays, progress tracking, and scheduled cleanups.
/// </summary>
public sealed class RestarterCycleService(
    IBrowserCleanupScheduleHandler browserCleanupScheduleHandler,
    IOutboundPortBrowserFactory browserFactory,
    IOutboundPortEVisitorConfigRepository configService,
    IDelayPhaseHandler delayPhaseHandler,
    IInboundPortLocalizationProvider localizationService,
    IRunBrowserPhaseHandler runBrowserPhaseHandler,
    TimeProvider timeProvider,
    IInboundPortApplicationValidator<ManageRestarterCycleRequest> validator) : IInboundPortRestarterCycleService
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const string BaseUrl = "https://www.ebesucher.de/surfbar/";

    private const int CleanupDelayMilliseconds = 1000;
    private const int EmergencyCooldownSeconds = 5;
    private const int InitialDelaySeconds = 5;
    private const int SecondsPerHour = 3600;

    private const string LocalizationKeyErrorPrefix = "General_ErrorPrefix";
    private const string LocalizationKeyStatusRestartIn = "Task_StatusRestartIn";
    private const string LocalizationKeyStatusRunning = "Task_StatusRunning";
    private const string LocalizationKeyStatusStartIn = "Task_StatusStartIn";
    private const string LocalizationKeyStatusStopped = "Task_StatusStopped";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch A–Z) ──
    private readonly IBrowserCleanupScheduleHandler _browserCleanupScheduleHandler = browserCleanupScheduleHandler ?? throw new ArgumentNullException(nameof(browserCleanupScheduleHandler));
    private readonly IOutboundPortBrowserFactory _browserFactory = browserFactory ?? throw new ArgumentNullException(nameof(browserFactory));
    private readonly IOutboundPortEVisitorConfigRepository _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    private readonly IDelayPhaseHandler _delayPhaseHandler = delayPhaseHandler ?? throw new ArgumentNullException(nameof(delayPhaseHandler));
    private readonly IInboundPortLocalizationProvider _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    private readonly IRunBrowserPhaseHandler _runBrowserPhaseHandler = runBrowserPhaseHandler ?? throw new ArgumentNullException(nameof(runBrowserPhaseHandler));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly IInboundPortApplicationValidator<ManageRestarterCycleRequest> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    // ── Block 4: Komplexe Typen (alphabetisch A–Z) ──
    private CancellationTokenSource? _cts;
    private IOutboundPortBrowser? _currentBrowser;
    private readonly System.Threading.Lock _syncLock = new();


    // ═══════════════════════════════════════════════════════
    //  5. Events & Delegates
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Occurs when the restarter cycle progress or state changes.
    /// </summary>
    public event EventHandler<RestarterCycleProgress>? ProgressChanged;


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Starts the restarter cycle execution loop asynchronously.
    /// </summary>
    /// <param name="request">The cycle configuration request parameters.</param>
    /// <param name="performCleanupCallback">Callback delegate invoked when browser cache cleanup is due.</param>
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

        try
        {
            await RunCycleAsync(request, performCleanupCallback, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Expected when stopped
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            ReportProgress(RestartTaskState.Idle, 0, $"Fehler: {ex.Message}");
        }
        finally
        {
            CloseCurrentBrowser();
            ReportProgress(RestartTaskState.Idle, 0);
        }
    }

    /// <summary>
    /// Stops the currently active restarter cycle execution loop.
    /// </summary>
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
            _ = cts.CancelAsync();
            cts.Dispose();
        }
    }

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
                BrowserType = appConfig.Browser != null && !string.IsNullOrWhiteSpace(appConfig.Browser.Selected) && Enum.TryParse<BrowserType>(appConfig.Browser.Selected, true, out var parsedType) ? parsedType : request.BrowserType,
                Username = string.IsNullOrWhiteSpace(appConfig.Username) ? request.Username : appConfig.Username,
                RuntimeSeconds = appConfig.Browser is not null ? appConfig.Browser.RuntimeHours * SecondsPerHour : request.RuntimeSeconds,
                PauseSeconds = appConfig.Browser?.RuntimePauseSeconds ?? request.PauseSeconds,
                CheckBrowserAliveRoutine = appConfig.Browser?.CheckBrowserAliveRoutine ?? request.CheckBrowserAliveRoutine
            };

            LaunchBrowser(request);

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

    private void LaunchBrowser(ManageRestarterCycleRequest request)
    {
        try
        {
            var browserType = request.BrowserType;

            _currentBrowser = _browserFactory.Create(browserType);
            string url = $"{BaseUrl}{request.Username}";

            _currentBrowser.Start(url);
        }
        catch (Exception ex)
        {
            var errorFormat = _localizationService.RetrieveString(LocalizationKeyErrorPrefix);
            var errorMsg = string.Format(errorFormat, ex.Message);

            ReportProgress(RestartTaskState.Idle, 0, errorMsg);

            throw;
        }
    }

    private void CloseCurrentBrowser()
    {
        _currentBrowser?.Close();
        _currentBrowser = null;
    }

    private async Task CheckAndExecuteBrowserCleanupAsync(
        Func<Task> performCleanupCallback,
        CancellationToken cancellationToken)
    {
        var appConfig = _configService.LoadConfig();
        var browser = appConfig.Browser;

        if (browser is null || !_browserCleanupScheduleHandler.ShouldRunCleanupNow(browser.DeleteBrowserCacheIntervalDays, browser.NextBrowserDeleteCacheDate))
        {
            return;
        }

        CloseCurrentBrowser();

        await Task.Delay(TimeSpan.FromMilliseconds(CleanupDelayMilliseconds), _timeProvider, cancellationToken);

        await performCleanupCallback();

        var today = _timeProvider.GetLocalNow().Date;
        var newDate = _browserCleanupScheduleHandler.CalculateNextCleanupDateAfterRun(today, appConfig.Browser.DeleteBrowserCacheIntervalDays);

        appConfig.Browser.SetNextCleanupDate(newDate);
        _configService.SaveConfig(appConfig);
    }

    private void ReportProgress(RestartTaskState state, int secondsRemaining, string? customMessage = null)
    {
        string statusMessage = customMessage ?? RetrieveStatusMessageForState(state, secondsRemaining);

        ProgressChanged?.Invoke(this, new RestarterCycleProgress(state, secondsRemaining, statusMessage));
    }

    private string RetrieveStatusMessageForState(RestartTaskState state, int secondsRemaining)
    {
        return state switch
        {
            RestartTaskState.Idle => _localizationService.RetrieveString(LocalizationKeyStatusStopped),
            RestartTaskState.InitialDelay => string.Format(_localizationService.RetrieveString(LocalizationKeyStatusStartIn), secondsRemaining),
            RestartTaskState.Running => _localizationService.RetrieveString(LocalizationKeyStatusRunning),
            RestartTaskState.Cooldown => string.Format(_localizationService.RetrieveString(LocalizationKeyStatusRestartIn), secondsRemaining),
            _ => string.Empty
        };
    }
}