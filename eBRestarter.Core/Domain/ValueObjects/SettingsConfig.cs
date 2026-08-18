namespace eBRestarter.Core.Domain.ValueObjects;

/// <summary>
/// Represents general user and application preferences settings.
/// </summary>
public sealed record SettingsConfig
{
    /// <summary>
    /// Gets or sets the UI theme preference (e.g., "Light", "Dark").
    /// </summary>
    public string Theme { get; set; } = "Light";

    /// <summary>
    /// Gets or sets the numeric language code identifier for localization.
    /// </summary>
    public int Language { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the application launches automatically with Windows startup.
    /// </summary>
    public bool StartWithWindows { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to start restarter execution upon program launch.
    /// </summary>
    public bool StartRestarterWithProgramStart { get; set; }

    /// <summary>
    /// Gets or sets the configured eBesucher API username.
    /// </summary>
    public string ApiUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the configured eBesucher API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
