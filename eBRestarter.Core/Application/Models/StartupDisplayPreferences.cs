namespace eBRestarter.Core.Application.Models;

/// <summary>
/// Culture and theme resolved from persisted config after launch-time normalization.
/// </summary>
public record StartupDisplayPreferences(string LanguageCode, string ThemeName);
