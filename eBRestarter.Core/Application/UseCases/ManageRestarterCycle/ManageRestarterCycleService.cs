using eBRestarter.Core.Application.Constants;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Domain.Services;
using System.Diagnostics;

namespace eBRestarter.Core.Application.UseCases.ManageRestarterCycle;

public class ManageRestarterCycleService(
    IBrowserFactory browserFactory,
    ILocalizationService localizationService,
    IBrowserDisplayNameResolver browserDisplayNameResolver,
    IEVisitorConfigService configService,
    IBrowserCleanupScheduleService browserCleanupScheduleService,
    TimeProvider timeProvider,
    IWindowsProcessControlService processService) : IManageRestarterCycleUseCase // NEU: IWindowsProcessControlService injiziert
{
    private const string BaseUrl = WebLinks.EVisitorSurflink; //"https://www.ebesucher.com/surfbar/";
    private const int InitialDelaySeconds = 5;

    private readonly IBrowserFactory _browserFactory = browserFactory;
    private readonly ILocalizationService _localizationService = localizationService;
    private readonly IBrowserDisplayNameResolver _browserDisplayNameResolver = browserDisplayNameResolver;
    private readonly IEVisitorConfigService _configService = configService;
    private readonly IBrowserCleanupScheduleService _browserCleanupScheduleService = browserCleanupScheduleService;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IWindowsProcessControlService _processService = processService; // NEU

    private CancellationTokenSource? _cts;
    private IBrowser? _currentBrowser;

    public event EventHandler<RestarterCycleProgress>? ProgressChanged;

    public async Task StartAsync(ManageRestarterCycleRequest request, Func<Task> performCleanupCallback)
    {
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
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private async Task RunCycleAsync(ManageRestarterCycleRequest request, Func<Task> performCleanupCallback, CancellationToken token)
    {
        // 1. Initial Delay (wird nur 1x ganz am Anfang ausgefÃ¼hrt)
        await RunDelayPhaseAsync(RestartTaskState.InitialDelay, InitialDelaySeconds, token);

        while (!token.IsCancellationRequested)
        {
            // ====================================================================
            // NEU: Config fÃ¼r den anstehenden Zyklus frisch laden
            // ====================================================================
            var appConfig = _configService.LoadConfig();

            // Da 'request' ein Record ist, kÃ¶nnen wir mit 'with' eine neue Kopie
            // erstellen und dabei nur die aktualisierten Werte Ã¼berschreiben!
            request = request with
            {
                BrowserDisplayName = string.IsNullOrWhiteSpace(appConfig.Browser?.Selected) ? request.BrowserDisplayName : appConfig.Browser.Selected,
                Username = string.IsNullOrWhiteSpace(appConfig.Username) ? request.Username : appConfig.Username,
                RuntimeSeconds = appConfig.Browser != null ? appConfig.Browser.RuntimeHours * 3600 : request.RuntimeSeconds,
                PauseSeconds = appConfig.Browser?.RuntimePauseSeconds ?? request.PauseSeconds,
                CheckBrowserAliveRoutine = appConfig.Browser?.CheckBrowserAliveRoutine ?? request.CheckBrowserAliveRoutine
            };
            // ====================================================================

            // 2. Running Phase (Nutzt jetzt automatisch die taufrischen Settings!)
            LaunchBrowser(request);

            var phaseResult = await RunBrowserPhaseAsync(request, token);

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
            await RunDelayPhaseAsync(RestartTaskState.Cooldown, request.PauseSeconds, token);
        }
    }

    /// <summary>
    /// FÃ¼hrt die Laufzeit aus und prÃ¼ft optional jede Sekunde, ob der Browser noch lÃ¤uft.
    /// Gibt 'true' zurÃ¼ck, wenn die Zeit normal abgelaufen ist, und 'false', wenn der Browser geschlossen wurde.
    /// </summary>
    private async Task<BrowserPhaseResult> RunBrowserPhaseAsync(ManageRestarterCycleRequest request, CancellationToken token)
    {
        string processName = GetProcessNameFromDisplayName(request.BrowserDisplayName);

        // ====================================================================
        // LAZY CONFIG RELOAD: Holt das aktuellste LÃ¶sch-Datum und Intervall
        // fÃ¼r diesen Zyklus direkt aus der frischen Config!
        // ====================================================================
        var appConfig = _configService.LoadConfig();
        bool isCleanupActive = appConfig.Browser != null && appConfig.Browser.DeleteBrowserCacheIntervalDays > 0;
        DateTime nextCleanupDate = appConfig.Browser?.NextBrowserDeleteCacheDate ?? DateTime.MaxValue;

        for (int secondsRemaining = request.RuntimeSeconds; secondsRemaining >= 0; secondsRemaining--)
        {
            token.ThrowIfCancellationRequested();

            // 1. Alive-Check
            if (request.CheckBrowserAliveRoutine && secondsRemaining < request.RuntimeSeconds - 2)
            {
                if (!_processService.IsProcessAlive(processName))
                {
                    return BrowserPhaseResult.BrowserClosed;
                }
            }

            // 2. Cleanup-Check (PrÃ¼ft gegen das taufrische Datum aus der Config)
            if (isCleanupActive && _timeProvider.GetLocalNow().DateTime >= nextCleanupDate)
            {
                return BrowserPhaseResult.CleanupDue;
            }

            ReportProgress(RestartTaskState.Running, secondsRemaining);

            if (secondsRemaining > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000), _timeProvider, token);
            }
        }

        return BrowserPhaseResult.Completed;
    }

    private async Task RunDelayPhaseAsync(RestartTaskState state, int totalSeconds, CancellationToken token)
    {
        for (int secondsRemaining = totalSeconds; secondsRemaining >= 0; secondsRemaining--)
        {
            token.ThrowIfCancellationRequested();
            ReportProgress(state, secondsRemaining);

            if (secondsRemaining > 0)
            {
                // We use Task.Delay with TimeProvider instead of a Timer
                await Task.Delay(TimeSpan.FromMilliseconds(1000), _timeProvider, token);
            }
        }
    }

    private void LaunchBrowser(ManageRestarterCycleRequest request)
    {
        try
        {
            string defaultText = _localizationService.GetString("Task_DefaultBrowser");
            var browserType = _browserDisplayNameResolver.GetBrowserTypeFromDisplayName(request.BrowserDisplayName, defaultText);

            _currentBrowser = _browserFactory.Create(browserType);
            string url = $"{BaseUrl}{request.Username}";

            _currentBrowser.Start(url);
        }
        catch (Exception ex)
        {
            string errorFormat = _localizationService.GetString("General_ErrorPrefix");
            string errorMsg = string.Format(errorFormat, ex.Message);
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
        // ====================================================================
        // LAZY CONFIG RELOAD: Wir laden frisch, bevor wir prÃ¼fen und speichern,
        // damit wir keine anderen Nutzer-Settings versehentlich Ã¼berschreiben!
        // ====================================================================
        var appConfig = _configService.LoadConfig();

        var browser = appConfig.Browser;
        if (browser == null || !_browserCleanupScheduleService.ShouldRunCleanupNow(browser.DeleteBrowserCacheIntervalDays, browser.NextBrowserDeleteCacheDate))
            return;

        CloseCurrentBrowser();

        await Task.Delay(TimeSpan.FromMilliseconds(1000), _timeProvider, token);

        // UI benachrichtigen (Dialog Ã¶ffnen)
        await performCleanupCallback();

        // Neues Datum berechnen und in der aktuellen Config speichern
        var today = _timeProvider.GetLocalNow().Date;
        var newDate = _browserCleanupScheduleService.GetNextCleanupDateAfterRun(today, appConfig.Browser.DeleteBrowserCacheIntervalDays);

        appConfig.Browser.NextBrowserDeleteCacheDate = newDate;
        _configService.SaveConfig(appConfig);
    }

    private void ReportProgress(RestartTaskState state, int secondsRemaining, string? customMessage = null)
    {
        string statusMessage = customMessage ?? GetStatusMessageForState(state, secondsRemaining);
        ProgressChanged?.Invoke(this, new RestarterCycleProgress(state, secondsRemaining, statusMessage));
    }

    private string GetStatusMessageForState(RestartTaskState state, int secondsRemaining)
    {
        return state switch
        {
            RestartTaskState.Idle => _localizationService.GetString("Task_StatusStopped"),
            RestartTaskState.InitialDelay => string.Format(_localizationService.GetString("Task_StatusStartIn"), secondsRemaining),
            RestartTaskState.Running => _localizationService.GetString("Task_StatusRunning"),
            RestartTaskState.Cooldown => string.Format(_localizationService.GetString("Task_StatusRestartIn"), secondsRemaining),
            _ => string.Empty
        };
    }

    /// <summary>
    /// Hilfsmethode, um den Anzeigenamen in den Windows-Prozessnamen umzuwandeln.
    /// </summary>
    private string GetProcessNameFromDisplayName(string displayName)
    {
        if (string.IsNullOrEmpty(displayName)) return "";
        var lower = displayName.ToLowerInvariant();

        if (lower.Contains("firefox")) return "firefox";
        if (lower.Contains("chrome")) return "chrome";
        if (lower.Contains("edge")) return "msedge";
        if (lower.Contains("vivaldi")) return "vivaldi";
        if (lower.Contains("brave")) return "brave";
        if (lower.Contains("opera")) return "opera";

        return lower; // Fallback
    }
}