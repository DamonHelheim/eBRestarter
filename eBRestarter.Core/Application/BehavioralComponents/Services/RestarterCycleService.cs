using eBRestarter.Core.Application.BehavioralComponents.Handlers.Interfaces;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Services;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Validators;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Domain.Handlers;

namespace eBRestarter.Core.Application.Services;

public sealed class RestarterCycleService(
    IOutboundPortBrowserFactory BrowserFactory,
    IInboundPortLocalizationProvider LocalizationService,

    IOutboundPortEVisitorConfigRepository configService,
    IBrowserCleanupScheduleHandler browserCleanupScheduleHandler,
    TimeProvider timeProvider,
    IInboundPortApplicationValidator<ManageRestarterCycleRequest> validator,
    IDelayPhaseHandler delayPhaseHandler,
    IRunBrowserPhaseHandler runBrowserPhaseHandler) : IInboundPortRestarterCycleService
{
    private const string BaseUrl = "https://www.ebesucher.de/surfbar/";

    private const int InitialDelaySeconds = 5;

    private readonly IOutboundPortBrowserFactory _browserFactory = BrowserFactory;
    private readonly IInboundPortLocalizationProvider _localizationService = LocalizationService;

    private readonly IOutboundPortEVisitorConfigRepository _configService = configService;
    private readonly IBrowserCleanupScheduleHandler _browserCleanupScheduleHandler = browserCleanupScheduleHandler;

    private readonly IInboundPortApplicationValidator<ManageRestarterCycleRequest> _validator = validator;
    private readonly IDelayPhaseHandler _delayPhaseHandler = delayPhaseHandler;
    private readonly IRunBrowserPhaseHandler _runBrowserPhaseHandler = runBrowserPhaseHandler;

    private readonly TimeProvider _timeProvider = timeProvider;

    private CancellationTokenSource? _cts;

    private IOutboundPortBrowser? _currentBrowser;

    public event EventHandler<RestarterCycleProgress>? ProgressChanged;

    public async Task StartAsync(ManageRestarterCycleRequest request, Func<Task> performCleanupCallback)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid)
        {
            ReportProgress(RestartTaskState.Idle, 0, validationResult.Errors[0].ErrorMessage);

            return;
        }

        Stop(); // Ensure any previous run is stopped

        _cts = new CancellationTokenSource();

        var token = _cts.Token;

        try
        {
            await RunCycleAsync(request, performCleanupCallback, token);
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

    public void Stop()
    {
        if (_cts is not null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private async Task RunCycleAsync(ManageRestarterCycleRequest request, Func<Task> performCleanupCallback, CancellationToken token)
    {
        // 1. Initial Delay (wird nur 1x ganz am Anfang ausgefÃ¼hrt)
        await _delayPhaseHandler.ExecuteAsync(RestartTaskState.InitialDelay, InitialDelaySeconds, ReportProgress, token);

        while (!token.IsCancellationRequested)
        {
            var appConfig = _configService.LoadConfig();

            request = request with
            {
                BrowserType = appConfig.Browser != null && !string.IsNullOrWhiteSpace(appConfig.Browser.Selected) && Enum.TryParse<BrowserType>(appConfig.Browser.Selected, true, out var parsedType) ? parsedType : request.BrowserType,

                Username = string.IsNullOrWhiteSpace(appConfig.Username) ? request.Username : appConfig.Username,

                RuntimeSeconds = appConfig.Browser is not null ? appConfig.Browser.RuntimeHours * 3600 : request.RuntimeSeconds,

                PauseSeconds = appConfig.Browser?.RuntimePauseSeconds ?? request.PauseSeconds,

                CheckBrowserAliveRoutine = appConfig.Browser?.CheckBrowserAliveRoutine ?? request.CheckBrowserAliveRoutine
            };

            // 2. Running Phase (Nutzt jetzt automatisch die taufrischen Settings!)
            LaunchBrowser(request);

            var runPhaseResult = await _runBrowserPhaseHandler.ExecuteAsync(request, _currentBrowser, ReportProgress, token);

            // Fall A: Browser ist abgestürzt/geschlossen worden
            if (runPhaseResult == BrowserPhaseResult.BrowserClosed)
            {
                CloseCurrentBrowser();

                for (int countdown = 5; countdown > 0; countdown--)
                {
                    token.ThrowIfCancellationRequested();

                    ReportProgress(RestartTaskState.Cooldown, countdown, $"Der Browser wurde geschlossen, Starte Restarter in {countdown}...");

                    await Task.Delay(TimeSpan.FromMilliseconds(1000), _timeProvider, token);
                }

                continue; // Springt wieder nach oben -> Lädt Config neu -> Startet!
            }

            // Fall B & C: Cleanup oder Completed
            await CheckAndExecuteBrowserCleanupAsync(performCleanupCallback, token);

            // 4. Cooldown Phase (Nutzt automatisch die frische Pausen-Zeit)
            CloseCurrentBrowser();

            await _delayPhaseHandler.ExecuteAsync(RestartTaskState.Cooldown, request.PauseSeconds, ReportProgress, token);
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
            var errorFormat = _localizationService.RetrieveString("General_ErrorPrefix");

            var errorMsg = string.Format(errorFormat, ex.Message);

            ReportProgress(RestartTaskState.Idle, 0, errorMsg);

            throw; // Stop the cycle
        }
    }

    private void CloseCurrentBrowser()
    {
        _currentBrowser?.Close();
        _currentBrowser = null;
    }

    private async Task CheckAndExecuteBrowserCleanupAsync(Func<Task> performCleanupCallback, CancellationToken token)
    {
        var appConfig = _configService.LoadConfig();

        var browser = appConfig.Browser;

        if (browser is null || !_browserCleanupScheduleHandler.ShouldRunCleanupNow(browser.DeleteBrowserCacheIntervalDays, browser.NextBrowserDeleteCacheDate))
            return;

        CloseCurrentBrowser();

        await Task.Delay(TimeSpan.FromMilliseconds(1000), _timeProvider, token);

        // UI benachrichtigen (Dialog öffnen)
        await performCleanupCallback();

        // Neues Datum berechnen und in der aktuellen Config speichern
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
            RestartTaskState.Idle => _localizationService.RetrieveString("Task_StatusStopped"),

            RestartTaskState.InitialDelay => string.Format(_localizationService.RetrieveString("Task_StatusStartIn"), secondsRemaining),

            RestartTaskState.Running => _localizationService.RetrieveString("Task_StatusRunning"),

            RestartTaskState.Cooldown => string.Format(_localizationService.RetrieveString("Task_StatusRestartIn"), secondsRemaining),

            _ => string.Empty
        };
    }
}