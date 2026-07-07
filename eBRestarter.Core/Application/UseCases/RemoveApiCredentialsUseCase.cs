using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Config;

namespace eBRestarter.Core.Application.UseCases;

public sealed class RemoveApiCredentialsUseCase(IOutboundPortEVisitorConfigRepository configService) : IUseCaseRemoveApiCredentials
{
    private readonly IOutboundPortEVisitorConfigRepository _configService = configService;

    public void Execute()
    {
        var currentConfig = _configService.LoadConfig();

        currentConfig.Settings.ApiUsername = string.Empty;
        currentConfig.Settings.ApiKey = string.Empty;

        _configService.SaveConfig(currentConfig);
    }
}


