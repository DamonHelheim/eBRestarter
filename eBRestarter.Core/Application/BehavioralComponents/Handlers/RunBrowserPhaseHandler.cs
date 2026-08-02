using System;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.BehavioralComponents.Handlers.Interfaces;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Core.Application.BehavioralComponents.Handlers;

/// <summary>
/// Handles the browser execution phase during a restart cycle, monitoring process status and cache cleanup triggers.
/// </summary>
public sealed class RunBrowserPhaseHandler(
    IOutboundPortEVisitorConfigRepository configService,
    IOutboundPortOsProcessControl processService,
    TimeProvider timeProvider) : IRunBrowserPhaseHandler
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private static readonly TimeSpan OneSecondInterval = TimeSpan.FromSeconds(1);

    private const int StartupGracePeriodSeconds = 2;

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    private readonly IOutboundPortEVisitorConfigRepository _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    private readonly IOutboundPortOsProcessControl _processService = processService ?? throw new ArgumentNullException(nameof(processService));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Executes the browser running phase asynchronously, checking process health and cleanup requirements each second.
    /// </summary>
    /// <param name="request">The cycle request parameters defining runtime duration and options.</param>
    /// <param name="currentBrowser">The active browser port instance to monitor, if available.</param>
    /// <param name="reportProgress">Delegate callback action for reporting progress updates.</param>
    /// <param name="token">Cancellation token to observe for phase cancellation.</param>
    /// <returns>A <see cref="BrowserPhaseResult"/> indicating how the phase concluded.</returns>
    public async Task<BrowserPhaseResult> ExecuteAsync(
        ManageRestarterCycleRequest request,
        IOutboundPortBrowser? currentBrowser,
        Action<RestartTaskState, int, string?> reportProgress,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(reportProgress);

        string processName = currentBrowser?.ProcessName ?? string.Empty;
        var appConfig = _configService.LoadConfig();

        bool isCleanupActive = appConfig?.Browser?.DeleteBrowserCacheIntervalDays > 0;
        DateTime nextCleanupDate = appConfig?.Browser?.NextBrowserDeleteCacheDate ?? DateTime.MaxValue;

        using var timer = new PeriodicTimer(OneSecondInterval, _timeProvider);

        for (int secondsRemaining = request.RuntimeSeconds; secondsRemaining >= 0; secondsRemaining--)
        {
            token.ThrowIfCancellationRequested();

            if (request.CheckBrowserAliveRoutine &&
                secondsRemaining < request.RuntimeSeconds - StartupGracePeriodSeconds &&
                !_processService.IsProcessAlive(processName))
            {
                return BrowserPhaseResult.BrowserClosed;
            }

            if (isCleanupActive && _timeProvider.GetLocalNow().DateTime >= nextCleanupDate)
            {
                return BrowserPhaseResult.CleanupDue;
            }

            reportProgress(RestartTaskState.Running, secondsRemaining, null);

            if (secondsRemaining > 0)
            {
                await timer.WaitForNextTickAsync(token);
            }
        }

        return BrowserPhaseResult.Completed;
    }
}
