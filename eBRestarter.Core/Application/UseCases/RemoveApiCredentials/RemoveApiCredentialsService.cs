using eBRestarter.Core.Application.Interfaces.Config;

namespace eBRestarter.Core.Application.UseCases.RemoveApiCredentials;

public class RemoveApiCredentialsService(IEVisitorConfigService configService) : IRemoveApiCredentialsUseCase
{
    private readonly IEVisitorConfigService _configService = configService;

    public void Execute()
    {
        var currentConfig = _configService.LoadConfig();

        currentConfig.Settings.ApiUsername = string.Empty;
        currentConfig.Settings.ApiKey = string.Empty;

        _configService.SaveConfig(currentConfig);
    }
}
