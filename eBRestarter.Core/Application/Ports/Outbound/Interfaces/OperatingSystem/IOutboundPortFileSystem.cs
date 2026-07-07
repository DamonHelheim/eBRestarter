namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

/// <summary>
/// Port: Driven Port (Outbound) abstracting physical operating system file and directory operations (I/O).
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Dateisystem)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (Use Cases wie <see cref="UseCases.DownloadBrowserUseCase"/>, Repositories und Adapter).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsFileSystemAdapter"/> via <c>System.IO</c>).<br/>
/// - <strong>Begründung:</strong> Entkoppelt die gesamte Anwendungs- und Domänenlogik perfekt von physischen Dateioperationen auf dem Host-OS und ist daher nach Abschnitt 1 des Leitfadens ein vorbildlicher <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt.
/// </para>
/// </summary>
public interface IOutboundPortFileSystem
{
    bool FileExists(string path);
    bool DirectoryExists(string path);
    string CombinePaths(params string[] paths);
    string ResolveEnvironmentPath(string variable);
    void DeleteFile(string path);
    void WriteAllText(string path, string content);
    string ReadAllText(string path);
    string[] ReadAllLines(string path);
    void CreateDirectory(string path);
    string? GetDirectoryName(string path);
    Stream OpenRead(string path);
    string[] GetDirectories(string path, string searchPattern);
}
