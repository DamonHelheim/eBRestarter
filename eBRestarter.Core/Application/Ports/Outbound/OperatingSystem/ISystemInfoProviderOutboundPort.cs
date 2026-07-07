namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

/// <summary>
/// Port: Driven Port (Outbound) for querying system-level environment state, default browser configuration, OS build versions, and security privileges (Administrator status).
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Systeminformationen)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (<see cref="eBRestarter.Core.Application.Providers.SystemInformationProvider"/>, <see cref="eBRestarter.Core.Application.UseCases.ConfigureAutoLogonUseCase"/>) und in UI-ViewModels.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsSystemInfoProvider"/> via Windows-Registry und Security-Tokens).<br/>
/// - <strong>Begründung:</strong> Kapselt spezifische OS- und Benutzerrechte-Abfragen für den Core und ist nach Leitfaden ein vorbildlicher <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt. Ergänzt sich nach dem Interface Segregation Principle (ISP) mit <see cref="IHardwareInfoProviderOutboundPort"/> und <see cref="IOsEditionProviderOutboundPort"/>.
/// </para>
/// </summary>
public interface ISystemInfoProviderOutboundPort
{
    string RetrieveCurrentStandardBrowserName();
    string RetrieveCurrentOsBuildVersion();
    string RetrieveCurrentOsDisplayVersion();
    bool IsUserAdministrator();
}


