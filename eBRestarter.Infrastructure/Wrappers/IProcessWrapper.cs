using eBRestarter.Infrastructure.Adapters.Wrapper;
using System.Diagnostics;

namespace eBRestarter.Infrastructure.Wrappers;

/// <summary>
/// Kapselt statische Methoden von <see cref="Process"/> für Unit-Tests und Mocking im Infrastructure Layer.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): KEIN ADAPTER (Fall A - Infrastruktur-Hilfsinterface)</strong><br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 1.1 des Leitfadens ist dieses Interface <strong>kein Adapter</strong>, da es kein Port-Interface aus dem Application Core darstellt oder implementiert. Es dient rein als internes Hilfsinterface zur Testbarkeit von <see cref="Process.Start()"/> und <see cref="Process.GetProcessesByName"/>.<br/>
/// - <strong>Aktion:</strong> Wurde aus <c>Infrastructure/Adapters/Wrapper</c> in den Ordner <c>Infrastructure/Wrappers</c> verschoben.
/// </para>
/// </summary>
public interface IProcessWrapper
{
    // Muss Process? zurÃ¼ckgeben, damit wir Streams lesen kÃ¶nnen (fÃ¼r MSI)
    IProcess? Start(ProcessStartInfo info);

    // FÃ¼r IsProcessAlive
    bool IsProcessRunning(string name);

    // FÃ¼r CloseApplication
    void KillProcess(string name);

    IProcess[] GetProcesses();
}

