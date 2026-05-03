using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Models;

namespace eBRestarter.Core.Application.Services;

public class ApplicationLaunchConfigService(IEVisitorConfigService configService) : IApplicationLaunchConfigService
{
    private readonly IEVisitorConfigService _configService = configService;

    /// <inheritdoc />
    public StartupDisplayPreferences PrepareConfigForLaunch()
    {
        var config = _configService.LoadConfig();

        int intervalDays = config.Browser?.DeleteBrowserCacheIntervalDays ?? 0;
        DateTime nextDate = config.Browser?.NextBrowserDeleteCacheDate ?? DateTime.MinValue;

        if (nextDate == DateTime.Today && intervalDays > 0)
        {
            nextDate = DateTime.Today.AddDays(intervalDays);
        }

        config.Browser?.NextBrowserDeleteCacheDate = nextDate;

        _configService.SaveConfig(config);

        string languageCode = config.Settings.Language == 0 ? "de-DE" : "en-US";
        string themeName = string.IsNullOrEmpty(config.Settings.Theme) ? "Light" : config.Settings.Theme;

        return new StartupDisplayPreferences(languageCode, themeName);
    }
}
