using eBRestarter.Core.Domain.ValueObjects;

namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;

/// <summary>
/// Port: Driven Port (Outbound) for loading, saving, and resetting the centralized application configuration.
/// <para>
/// <strong>Architectural Classification: OUTBOUND PORT (Driven Port / Configuration Storage Repository)</strong><br/>
/// - <strong>Consumer:</strong> Located inside the Application Core (Use Cases such as <c>ToggleAppAutoStartUseCase</c>, <c>RestarterCycleService</c>) as well as GUI ViewModels.<br/>
/// - <strong>Implementer:</strong> Located in the Infrastructure Layer (<c>EVRestarterConfigRepository</c>, <c>EncryptedEVisitorConfigRepositoryDecorator</c>).<br/>
/// - <strong>Rationale:</strong> Encapsulates physical persistence of application and user configuration for the application core.
/// </para>
/// </summary>
public interface IOutboundPortEVisitorConfigRepository
{
    /// <summary>
    /// Loads the application configuration. If none exists, a default configuration is returned.
    /// </summary>
    /// <returns>The loaded or default <see cref="AppConfig"/>.</returns>
    AppConfig LoadConfig();

    /// <summary>
    /// Saves the complete application configuration to storage.
    /// </summary>
    /// <param name="config">The <see cref="AppConfig"/> instance to persist.</param>
    void SaveConfig(AppConfig config);

    /// <summary>
    /// Deletes or resets the stored configuration to default values.
    /// </summary>
    void ResetConfig();
}
