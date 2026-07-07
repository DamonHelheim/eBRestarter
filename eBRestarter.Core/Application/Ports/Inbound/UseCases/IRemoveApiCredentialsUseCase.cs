namespace eBRestarter.Core.Application.Ports.Inbound.UseCases;

/// <summary>
/// Port: Use Case Interface for removing stored E-Visitor API credentials from the presentation layer.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT (Use Case Interface)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelOptionsApi"/> im Presentation Layer via MVVM).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt INNERHALB des Application Cores (<see cref="eBRestarter.Core.Application.UseCases.RemoveApiCredentialsUseCase"/>).<br/>
/// - <strong>Begründung:</strong> Dient der Benutzeroberfläche als Eingangstür (Inbound Port) in den Anwendungskern, um gespeicherte API-Zugangsdaten sicher zu löschen. Suffix <c>UseCase</c> ist vorbildlich.
/// </para>
/// </summary>
public interface IRemoveApiCredentialsUseCase
{
    void Execute();
}

