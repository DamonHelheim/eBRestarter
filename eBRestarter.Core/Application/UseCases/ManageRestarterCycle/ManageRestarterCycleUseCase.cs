using eBRestarter.Core.Application.Ports.Inbound.UseCases.ManageRestarterCycle;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Handlers.ManageRestarterCycle.Strategies;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using eBRestarter.Core.Domain.Handlers;
using FluentResults;
using FluentValidation;
using eBRestarter.Core.Application.Ports.Outbound.Providers;
using eBRestarter.Core.Application.Providers;

namespace eBRestarter.Core.Application.UseCases.ManageRestarterCycle;

using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Errors;

public sealed class ManageRestarterCycleUseCase(
    IBrowserFactoryPort BrowserFactory,
    ILocalizationProvider LocalizationService,
    
    IEVisitorConfigPort configService,
    IBrowserCleanupScheduleHandler browserCleanupScheduleHandler,
    TimeProvider timeProvider,
    IValidator<ManageRestarterCycleRequest> validator,
    IDelayPhaseStrategy delayPhaseStrategy,
    IRunBrowserPhaseStrategy runBrowserPhaseStrategy) : IManageRestarterCycleUseCase
{
    private const string BaseUrl = "https://www.ebesucher.de/surfbar/";
    private const int InitialDelaySeconds = 5;

    private readonly IBrowserFactoryPort _browserFactory = BrowserFactory;
    private readonly ILocalizationProvider _localizationService = LocalizationService;
    
    private readonly IEVisitorConfigPort _configService = configService;
    private readonly IBrowserCleanupScheduleHandler _browserCleanupScheduleHandler = browserCleanupScheduleHandler;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IValidator<ManageRestarterCycleRequest> _validator = validator;
    private readonly IDelayPhaseStrategy _delayPhaseStrategy = delayPhaseStrategy;
    private readonly IRunBrowserPhaseStrategy _runBrowserPhaseStrategy = runBrowserPhaseStrategy;

    private CancellationTokenSource? _cts;

    private IBrowserPort? _currentBrowser;

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
        await _delayPhaseStrategy.ExecuteAsync(RestartTaskState.InitialDelay, InitialDelaySeconds, ReportProgress, token);

        while (!token.IsCancellationRequested)
        {
            var appConfig = _configService.LoadConfig();

            request = request with { BrowserType = appConfig.Browser != null ? System.Enum.Parse<BrowserType>(appConfig.Browser.Selected ?? "Edge") : request.BrowserType,

                Username = string.IsNullOrWhiteSpace(appConfig.Username) ? request.Username : appConfig.Username,

                RuntimeSeconds = appConfig.Browser is not null ? appConfig.Browser.RuntimeHours * 3600 : request.RuntimeSeconds,

                PauseSeconds = appConfig.Browser?.RuntimePauseSeconds ?? request.PauseSeconds,

                CheckBrowserAliveRoutine = appConfig.Browser?.CheckBrowserAliveRoutine ?? request.CheckBrowserAliveRoutine
            };

            // 2. Running Phase (Nutzt jetzt automatisch die taufrischen Settings!)
            LaunchBrowser(request);

            var phaseResult = await _runBrowserPhaseStrategy.ExecuteAsync(request, _currentBrowser, ReportProgress, token);

            // Fall A: Browser ist abgestÃ¼rzt/geschlossen worden
            if (phaseResult == BrowserPhaseResult.BrowserClosed)
            {
                CloseCurrentBrowser();

                for (int countdown = 5; countdown > 0; countdown--)
                {
                    token.ThrowIfCancellationRequested();

                    ReportProgress(RestartTaskState.Cooldown, countdown, $"Der Browser wurde geschlossen, Starte Restarter in {countdown}...");

                    await Task.Delay(TimeSpan.FromMilliseconds(1000), _timeProvider, token);
                }

                continue; // Springt wieder nach oben -> LÃ¤dt Config neu -> Startet!
            }

            // Fall B & C: Cleanup oder Completed
            await CheckAndExecuteBrowserCleanupAsync(performCleanupCallback, token);

            // 4. Cooldown Phase (Nutzt automatisch die frische Pausen-Zeit)
            CloseCurrentBrowser();

            await _delayPhaseStrategy.ExecuteAsync(RestartTaskState.Cooldown, request.PauseSeconds, ReportProgress, token);
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

        // UI benachrichtigen (Dialog Ã¶ffnen)
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












