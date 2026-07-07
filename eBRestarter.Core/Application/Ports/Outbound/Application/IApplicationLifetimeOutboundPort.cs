namespace eBRestarter.Core.Application.Ports.Outbound;

/// <summary>
/// Port: Driven Port (Outbound) for terminating the current application process via the host operating system.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Prozesssteuerung)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt INNERHALB des Application Cores (<see cref="eBRestarter.Core.Application.Services.ComputerRestartService"/>).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsApplicationLifetimeAdapter"/> im Infrastructure Layer via <c>Environment.Exit</c>).<br/>
/// - <strong>Begründung:</strong> Da der Core über diese Schnittstelle die physische Beendigung des Prozesses anfordert und die Implementierung im OS-spezifischen Adapter liegt, handelt es sich nach Leitfaden um einen vorbildlichen <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt. Hinweis zum Namespace: Die Datei befindet sich im Ordner <c>Ports/Outbound/Application/</c>, nutzt aber noch den Wurzel-Namespace <c>eBRestarter.Core.Application.Ports.Outbound</c>.
/// </para>
/// </summary>
public interface IApplicationLifetimeOutboundPort
{
    void ExitApplication(int exitCode);
}
