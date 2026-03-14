using System;
using eBRestarter.Core.Application.Interfaces.Config;

namespace eBRestarter.Core.Application.UseCases.ScheduleBrowserCleanup;

public class ScheduleBrowserCleanupService : IScheduleBrowserCleanupUseCase
{
    private readonly IEVisitorConfigService _configService;
    private readonly TimeProvider _timeProvider;

    public ScheduleBrowserCleanupService(
        IEVisitorConfigService configService,
        TimeProvider timeProvider)
    {
        _configService = configService;
        _timeProvider = timeProvider;
    }

    public ScheduleBrowserCleanupResponse UpdateSchedule(ScheduleBrowserCleanupRequest request)
    {
        var config = _configService.LoadConfig();
        config.Browser.DeleteBrowserCacheIntervalDays = request.IntervalDays;

        bool isActive = IsIntervalAllowed(request.IntervalDays);
        DateTime? nextDate = null;

        if (isActive)
        {
            nextDate = _timeProvider.GetLocalNow().Date.AddDays(request.IntervalDays);
            config.Browser.NextBrowserDeleteCacheDate = nextDate.Value;
        }
        else
        {
            config.Browser.NextBrowserDeleteCacheDate = DateTime.MinValue;
        }

        _configService.SaveConfig(config);

        return new ScheduleBrowserCleanupResponse(isActive, nextDate);
    }

    private bool IsIntervalAllowed(int days)
    {
        return days switch
        {
            1 or 3 or 7 or 14 => true,
            _ => false
        };
    }
}
