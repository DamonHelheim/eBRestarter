using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.Network;

/// <summary>
/// Port: Driven Port (Outbound) for downloading files over HTTP/HTTPS with progress reporting and cancellation support.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für Netzwerk-Downloads)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (<see cref="UseCases.DownloadBrowserUseCase"/>) sowie in GUI-ViewModels.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.Http.HttpClientDownloadHandlerAdapter"/> im Infrastructure Layer via <c>HttpClient</c>).<br/>
/// - <strong>Begründung:</strong> Entkoppelt den Anwendungskern von externen Netzwerk- und Datei-I/O-Operationen und ist daher nach Abschnitt 1 des Leitfadens ein klassischer <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt.
/// </para>
/// </summary>
public interface IOutboundPortHttpDownload
{
    Task DownloadFileAsync(
        string url,
        string destinationPath,
        IProgress<DownloadProgressStatus> progress,
        CancellationToken cancel);
}
