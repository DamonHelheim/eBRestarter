using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

namespace eBRestarter.Core.Application.Handlers.ManageRestarterCycle.Strategies;

public sealed class RunBrowserPhaseStrategy(
    TimeProvider timeProvider,
    IOsProcessControlPort processService,
    IEVisitorConfigPort configService) : IRunBrowserPhaseStrategy
{
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IOsProcessControlPort _processService = processService;
    private readonly IEVisitorConfigPort _configService = configService;

    public async Task<BrowserPhaseResult> ExecuteAsync(ManageRestarterCycleRequest request, IBrowserPort? currentBrowser, Action<RestartTaskState, int, string?> reportProgress, CancellationToken token)
    {
        var processName = currentBrowser?.ProcessName ?? string.Empty;

        var appConfig = _configService.LoadConfig();

        bool isCleanupActive = appConfig.Browser?.DeleteBrowserCacheIntervalDays > 0;

        DateTime nextCleanupDate = appConfig.Browser?.NextBrowserDeleteCacheDate ?? DateTime.MaxValue;

        for (int secondsRemaining = request.RuntimeSeconds; secondsRemaining >= 0; secondsRemaining--)
        {
            token.ThrowIfCancellationRequested();

            // 1. Alive-Check
            if (request.CheckBrowserAliveRoutine && secondsRemaining < request.RuntimeSeconds - 2 && !_processService.IsProcessAlive(processName))
            {
                return BrowserPhaseResult.BrowserClosed;
            }

            // 2. Cleanup-Check
            if (isCleanupActive && _timeProvider.GetLocalNow().DateTime >= nextCleanupDate)
            {
                return BrowserPhaseResult.CleanupDue;
            }

            reportProgress(RestartTaskState.Running, secondsRemaining, null);

            if (secondsRemaining > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000), _timeProvider, token);
            }
        }

        return BrowserPhaseResult.Completed;
    }
}


