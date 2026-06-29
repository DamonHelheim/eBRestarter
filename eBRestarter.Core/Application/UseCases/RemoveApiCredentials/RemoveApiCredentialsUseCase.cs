using eBRestarter.Core.Application.Ports.Outbound.Providers;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.RemoveApiCredentials;
using eBRestarter.Core.Application.Ports.Outbound.Config;

namespace eBRestarter.Core.Application.UseCases.RemoveApiCredentials;

using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Errors;

public sealed class RemoveApiCredentialsUseCase(IEVisitorConfigPort configService) : IRemoveApiCredentialsUseCase
{
    private readonly IEVisitorConfigPort _configService = configService;

    public void Execute()
    {
        var currentConfig = _configService.LoadConfig();

        currentConfig.Settings.ApiUsername = string.Empty;
        currentConfig.Settings.ApiKey = string.Empty;

        _configService.SaveConfig(currentConfig);
    }
}


