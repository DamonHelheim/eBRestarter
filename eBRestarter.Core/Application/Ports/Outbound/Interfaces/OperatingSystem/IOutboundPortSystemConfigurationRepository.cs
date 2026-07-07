namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

/// <summary>
/// Port: Driven Port (Outbound) for reading and writing persistent system and user settings in the host operating system (e.g., Windows Registry).
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Systemeinstellungen)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt in Infrastruktur-Adaptern (wie <see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsStartupRepository"/>, Browser-Adaptern und Update-Services).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsRegistryRepository"/> via Win32 Registry-APIs).<br/>
/// - <strong>Begründung:</strong> Kapselt tiefe OS-Registry-Zugriffe für die Anwendung und stellt somit nach Leitfaden einen klassischen <strong>Outbound Port</strong> dar.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>RepositoryOutboundPort</c> ist vorbildlich. Note: Die Methodennamen (z. B. <c>subKey</c>) verraten leicht die unterliegende Registry-Struktur, bieten aber eine hervorragende Abstraktion von den physischen Win32-Aufrufen.
/// </para>
/// </summary>
public interface IOutboundPortSystemConfigurationRepository
{
    void SetUserValue(string subKey, string name, object value);
    void DeleteUserValue(string subKey, string name);
    void SetSystemValue(string subKey, string name, object value);
    object? GetUserValue(string subKey, string valueName);
    Dictionary<string, object> GetUserValues(string subKey);
    object? GetSystemValue(string subKey, string valueName);
}
