using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.Authentication;

/// <summary>
/// Port: Driven Port (Outbound) for securely persisting, loading, and removing API credentials (username/key) to/from storage.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für Credential-Speicher)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (z. B. bei der Verwaltung oder dem Löschen von API-Zugangsdaten).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Repositories.Authentication.JsonCredentialStoreRepository"/> im Infrastructure Layer via Dateisystem/JSON).<br/>
/// - <strong>Begründung:</strong> Kapselt den physischen Datenzugriff auf den Zugangsdaten-Speicher für den Anwendungskern und ist daher nach Abschnitt 1 des Leitfadens ein klassischer <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Die Namensgebung mit Suffix <c>RepositoryOutboundPort</c> ist vorbildlich.
/// </para>
/// </summary>
public interface IOutboundPortCredentialStoreRepository
{
    /// <summary>
    /// Securely saves the username and key.
    /// </summary>
    void SaveCredentials(ApiCredentials credentials);

    /// <summary>
    /// Loads the stored data (if available).
    /// </summary>
    ApiCredentials? LoadCredentials();

    /// <summary>
    /// Deletes the data (reset).
    /// </summary>
    void ClearCredentials();

    /// <summary>
    /// Imports credentials from a legacy file (used for migration).
    /// </summary>
    ApiCredentials? ImportFromLegacyFile(string filePath);
}


