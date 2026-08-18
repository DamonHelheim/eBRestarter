using System;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;

namespace eBRestarter.Core.Application.BehavioralComponents.Providers;

/// <summary>
/// Provides startup configuration preferences such as language and UI theme derived from the application configuration repository.
/// </summary>
/// <param name="configService">The repository provider for application configuration.</param>
public sealed class StartupConfigProvider(IOutboundPortEVisitorConfigRepository configService) : IInboundPortStartupConfigProvider
{
    private const int GermanLanguageCode = 0;

    private const string DefaultEnglishCultureCode = "en-US";
    private const string DefaultGermanCultureCode = "de-DE";
    private const string DefaultThemeName = "Light";

    private readonly IOutboundPortEVisitorConfigRepository _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    /// <inheritdoc />
    public StartupDisplayPreferences RetrieveStartupPreferences()
    {
        var appConfig = _configService.LoadConfig();

        string languageCode = appConfig?.Settings?.Language == GermanLanguageCode
            ? DefaultGermanCultureCode
            : DefaultEnglishCultureCode;

        string? rawTheme = appConfig?.Settings?.Theme;
        string themeName = string.IsNullOrWhiteSpace(rawTheme)
            ? DefaultThemeName
            : rawTheme;

        return new StartupDisplayPreferences(languageCode, themeName);
    }
}
