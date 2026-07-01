using eBRestarter.Core.Application.Models;

namespace eBRestarter.Core.Application.Ports.Inbound.Providers;

/// <summary>
/// Normalizes persisted config on process launch (e.g. browser cache delete date) and exposes UI preferences.
/// </summary>
public interface IStartupConfigProvider
{
    /// <summary>
    /// Returns language and theme for WinUI bootstrap by reading the configuration.
    /// </summary>
    StartupDisplayPreferences RetrieveStartupPreferences();
}

