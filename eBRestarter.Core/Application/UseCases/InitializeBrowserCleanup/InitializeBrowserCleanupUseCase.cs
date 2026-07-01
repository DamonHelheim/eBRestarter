using System;
using System.Threading.Tasks;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.InitializeBrowserCleanup;
using eBRestarter.Core.Application.Ports.Outbound.Config;

namespace eBRestarter.Core.Application.UseCases.InitializeBrowserCleanup;

public class InitializeBrowserCleanupUseCase(IEVisitorConfigRepositoryOutboundPort configService) : IInitializeBrowserCleanupUseCase
{
    private readonly IEVisitorConfigRepositoryOutboundPort _configService = configService;

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
