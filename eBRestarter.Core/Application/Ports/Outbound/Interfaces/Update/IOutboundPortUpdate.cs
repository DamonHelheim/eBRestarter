using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.Update;

/// <summary>
/// Port: Driven Port (Outbound) for checking, downloading, and installing new application releases from external update repositories.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für Anwendungs-Updates)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (<see cref="UseCases.ManageApplicationUpdatesUseCase"/>).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.Update.GitHubUpdateAdapter"/> im Infrastructure Layer via GitHub-REST-API und OS-Installer).<br/>
/// - <strong>Begründung:</strong> Kapselt externe Netzwerk- und Installationsprozesse für neue Programmversionen und ist nach Abschnitt 1 des Leitfadens ein klassischer <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist vorbildlich.
/// </para>
/// </summary>
public interface IOutboundPortUpdate
{
    /// <summary>
    /// Validates against the source distribution repository (e.g., GitHub) to see if a newer version is available.
    /// </summary>
    Task<UpdateInfo> CheckForUpdateAsync();

    /// <summary>
    /// Downloads the update package payload (into the system temporary folder location) and launches the installer process.
    /// </summary>
    Task DownloadAndInstallAsync(UpdateInfo updateInfo);
}