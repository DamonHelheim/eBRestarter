namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces;

/// <summary>
/// Port: Driven Port (Outbound) for deleting files and counting directory items on the physical file system.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für Dateisystem)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt INNERHALB des Application Cores (<see cref="UseCases.DeleteBrowserContentUseCase"/>).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsFileDeletionServiceAdapter"/> im Infrastructure Layer via OS-Dateisystem-APIs).<br/>
/// - <strong>Begründung:</strong> Da der Anwendungskern diese Schnittstelle zur Ausführung physischer Dateioperationen aufruft und die Implementierung im Infrastruktur-Layer liegt, handelt es sich nach Leitfaden Abschnitt 1 um einen vorbildlichen <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt. Zur besseren Übersicht sollte die Datei idealerweise in den Unterordner <c>Ports/Outbound/OperatingSystem/</c> (oder einen separaten <c>FileSystem</c>-Ordner) verschoben werden.
/// </para>
/// </summary>
public interface IOutboundPortFileDeletion
{
    /// <summary>
    /// Recursively counts files within the specified directories.
    /// </summary>
    Task<int> CountFilesAsync(List<string> directories);

    /// <summary>
    /// Deletes files within the specified directories and reports progress.
    /// </summary>
    Task DeleteFilesAsync(List<string> directories, IProgress<string> statusReporter, IProgress<int> valueReporter, CancellationToken token);

    /// <summary>
    /// Deletes a single file (e.g., for Firefox cookies).
    /// </summary>
    void DeleteSingleFile(string filePath);
}
