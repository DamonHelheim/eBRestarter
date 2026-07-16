using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;

namespace eBRestarter.Core.Application.BehavioralComponents.Providers;

public class StartupConfigProvider(IOutboundPortEVisitorConfigRepository configService) : IInboundPortStartupConfigProvider
{
    private readonly IOutboundPortEVisitorConfigRepository _configService = configService;

    /// <inheritdoc />
    public StartupDisplayPreferences RetrieveStartupPreferences()
    {
        var config = _configService.LoadConfig();

        string languageCode = config.Settings.Language == 0 ? "de-DE" : "en-US";

        string themeName = string.IsNullOrEmpty(config.Settings.Theme) ? "Light" : config.Settings.Theme;

        return new StartupDisplayPreferences(languageCode, themeName);
    }
}


