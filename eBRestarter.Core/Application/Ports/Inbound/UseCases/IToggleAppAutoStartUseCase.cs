namespace eBRestarter.Core.Application.Ports.Inbound.UseCases;

/// <summary>
/// Port: Use Case Interface for retrieving and modifying application Windows auto-start preferences from the presentation layer.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT (Use Case Interface)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelOptionsGeneral"/> im Presentation Layer via MVVM).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt INNERHALB des Application Cores (<see cref="eBRestarter.Core.Application.UseCases.ToggleAppAutoStartUseCase"/>).<br/>
/// - <strong>Begründung:</strong> Dient der Benutzeroberfläche als Eingangstür (Inbound Port) in den Anwendungskern, um den automatischen App-Start bei der Systemanmeldung ein- oder auszuschalten. Suffix <c>UseCase</c> ist vorbildlich.
/// </para>
/// </summary>
public interface IToggleAppAutoStartUseCase
{
    Task<bool> InitializeAndGetStateAsync();
    Task ToggleAsync(bool enable);
}

