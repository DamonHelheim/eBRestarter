using eBRestarter.Core.Application.Interfaces.Config;

namespace eBRestarter.Core.Application.UseCases.RemoveApiCredentials;

public class RemoveApiCredentialsService : IRemoveApiCredentialsUseCase
{
    private readonly IEVisitorConfigService _configService;

    public RemoveApiCredentialsService(IEVisitorConfigService configService)
    {
        _configService = configService;
    }

    public void Execute()
    {
        var currentConfig = _configService.LoadConfig();

        var newConfig = currentConfig with
        {
            Settings = currentConfig.Settings with
            {
                ApiUsername = string.Empty,
                ApiKey = string.Empty
            }
        };

        _configService.SaveConfig(newConfig);
    }
}
