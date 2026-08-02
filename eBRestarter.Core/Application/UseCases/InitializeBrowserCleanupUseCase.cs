using System;
using System.Threading.Tasks;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;

namespace eBRestarter.Core.Application.UseCases;

/// <summary>
/// Use case implementation for initializing scheduled browser cache cleanup timelines upon application startup.
/// </summary>
public sealed class InitializeBrowserCleanupUseCase(
    IOutboundPortEVisitorConfigRepository configService,
    TimeProvider timeProvider)
    : IUseCaseInitializeBrowserCleanup
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch A–Z) ──
    private readonly IOutboundPortEVisitorConfigRepository _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Evaluates existing cleanup schedules and recalculates future cleanup dates if necessary.
    /// </summary>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public Task ExecuteAsync()
    {
        var config = _configService.LoadConfig();

        int intervalDays = config.Browser?.DeleteBrowserCacheIntervalDays ?? 0;
        DateTime nextDate = config.Browser?.NextBrowserDeleteCacheDate ?? DateTime.MinValue;
        DateTime today = _timeProvider.GetLocalNow().Date;

        if (nextDate == today && intervalDays > 0)
        {
            nextDate = today.AddDays(intervalDays);
            config.Browser?.SetNextCleanupDate(nextDate);
            _configService.SaveConfig(config);
        }

        return Task.CompletedTask;
    }
}
