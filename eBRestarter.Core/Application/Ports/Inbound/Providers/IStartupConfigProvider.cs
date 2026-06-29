using eBRestarter.Core.Application.Models;

namespace eBRestarter.Core.Application.Ports.Inbound.Providers;

/// <summary>
/// Normalizes persisted config on process launch (e.g. browser cache delete date) and exposes UI preferences.
/// </summary>
public interface IStartupConfigProvider
{
    /// <summary>
    /// Rolls forward the next browser-cache deletion date when it falls on today (and interval is active),
    /// persists the config, then returns language and theme for WinUI bootstrap.
    /// </summary>
    StartupDisplayPreferences PrepareConfigForLaunch();
}

