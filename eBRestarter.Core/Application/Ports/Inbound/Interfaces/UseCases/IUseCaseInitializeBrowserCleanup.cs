namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;

/// <summary>
/// Port: Use Case Interface for initializing browser cleanup tasks during application bootstrap from the presentation layer.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT (Use Case Interface)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Desktop.WinUI3.App"/> im Presentation Layer beim App-Start).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt INNERHALB des Application Cores (<see cref="Application.UseCases.InitializeBrowserCleanupUseCase"/>).<br/>
/// - <strong>Begründung:</strong> Dient der Benutzeroberfläche als Eingangstür (Inbound Port) in den Anwendungskern, um beim Start der Anwendung eventuell anstehende Bereinigungsprozesse zu initialisieren. Suffix <c>UseCase</c> ist vorbildlich.
/// </para>
/// </summary>
public interface IUseCaseInitializeBrowserCleanup
{
    Task ExecuteAsync();
}
