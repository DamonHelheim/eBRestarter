using eBRestarter.Core.Domain.ValueObjects;

namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;

/// <summary>
/// Port: Driven Port (Outbound) for loading, saving, and resetting the centralized application configuration.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für Konfigurationsspeicher)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (Use Cases wie <see cref="UseCases.ToggleAppAutoStartUseCase"/>, <see cref="Services.RestarterCycleService"/>) sowie in GUI-ViewModels.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Repositories.Config.EVRestarterConfigRepository"/> im Infrastructure Layer via Dateisystem/JSON).<br/>
/// - <strong>Begründung:</strong> Kapselt die physische Persistenz der Anwendungs- und Benutzereinstellungen und ist nach Abschnitt 1 des Leitfadens ein klassischer <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Namensgebung mit Suffix <c>RepositoryOutboundPort</c> ist vorbildlich.
/// </para>
/// </summary>
public interface IOutboundPortEVisitorConfigRepository
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


