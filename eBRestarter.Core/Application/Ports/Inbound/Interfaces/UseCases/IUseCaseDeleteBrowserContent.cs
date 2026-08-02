using eBRestarter.Core.Application.Common.Results;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;

/// <summary>
/// Port: Use Case Interface for deleting browser cache, cookies, and temporary data from the presentation layer.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT (Use Case Interface)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores im Presentation Layer via MVVM.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt INNERHALB des Application Cores (<see cref="Application.UseCases.DeleteBrowserContentUseCase"/>).<br/>
/// - <strong>Begründung:</strong> Dient der Benutzeroberfläche als Eingangstür (Inbound Port) in den Anwendungskern, um die asynchrone Bereinigung der Browserdaten inklusive Fortschrittsüberwachung auszuführen.<br/>
/// </para>
/// </summary>
public interface IUseCaseDeleteBrowserContent
{
    Task<Result> ExecuteAsync(
        DeleteBrowserContentRequest request,
        IProgress<DeleteBrowserContentProgress> progress,
        CancellationToken cancellationToken);
}
