using eBRestarter.Core.Application.Interfaces.Config;

namespace eBRestarter.Core.Application.UseCases.ScheduleBrowserCleanup;

public class ScheduleBrowserCleanupService(
    IEVisitorConfigService configService,
    TimeProvider timeProvider) : IScheduleBrowserCleanupUseCase
{
    private readonly IEVisitorConfigService _configService = configService;
    private readonly TimeProvider _timeProvider = timeProvider;

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

    private static bool IsIntervalAllowed(int days)
    {
        return days switch
        {
            1 or 3 or 7 or 14 => true,
            _ => false
        };
    }
}
