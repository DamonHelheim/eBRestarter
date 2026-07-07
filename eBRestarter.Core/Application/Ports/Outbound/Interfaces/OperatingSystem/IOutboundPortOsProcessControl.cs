namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

/// <summary>
/// Port: Driven Port (Outbound) for managing operating system processes, launching executables/urls, and executing system shutdowns.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Prozesssteuerung)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (<see cref="Services.ComputerRestartService"/>, Use Cases und Handlers) sowie in Browser-Adaptern und ViewModels.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsProcessControlAdapter"/> via <c>System.Diagnostics.Process</c> und Win32-APIs).<br/>
/// - <strong>Begründung:</strong> Entkoppelt die gesamte Anwendung und Domäne von physischen Prozessaufrufen (Starten/Stoppen von Programmen, Herunterfahren) und ist somit nach Abschnitt 1 des Leitfadens ein vorbildlicher <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt.
/// </para>
/// </summary>
public interface IOutboundPortOsProcessControl
{
    void RunInstaller(string installerPath);
    void StartExecutable(string exeFilePath);
    Task StartExecutableAsync(string exeFilePath);
    void OpenDirectoryInFileBrowser(string folderPath);
    void OpenUrlInBrowser(string exeFilePath, string arguments);
    void CloseApplication(string processName);
    Task CloseAllOpenProgramsAsync(int timeoutMilliseconds);
    void ShutdownComputer();
    bool IsProcessAlive(string processName);
}
