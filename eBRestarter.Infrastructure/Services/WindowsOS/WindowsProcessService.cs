using eBRestarter.Application.Services.Ports.Interfaces;
using eBRestarter.Infrastructure.Wrapper.Interface;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace eBRestarter.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Implementiert den <see cref="IProcessControlService"/> für das Windows-Betriebssystem.
    /// <br/>
    /// <b>Architektur-Layer:</b> Infrastructure (Adapter)
    /// <br/>
    /// <b>Verantwortlichkeit:</b> Kapselt alle direkten Interaktionen mit Windows-Prozessen, 
    /// dem Starten von Dateien und dem Herunterfahren des Systems.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class WindowsProcessService : IProcessControlService
    {
        /// <summary>
        /// Der Logger für diesen Service. Wird via Dependency Injection bereitgestellt.
        /// </summary>
        private readonly ILogger<WindowsProcessService> _logger;
        private readonly IProcessWrapper _processWrapper; // Neu!

        /// <summary>
        /// Initialisiert eine neue Instanz des <see cref="WindowsProcessService"/>.
        /// </summary>
        /// <param name="logger">
        /// Der Logger, der vom DI-Container (z.B. in App.xaml.cs konfiguriert) injiziert wird.
        /// Ermöglicht das Schreiben von Logs (Serilog, Konsole, Datei) ohne statische Abhängigkeiten.
        /// </param>
        public WindowsProcessService(ILogger<WindowsProcessService> logger, IProcessWrapper processWrapper)
        {
            _logger = logger;
            _processWrapper = processWrapper;
        }

        /// <summary>
        /// Startet eine ausführbare Datei (.exe).
        /// </summary>
        /// <param name="exeFilePath">Der vollständige Pfad zur .exe-Datei.</param>
        public void StartExecutable(string exeFilePath)
        {
            try
            {
                // Kein statischer Aufruf mehr!
                _processWrapper.Start(new ProcessStartInfo { FileName = exeFilePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "...");
            }
        }

        /// <summary>
        /// Startet einen MSI-Installer und protokolliert dessen Ausgabe.
        /// </summary>
        /// <param name="path">Der Pfad zur .msi-Datei.</param>
        public void StartMsiFile(string path)
        {
            // Konfiguration für den MSI-Start.
            // Wir nutzen msiexec.exe direkt, um Argumente (/i für Install) sauber zu übergeben.
            var startInfo = new ProcessStartInfo
            {
                FileName = "msiexec.exe",
                Arguments = $"/i \"{path}\"",

                // UseShellExecute = false wird benötigt, um StandardOutput und StandardError umzuleiten.
                // Mit 'true' könnten wir den Text-Output des Prozesses nicht lesen.
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,

                // Verhindert, dass ein leeres Konsolenfenster aufpoppt.
                CreateNoWindow = true
            };

            try
            {
                using var process = Process.Start(startInfo);

                if (process == null)
                {
                    _logger.LogWarning("MSI Prozess konnte nicht gestartet werden (null): {Path}", path);
                    return;
                }

                // WICHTIG: Wir lesen den Output synchron bis zum Ende.
                // Das blockiert diesen Thread, bis der Installer fertig ist oder Output liefert.
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                // Wir warten explizit, bis der Installer-Prozess beendet ist.
                process.WaitForExit();

                // Logging der Ergebnisse
                if (!string.IsNullOrWhiteSpace(error))
                {
                    _logger.LogWarning("MSI Installer Fehler-Output: {Error}", error);
                }

                if (!string.IsNullOrWhiteSpace(output))
                {
                    // Debug-Level, da Output bei Erfolg oft sehr geschwätzig sein kann
                    _logger.LogDebug("MSI Installer Output: {Output}", output);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Starten des MSI-Installers: {Path}", path);
            }
        }

        /// <summary>
        /// Öffnet eine URL im Standardbrowser des Systems.
        /// </summary>
        /// <param name="url">Die zu öffnende Webadresse (z.B. https://google.com).</param>
        public void OpenUrlInBrowser(string url)
        {
            try
            {
                // Trick: Durch UseShellExecute = true erkennt Windows anhand des Protokolls (http/https),
                // dass der Standardbrowser geöffnet werden muss.
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Öffnen der URL: {Url}", url);
            }
        }

        /// <summary>
        /// Führt einen Neustart des Computers durch.
        /// </summary>
        /// <remarks>
        /// Nutzt den Windows-Befehl 'shutdown'.
        /// Parameter: /r (Reboot), /f (Force close apps), /t 0 (Time zero/sofort).
        /// </remarks>
        public void ShutdownComputer()
        {
            try
            {
                _logger.LogInformation("Fahre Computer herunter (Neustart)...");
                Process.Start("shutdown", "/r /f /t 0");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Versuch, den Computer herunterzufahren.");
            }
        }

        /// <summary>
        /// Prüft, ob ein Prozess mit dem angegebenen Namen aktuell läuft.
        /// </summary>
        /// <param name="processName">Der Name des Prozesses (ohne .exe Endung).</param>
        /// <returns>True, wenn mindestens eine Instanz läuft, sonst False.</returns>
        public bool IsProcessAlive(string processName)
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }

        /// <summary>
        /// Beendet eine Anwendung hart, falls sie läuft.
        /// </summary>
        /// <param name="processName">Name des Prozesses.</param>
        public void CloseApplication(string processName)
        {
            try
            {
                if (_processWrapper.IsProcessRunning(processName))
                {
                    _processWrapper.KillProcess(processName);
                }
            }
            catch (Exception ex)
            {
                // Hier wird der Fehler gefangen, der aus dem Wrapper kommt
                _logger.LogError(ex, "Fehler beim Beenden von {Name}", processName);
            }
        }

        /// <summary>
        /// Versucht, alle geöffneten Fenster/Programme auf dem Desktop sanft zu schließen.
        /// </summary>
        /// <remarks>
        /// Sendet zuerst eine WM_CLOSE Nachricht (entspricht dem Klicken auf das X).
        /// </remarks>
        public void CloseAllOpenPrograms()
        {
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                // WICHTIG: Kritische Systemprozesse dürfen nicht geschlossen werden,
                // da sonst Windows instabil wird oder abstürzt.
                if (process.ProcessName == "System" || process.ProcessName == "Idle")
                    continue;

                try
                {
                    // Wir interagieren nur mit Prozessen, die ein Fenster haben (MainWindowHandle).
                    // Hintergrunddienste werden hier ignoriert.
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        // Sende "Schließen"-Signal (sanftes Beenden)
                        PostMessage(process.MainWindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                        // Warte max. 5 Sekunden, ob der Prozess reagiert
                        bool exited = process.WaitForExit(5000);

                        if (!exited)
                        {
                            _logger.LogWarning("Prozess {Name} hat auf WM_CLOSE nicht reagiert (hängt evtl.).", process.ProcessName);
                            // Optional: Hier könnte man process.Kill() aufrufen, wenn man aggressiver sein will.
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler beim Schließen von Prozess {Name}", process.ProcessName);
                }
            }
        }

        // --- Private Helper Methoden ---

        /// <summary>
        /// Interne Methode zum harten Beenden (Kill) von Prozessen.
        /// </summary>
        /// <param name="processName">Name des Prozesses.</param>
        private void StopProcessInternal(string processName)
        {
            try
            {
                foreach (var process in Process.GetProcessesByName(processName))
                {
                    try
                    {
                        // Kill() ist ein hartes Beenden (Task Manager -> Task beenden).
                        // Daten im Prozess werden eventuell nicht gespeichert.
                        process.Kill();
                        _logger.LogInformation("Prozess gekillt: {Name}", processName);
                    }
                    catch (Win32Exception ex)
                    {
                        // Win32Exception tritt auf, wenn:
                        // 1. Der Prozess zwischenzeitlich schon beendet wurde (Race Condition).
                        // 2. Wir keine Berechtigung haben (z.B. Systemprozess).
                        _logger.LogWarning(ex, "Konnte Prozess {Name} nicht killen (Win32Exception).", processName);
                    }
                }
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "IOException beim Stoppen von Prozess {Name}", processName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Allgemeiner Fehler beim Stoppen von Prozess {Name}", processName);
            }
        }

        // --- Native Importe (P/Invoke) ---

        // Importiert die Funktion aus der Windows User32.dll, um Nachrichten an Fenster zu senden.
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // Konstante für die "Fenster schließen"-Nachricht
        private const uint WM_CLOSE = 0x0010;
    }
}
