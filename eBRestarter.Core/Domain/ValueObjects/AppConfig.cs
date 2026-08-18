namespace eBRestarter.Core.Domain.ValueObjects;

/// <summary>
/// Represents the root application configuration structure.
/// </summary>
public sealed record AppConfig
{
    /// <summary>
    /// Gets or sets the primary eBesucher username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the browser-specific configuration settings.
    /// </summary>
    public BrowserConfig Browser { get; set; } = new();

    /// <summary>
    /// Gets or sets general application settings.
    /// </summary>
    public SettingsConfig Settings { get; set; } = new();

    /// <summary>
    /// Gets or sets host computer restart schedule settings.
    /// </summary>
    public Computer Computer { get; set; } = new();
}
