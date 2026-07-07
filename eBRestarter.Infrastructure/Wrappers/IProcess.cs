namespace eBRestarter.Infrastructure.Adapters.Wrapper;

/// <summary>
/// Kapselt eine nativ ausgeführte Betriebssystem-Prozessinstanz für Unit-Tests im Infrastructure Layer.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): KEIN ADAPTER (Fall A - Infrastruktur-Hilfsinterface)</strong><br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 1.1 des Leitfadens ist dieses Interface <strong>kein Adapter</strong>, da es kein Port-Interface aus dem Application Core darstellt oder implementiert. Es dient rein als internes Hilfsinterface zur Testbarkeit von OS-Prozessaufrufen innerhalb der Infrastruktur.<br/>
/// - <strong>Aktion:</strong> Wurde aus <c>Infrastructure/Adapters/Wrapper</c> in den Ordner <c>Infrastructure/Wrappers</c> verschoben.
/// </para>
/// </summary>
public interface IProcess : IDisposable
{
    StreamReader StandardOutput { get; }
    StreamReader StandardError { get; }
    string ProcessName { get; }
    IntPtr MainWindowHandle { get; }

    void WaitForExit();
    bool WaitForExit(int milliseconds);
    Task WaitForExitAsync();
}
