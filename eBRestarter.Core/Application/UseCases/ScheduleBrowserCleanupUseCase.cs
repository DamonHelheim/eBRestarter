using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;

namespace eBRestarter.Core.Application.UseCases;


public sealed class ScheduleBrowserCleanupUseCase(IOutboundPortEVisitorConfigRepository configService, TimeProvider timeProvider) : IUseCaseScheduleBrowserCleanup
{
    private readonly IOutboundPortEVisitorConfigRepository _configService = configService;
    private readonly TimeProvider _timeProvider = timeProvider;

    public ScheduleBrowserCleanupResponse UpdateSchedule(ScheduleBrowserCleanupRequest request)
    {
        var config = _configService.LoadConfig();

        var isActive = IsIntervalAllowed(request.IntervalDays);

        if (isActive)
        {
            config.Browser.UpdateCleanupSettings(request.IntervalDays, _timeProvider);
        }
        else
        {
            config.Browser.UpdateCleanupSettings(0, _timeProvider);
            config.Browser.SetNextCleanupDate(DateTime.MinValue);
        }

        _configService.SaveConfig(config);

        return new ScheduleBrowserCleanupResponse(isActive, isActive ? config.Browser.NextBrowserDeleteCacheDate : null);
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


