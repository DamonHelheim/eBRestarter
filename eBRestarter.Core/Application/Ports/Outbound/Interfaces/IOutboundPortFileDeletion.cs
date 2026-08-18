using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces;

/// <summary>
/// Port: Driven Port (Outbound) for deleting files on the physical file system.
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
    /// Deletes files within the specified directories and reports progress.
    /// </summary>
    /// <param name="directories">Directories to clear recursively.</param>
    /// <param name="statusReporter">Receives the file currently being deleted (throttled).</param>
    /// <param name="completedDirectoriesReporter">
    /// Receives the number of directories fully processed so far — NOT a file count.
    /// </param>
    /// <param name="token">Cancellation token observed between files and directories.</param>
    /// <remarks>
    /// ⚡ Guide Kap. 7.4: Die frühere <c>CountFilesAsync</c>-Methode wurde entfernt. Sie lief
    /// <c>Directory.EnumerateFiles(..., AllDirectories)</c> über dieselben Verzeichnisse wie
    /// diese Methode, nur um vorab eine Gesamtzahl für den Fortschrittsbalken zu liefern — der
    /// komplette Cache-Baum wurde also zweimal durchlaufen. Die Verzeichnis-Anzahl ist ohne
    /// jede I/O bekannt, deshalb meldet dieser Reporter jetzt abgeschlossene Verzeichnisse.
    /// </remarks>
    Task DeleteFilesAsync(
        List<string> directories,
        IProgress<string> statusReporter,
        IProgress<int> completedDirectoriesReporter,
        CancellationToken token);

    /// <summary>
    /// Deletes a single file (e.g., for Firefox cookies).
    /// </summary>
    /// <param name="filePath">The absolute path of the file to delete.</param>
    void DeleteSingleFile(string filePath);
}
