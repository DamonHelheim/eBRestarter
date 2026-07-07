using eBRestarter.Core.Application.Ports.Inbound.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Config;

namespace eBRestarter.Core.Application.UseCases;

public sealed class RemoveApiCredentialsUseCase(IEVisitorConfigRepositoryOutboundPort configService) : IRemoveApiCredentialsUseCase
{
    private readonly IEVisitorConfigRepositoryOutboundPort _configService = configService;

    public void Execute()
    {
        var currentConfig = _configService.LoadConfig();

        currentConfig.Settings.ApiUsername = string.Empty;
        currentConfig.Settings.ApiKey = string.Empty;

        _configService.SaveConfig(currentConfig);
    }
}


