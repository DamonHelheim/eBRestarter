using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.ScheduleBrowserCleanup;

/// <summary>
/// Port: Use Case Interface for updating and managing scheduled browser cleanup times from the presentation layer.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT (Use Case Interface)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelRestarterProperties"/> im Presentation Layer via MVVM).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt INNERHALB des Application Cores (<see cref="eBRestarter.Core.Application.UseCases.ScheduleBrowserCleanupUseCase"/>).<br/>
/// - <strong>Begründung:</strong> Dient der Benutzeroberfläche als Eingangstür (Inbound Port) in den Anwendungskern, um den Zeitplan (Intervalle und Termine) für die automatische Browser-Bereinigung zu aktualisieren. Suffix <c>UseCase</c> ist vorbildlich.
/// </para>
/// </summary>
public interface IScheduleBrowserCleanupUseCase
{
    ScheduleBrowserCleanupResponse UpdateSchedule(ScheduleBrowserCleanupRequest request);
}

