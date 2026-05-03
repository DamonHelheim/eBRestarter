using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS.Process;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace eBRestarter.Infrastructure.Services.WindowsOS;

/// <summary>
/// Implementiert den <see cref="IProcessControlService"/> für das Windows-Betriebssystem.
/// <br/>
/// <b>Architektur-Layer:</b> Infrastructure (Adapter)
/// <br/>
/// <b>Verantwortlichkeit:</b> Kapselt die technische Umsetzung der Prozesssteuerung.
/// Nutzt einen <see cref="IProcessWrapper"/>, um Systemaufrufe testbar zu machen,
/// und P/Invoke für Fenster-Interaktionen.
/// </summary>
/// <remarks>
/// Initialisiert eine neue Instanz des <see cref="WindowsProcessService"/>.
/// </remarks>
/// <param name="logger">Der Logger für Fehler- und Info-Meldungen.</param>
/// <param name="processWrapper">Der Wrapper für Systemprozess-Aufrufe (Injected).</param>
[SupportedOSPlatform("windows")]
public partial class WindowsProcessService(ILogger<WindowsProcessService> logger, IProcessWrapper processWrapper) : IWindowsProcessControlService
{
    private readonly ILogger<WindowsProcessService> _logger = logger;

    /// <summary>
    /// Abstraktionsschicht für <see cref="Process"/>-Aufrufe, um Unit-Testing zu ermöglichen.
    /// </summary>
    private readonly IProcessWrapper _processWrapper = processWrapper;

