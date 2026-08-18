using eBRestarter.Infrastructure.Common.Statics;

namespace eBRestarter.Infrastructure.ObjectArchetypes.DTOs;

/// <summary>
/// Data Transfer Object representing the configuration payload written to the browser extension config file.
/// </summary>
public sealed record ExtensionConfig
{
    /// <summary>
    /// Gets the UI and content language identifier expected by the browser extension.
    /// </summary>
    public string LANGUAGE { get; init; } = "DE";

    /// <summary>
    /// Gets the destination surfbar URL executed by the browser extension.
    /// </summary>
    public string ZIEL_URL { get; init; } = WebLinks.EVisitorSurflink;

    /// <summary>
    /// Gets the restarter polling delay/interval in milliseconds.
    /// </summary>
    public int WARTEZEIT_MS { get; init; } = 180000;
}
