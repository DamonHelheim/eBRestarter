using eBRestarter.Core.Application.Models;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Config;

namespace eBRestarter.Core.Application.Providers;

public class StartupConfigProvider(IEVisitorConfigRepositoryOutboundPort configService) : IStartupConfigProvider
{
    private readonly IEVisitorConfigRepositoryOutboundPort _configService = configService;

    /// <inheritdoc />
    public StartupDisplayPreferences RetrieveStartupPreferences()
    {
        var config = _configService.LoadConfig();

        string languageCode = config.Settings.Language == 0 ? "de-DE" : "en-US";
        string themeName = string.IsNullOrEmpty(config.Settings.Theme) ? "Light" : config.Settings.Theme;

        return new StartupDisplayPreferences(languageCode, themeName);
    }
}


