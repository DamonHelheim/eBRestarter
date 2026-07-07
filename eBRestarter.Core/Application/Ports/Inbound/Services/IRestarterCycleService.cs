using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.Services;

/// <summary>
/// Port: Controls the execution lifecycle of the browser restarting cycle from the UI/presentation layer.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT (System-Trigger / Application Service Port)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelRestartTask"/> im Presentation Layer via MVVM).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt INNERHALB des Application Cores (<see cref="eBRestarter.Core.Application.Services.RestarterCycleService"/>).<br/>
/// - <strong>Begründung:</strong> Dient der Benutzeroberfläche als Eingangstür in den Anwendungskern, um den Haupt-Neustartzyklus zu starten, zu stoppen und den Fortschritt zu überwachen.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix sollte gemäß Leitfaden idealerweise auf <c>Port</c> (z. B. <c>IRestarterCycleServicePort</c>) enden.
/// </para>
/// </summary>
public interface IRestarterCycleService
{
    event EventHandler<RestarterCycleProgress> ProgressChanged;

    Task StartAsync(ManageRestarterCycleRequest request, Func<Task> performCleanupCallback);

    void Stop();
}

