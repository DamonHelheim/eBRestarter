using System.Diagnostics;

namespace eBRestarter.Infrastructure.Adapters.Wrapper;

/// <summary>
/// Implementierung von <see cref="IProcess"/>, die eine nativ ausgeführte <see cref="Process"/>-Instanz kapselt.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): KEIN ADAPTER (Fall A - Infrastruktur-Hilfsklasse / Wrapper)</strong><br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 1.1 des Leitfadens ist diese Klasse trotz des Suffixes „Adapter“ <strong>kein echter Adapter</strong> im Sinne der hexagonalen Architektur, da sie kein Port-Interface aus dem Application Core implementiert. Sie dient rein als technischer Wrapper zur Entkopplung von OS-Prozessen innerhalb der Infrastruktur.<br/>
/// - <strong>Aktion:</strong> Wurde aus <c>Infrastructure/Adapters/Wrapper</c> in den Ordner <c>Infrastructure/Wrappers</c> verschoben.
/// </para>
/// </summary>
public partial class ProcessAdapter(Process process) : IProcess
{
    private bool _disposedValue;

    private readonly Process _process = process ?? throw new ArgumentNullException(nameof(process));

    public StreamReader StandardOutput => _process.StandardOutput;
    public StreamReader StandardError => _process.StandardError;
    public string ProcessName => _process.ProcessName;
    public IntPtr MainWindowHandle => _process.MainWindowHandle;

    public void WaitForExit() =>
        _process.WaitForExit();

    public bool WaitForExit(int milliseconds) =>
        _process.WaitForExit(milliseconds);

    public Task WaitForExitAsync() =>
        _process.WaitForExitAsync();

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _process.Dispose();
            }
            _disposedValue = true;
        }
    }
}
