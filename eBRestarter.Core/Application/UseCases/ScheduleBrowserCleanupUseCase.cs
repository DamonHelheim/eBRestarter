using System;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;

namespace eBRestarter.Core.Application.UseCases;

/// <summary>
/// Use case implementation for configuring and scheduling automated browser cache cleanup intervals.
/// </summary>
/// <param name="configService">Outbound repository for accessing application configuration settings.</param>
/// <param name="timeProvider">Time provider for evaluating current date and calculating schedules.</param>
public sealed class ScheduleBrowserCleanupUseCase(
    IOutboundPortEVisitorConfigRepository configService,
    TimeProvider timeProvider) : IUseCaseScheduleBrowserCleanup
{
    private const int DisabledIntervalDays = 0;
    private const int IntervalBiWeeklyDays = 14;
    private const int IntervalDailyDays = 1;
    private const int IntervalThreeDays = 3;
    private const int IntervalWeeklyDays = 7;

    private readonly IOutboundPortEVisitorConfigRepository _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

    /// <inheritdoc />
    public ScheduleBrowserCleanupResponse UpdateSchedule(ScheduleBrowserCleanupRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var config = _configService.LoadConfig();
        var isActive = IsIntervalAllowed(request.IntervalDays);

        if (isActive)
        {
            config.Browser.UpdateCleanupSettings(request.IntervalDays, _timeProvider);
        }
        else
        {
            config.Browser.UpdateCleanupSettings(DisabledIntervalDays, _timeProvider);
            config.Browser.SetNextCleanupDate(DateTime.MinValue);
        }

        _configService.SaveConfig(config);

        return new ScheduleBrowserCleanupResponse(isActive, isActive ? config.Browser.NextBrowserDeleteCacheDate : null);
    }

    private static bool IsIntervalAllowed(int days) => days switch
    {
        IntervalDailyDays or IntervalThreeDays or IntervalWeeklyDays or IntervalBiWeeklyDays => true,
        _ => false
    };
}
