using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

/// <summary>
/// Port: Driven Port (Outbound) for querying physical hardware specifications (CPU, RAM, GPU, Motherboard) from the host system.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Hardwareabfrage)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (<see cref="BehavioralComponents.Providers.SystemInformationProvider"/>).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WmiHardwareProvider"/> via Windows WMI-Schnittstellen).<br/>
/// - <strong>Begründung:</strong> Kapselt tiefgreifende OS- und Hardware-Abfragen (WMI) für den Core und ist somit nach Leitfaden ein klassischer <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist vorbildlich.
/// </para>
/// </summary>
public interface IOutboundPortHardwareInfoProvider
{
    Task<HardwareInfo> RetrieveHardwareInfoAsync();
}


