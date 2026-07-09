using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;

namespace eBRestarter.Core.Application.UseCases;

public class InitializeBrowserCleanupUseCase(IOutboundPortEVisitorConfigRepository configService) : IUseCaseInitializeBrowserCleanup
{
    private readonly IOutboundPortEVisitorConfigRepository _configService = configService;

    public Task ExecuteAsync()
    {
        var config = _configService.LoadConfig();

        int intervalDays = config.Browser?.DeleteBrowserCacheIntervalDays ?? 0;
        DateTime nextDate = config.Browser?.NextBrowserDeleteCacheDate ?? DateTime.MinValue;

        if (nextDate == DateTime.Today && intervalDays > 0)
        {
            nextDate = DateTime.Today.AddDays(intervalDays);
            config.Browser?.SetNextCleanupDate(nextDate);
            _configService.SaveConfig(config);
        }

        return Task.CompletedTask;
    }
}