    /// <summary>
    /// Startet eine externe Anwendung (.exe).
    /// </summary>
    /// <param name="exeFilePath">Der vollständige Pfad zur ausführbaren Datei.</param>
    /// <remarks>
    /// Setzt <c>UseShellExecute = true</c>, damit Windows die Datei so behandelt,
    /// als würde der Benutzer sie im Explorer doppelklicken (berücksichtigt UAC, Pfade und Assoziationen).
    /// </remarks>
    public void StartExecutable(string exeFilePath)
    {
        try
        {
            _processWrapper.Start(new ProcessStartInfo
            {
                FileName = exeFilePath,
                UseShellExecute = true
            });

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Executable gestartet: {Path}", exeFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Starten der EXE: {Path}", exeFilePath);
        }
    }

    /// <summary>
    /// Startet einen MSI-Installer (via msiexec.exe) und protokolliert dessen Textausgabe.
    /// </summary>
    /// <param name="path">Der Pfad zur .msi-Datei.</param>
    /// <remarks>
    /// <b>Achtung:</b> Diese Methode läuft <i>synchron</i> und blockiert den aufrufenden Thread,
    /// bis die Installation abgeschlossen ist, um die Logs (StdOut/StdErr) vollständig zu lesen.
    /// </remarks>
    public void StartMsiFile(string path)
    {
        // SonarQube Fix: Absoluten Pfad zur msiexec.exe aus dem System32-Ordner holen,
        // um Path-Hijacking über Umgebungsvariablen zu verhindern.
        string systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
        string msiExecPath = Path.Combine(systemFolder, "msiexec.exe");

        var startInfo = new ProcessStartInfo
        {
            FileName = msiExecPath, // <-- Hier nutzen wir jetzt den absolut sicheren Pfad!
            Arguments = $"/i \"{path}\"",

            // UseShellExecute = false ist zwingend nötig, um Output-Streams umzuleiten.
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,

            // Unterdrückt das Aufpoppen eines leeren Konsolenfensters.
            CreateNoWindow = true
        };

        try
        {
            // Startet den Prozess über den Wrapper
            using var process = _processWrapper.Start(startInfo);

            if (process == null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("MSI Prozess konnte nicht gestartet werden (null): {Path}", path);
                }
                return;
            }

            // Liest die Ausgabeströme (blockierend bis zum Ende)
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            // Protokolliert Fehler oder Ausgaben, falls vorhanden
            if (!string.IsNullOrWhiteSpace(error))
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("MSI Installer Fehler-Output: {Error}", error);
                }
            }

            if (!string.IsNullOrWhiteSpace(output))
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("MSI Installer Output: {Output}", output);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Starten des MSI-Installers: {Path}", path);
        }
    }

    /// <summary>
    /// Startet eine externe Anwendung (.exe) mit optionalen Argumenten.
    /// </summary>
    /// <param name="exeFilePath">Der vollständige Pfad zur ausführbaren Datei.</param>
    /// <param name="arguments">Optionale Argumente (z.B. eine URL).</param>
    public void OpenUrlInBrowser(string exeFilePath, string arguments = "")
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exeFilePath,
                Arguments = arguments, // HIER: Das Argument (die URL) wird gesetzt
                UseShellExecute = true
            };

            _processWrapper.Start(startInfo);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Executable gestartet: {Path} mit Arguments: {Args}", exeFilePath, arguments);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Starten der EXE: {Path}", exeFilePath);
        }
    }

    /// <summary>
    /// Erzwingt einen sofortigen Neustart des Computers.
    /// </summary>
    /// <remarks>
    /// Ruft <c>shutdown.exe</c> mit den Parametern <c>/r</c> (Reboot), <c>/f</c> (Force Close) und <c>/t 0</c> (Sofort) auf.
    /// </remarks>
    public void ShutdownComputer()
    {
        try
        {
            _logger.LogInformation("Fahre Computer herunter (Neustart)...");

            // SonarQube Fix: Absoluten Pfad zur shutdown.exe aus dem System32-Ordner holen
            string systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string shutdownPath = Path.Combine(systemFolder, "shutdown.exe");

            _processWrapper.Start(new ProcessStartInfo(shutdownPath, "/r /f /t 0") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Versuch, den Computer herunterzufahren.");
        }
    }

    /// <summary>
    /// Prüft, ob mindestens eine Instanz eines Prozesses mit dem angegebenen Namen läuft.
    /// </summary>
    /// <param name="processName">Der Name des Prozesses (ohne .exe).</param>
    /// <returns><c>true</c>, wenn der Prozess läuft, sonst <c>false</c>.</returns>
    public bool IsProcessAlive(string processName)
    {
        return _processWrapper.IsProcessRunning(processName);
    }

    /// <summary>
    /// Beendet alle Instanzen einer Anwendung hart (Kill), falls sie laufen.
    /// </summary>
    /// <param name="processName">Der Name des zu beendenden Prozesses.</param>
    public void CloseApplication(string processName)
    {
        try
        {
            if (_processWrapper.IsProcessRunning(processName))
            {
                // KillProcess im Wrapper führt process.Kill() aus.
                _processWrapper.KillProcess(processName);
            }
        }
        catch (Exception ex)
        {
            // Fängt Fehler ab, z.B. wenn der Prozess Systemrechte hat und wir ihn nicht beenden dürfen.
            _logger.LogError(ex, "Fehler beim Beenden von {Name}", processName);
        }
    }

    /// <summary>
    /// Versucht, alle sichtbaren Desktop-Programme sanft zu schließen.
    /// </summary>
    /// <remarks>
    /// Diese Methode sendet eine <c>WM_CLOSE</c>-Nachricht an das Hauptfenster jedes Prozesses
    /// (entspricht dem Klicken auf das X). Wartet bis zu 5 Sekunden auf das Beenden.
    /// <br/>
    /// Kritische Systemprozesse ("System", "Idle") werden ignoriert.
    /// </remarks>
    public void CloseAllOpenPrograms()
    {
        var processes = _processWrapper.GetProcesses();

        foreach (var process in processes)
        {
            // DAS HIER HAT GEFEHLT: using sorgt für den automatischen Aufruf von Dispose()
            using (process)
            {
                if (process.ProcessName == "System" || process.ProcessName == "Idle")
                    continue;

                try
                {
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        PostMessage(process.MainWindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                        bool exited = process.WaitForExit(5000);

                        if (!exited)
                        {
                            if (_logger.IsEnabled(LogLevel.Warning))
                            {
                                _logger.LogWarning("Prozess {Name} hat auf WM_CLOSE nicht reagiert.", process.ProcessName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler beim Schließen von Prozess {Name}", process.ProcessName);
                }
            } // Hier wird Dispose() automatisch aufgerufen!
        }
    }

    // --- Native Importe (P/Invoke) ---

    /// <summary>
    /// Importiert die Funktion <c>PostMessage</c> aus der <c>user32.dll</c>.
    /// Ermöglicht das Senden von Nachrichten an Fenster-Handles.
    /// </summary>
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    public async Task StartExecutableAsync(string exeFilePath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exeFilePath,
                UseShellExecute = true
            };

            // 1. Prozess starten
            // Wichtig: Wir nutzen 'using', damit Ressourcen bereinigt werden,
            // ABER erst nachdem wir gewartet haben.
            using var process = _processWrapper.Start(startInfo);

            if (process != null)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Executable gestartet und warte auf Beendeung: {Path}", exeFilePath);
                }

                // 2. Asynchron warten
                // Das blockiert den UI-Thread NICHT technisch, aber die Methode wartet hier logisch.
                await process.WaitForExitAsync();

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Executable wurde beendet: {Path}", exeFilePath);
                }
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("Prozess konnte nicht gestartet werden (null zurückerhalten): {Path}", exeFilePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Starten/Warten der EXE: {Path}", exeFilePath);
            // Optional: Exception weiterwerfen, damit das ViewModel Bescheid weiß
            throw;
        }
    }

    public void OpenExplorer(string folderPath)
    {
        try
        {
            // Sicherheitshalber Anführungszeichen um den Pfad, falls Leerzeichen enthalten sind.
            string args = $"\"{folderPath}\"";

            // SonarQube Fix: Absoluten Pfad zur explorer.exe aus dem Windows-Hauptordner holen
            string windowsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string explorerPath = Path.Combine(windowsFolder, "explorer.exe");

            _processWrapper.Start(new ProcessStartInfo
            {
                FileName = explorerPath,
                Arguments = args,
                UseShellExecute = true // Wichtig für Explorer-Interaktion
            });

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Explorer geöffnet in: {Path}", folderPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Öffnen des Explorers: {Path}", folderPath);
        }
    }

    /// <summary>
    /// Windows Message ID für "Close Window" (0x0010).
    /// </summary>
    private const uint WM_CLOSE = 0x0010;
}
