using eBRestarter.Core.Domain.Entities;

namespace eBRestarter.Core.Application.Ports.Outbound.Config;

public interface IEVisitorConfigPort
{
    /// <summary>
    /// Loads the configuration. If none exists, a default configuration is created.
    /// </summary>
    AppConfig LoadConfig();

    /// <summary>
    /// Saves the complete configuration.
    /// </summary>
    void SaveConfig(AppConfig config);

    /// <summary>
    /// Deletes or resets the configuration.
    /// </summary>
    void ResetConfig();
}


