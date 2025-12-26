using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Infrastructure.Wrapper.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Serilog;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace eBRestarter.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Verwaltet Systemstart-Konfigurationen, einschließlich Windows Autostart, Edge Startup Boost 
    /// und AutoLogon-Einstellungen über die Windows Registry.
    /// <br/>
    /// <b>Architektur-Layer:</b> Infrastructure (Adapter)
    /// <br/>
    /// <b>Verantwortlichkeit:</b> Kapselt die Logik zum Schreiben und Lesen von Registry-Werten, 
    /// um das Verhalten von Windows beim Start zu beeinflussen. Nutzt Wrapper-Interfaces, 
    /// um die Testbarkeit (Mocking) der statischen Registry-Klassen zu gewährleisten.
    /// </summary>
    public class WindowsStartupService : IWindowsStartupManagerService
    {
        // --- Konstanten für Registry-Pfade ---

        // Pfad für den "Current User" Run-Key (Standard Autostart für den aktuellen Benutzer).
        // Anwendungen, die hier eingetragen sind, starten automatisch nach dem Login.
        private const string RegistryPathRun = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        // Pfad für Edge Policies (Maschinenweit / HKLM).
        // Hier wird der "Startup Boost" gesteuert, der Edge im Hintergrund vorlädt.
        private const string RegistryPathEdgePolicies = @"SOFTWARE\Policies\Microsoft\Edge";

        // Pfad für Passwordless Sign-in (Maschinenweit / HKLM).
        // Steuert, ob Windows Hello zwingend erforderlich ist oder ob AutoLogon möglich ist.
        private const string RegistryPathPasswordLess = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device";

        private readonly ILogger<WindowsStartupService> _logger;

        // Wrapper für Registry-Zugriffe (ermöglicht Unit-Tests ohne echte Registry).
        private readonly IWindowsRegistryService _registry;

        // Wrapper für Prozess-Informationen (ermöglicht Unit-Tests ohne echten Prozess).
        private readonly IProcessInfoService _processInfo;

        /// <summary>
        /// Initialisiert eine neue Instanz des <see cref="WindowsStartupService"/>.
        /// </summary>
        /// <param name="logger">Logger für Fehler- und Statusmeldungen.</param>
        /// <param name="registry">Inijiierter Service zum Zugriff auf die Windows-Registry (Wrapper).</param>
        /// <param name="processInfo">Inijiierter Service zum Abrufen des aktuellen Exe-Pfads (Wrapper).</param>
        public WindowsStartupService(
            ILogger<WindowsStartupService> logger,
            IWindowsRegistryService registry,
            IProcessInfoService processInfo)
        {
            _logger = logger;
            _registry = registry;
            _processInfo = processInfo;
        }

        /// <summary>
        /// Fügt die aktuelle Anwendung zum Windows Autostart ("Run"-Key) für den aktuellen Benutzer hinzu.
        /// </summary>
        /// <remarks>
        /// Nutzt <see cref="IProcessInfoService"/>, um den Pfad der laufenden .exe zu ermitteln.
        /// Schreibt in <c>HKEY_CURRENT_USER</c>, was keine Administratorrechte erfordert.
        /// </remarks>
        public void EnableAutoStart()
        {
            try
            {
                // Hole den Pfad der aktuell ausgeführten .exe-Datei über den Wrapper.
                string exePath = _processInfo.GetCurrentExecutablePath();

                // Setze den Registry-Wert: Name = "eV Restarter", Wert = "C:\Pfad\zur\App.exe"
                _registry.SetCurrentUserValue(RegistryPathRun, "eBRestarter", exePath);

                _logger.LogInformation("Autostart-Eintrag für 'eBRestarter' erfolgreich erstellt.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Setzen des Autostarts in der Registry.");
            }
        }

        /// <summary>
        /// Entfernt die Anwendung aus dem Windows Autostart ("Run"-Key).
        /// </summary>
        public void DisableAutoStart()
        {
            try
            {
                // Löscht den Wert "eV Restarter" aus dem Run-Key.
                // Der Wrapper behandelt den Fall, dass der Key gar nicht existiert, intern.
                _registry.DeleteCurrentUserValue(RegistryPathRun, "eBRestarter");

                _logger.LogInformation("Autostart-Eintrag für 'eBRestarter' entfernt.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Entfernen des Autostarts aus der Registry.");
            }
        }

        /// <summary>
        /// Ruft alle aktuellen Autostart-Einträge des Benutzers ab.
        /// </summary>
        /// <returns>Ein Dictionary mit dem Namen der Anwendung (Key) und dem Pfad (Value).</returns>
        public Dictionary<string, object> GetStartupEntries()
        {
            try
            {
                // Liest alle Werte unter HKCU\...\Run aus.
                return _registry.GetCurrentUserValues(RegistryPathRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehlerbeim Abrufen der Autostart-Einträge.");
                // Gib eine leere Liste zurück, um NullReferenceExceptions im UI zu vermeiden.
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Aktiviert oder deaktiviert das "Startup Boost" Feature von Microsoft Edge.
        /// </summary>
        /// <param name="enable">
        /// <c>true</c>: Setzt Registry-Wert auf 1 (Aktiviert).
        /// <c>false</c>: Setzt Registry-Wert auf 0 (Deaktiviert).
        /// </param>
        /// <remarks>
        /// <b>Erfordert Administratorrechte</b>, da in <c>HKEY_LOCAL_MACHINE</c> geschrieben wird.
        /// </remarks>
        public void SetEdgeStartupBoost(bool enable)
        {
            try
            {
                // Konvertierung: Registry erwartet DWORD (1 = an, 0 = aus)
                int dwordValue = enable ? 1 : 0;

                // Setzt den Wert im HKLM-Zweig via Wrapper.
                // Der Wrapper kümmert sich um "CreateSubKey", falls der Pfad noch nicht existiert.
                _registry.SetLocalMachineValue(RegistryPathEdgePolicies, "StartupBoostEnabled", dwordValue, RegistryValueKind.DWord);

                _logger.LogInformation("Edge Startup Boost auf {State} gesetzt (Wert: {Value}).", enable, dwordValue);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Spezifisches Logging, wenn dem User die Rechte fehlen (App nicht als Admin gestartet).
                _logger.LogError(ex, "Zugriff verweigert beim Ändern der Edge-Policies. Bitte als Administrator ausführen.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Allgemeiner Fehler beim Setzen von Edge Startup Boost.");
            }
        }

        /// <summary>
        /// Konfiguriert die "DevicePasswordLessBuildVersion"-Einstellung, um automatische Anmeldung zu erlauben oder zu verbieten.
        /// </summary>
        /// <param name="enable">
        /// <c>true</c>: Setzt Wert auf 0 (Erlaubt AutoLogon / deaktiviert Hello-Zwang).
        /// <c>false</c>: Setzt Wert auf 2 (Erzwingt Windows Hello / deaktiviert AutoLogon).
        /// </param>
        /// <remarks>
        /// <b>Erfordert Administratorrechte</b> (HKEY_LOCAL_MACHINE).
        /// <br/>
        /// Logik-Erklärung:
        /// 0 = PasswordLess Disabled -> Klassischer Login (inkl. AutoLogon) möglich.
        /// 2 = PasswordLess Enabled -> Windows Hello zwingend erforderlich.
        /// </remarks>
        public void SetAutoLogon(bool enable)
        {
            try
            {
                // Die Logik ist hier invertiert zur Registry-Bedeutung:
                // Wir wollen AutoLogon *einschalten* (enable=true) -> Das bedeutet PasswordLess *aus* (Wert 0).
                int dwordValue = enable ? 0 : 2;

                _registry.SetLocalMachineValue(RegistryPathPasswordLess, "DevicePasswordLessBuildVersion", dwordValue, RegistryValueKind.DWord);

                _logger.LogInformation("AutoLogon-Einstellung auf {State} gesetzt (RegWert: {Value}).", enable, dwordValue);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Zugriff verweigert beim Ändern der PasswordLess-Einstellungen. Bitte als Administrator ausführen.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Konfigurieren der AutoLogon-Einstellungen.");
            }
        }
    }
}
