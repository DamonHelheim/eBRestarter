using eBRestarter.Infrastructure.Wrapper.Interface;
using System.Diagnostics;

namespace eBRestarter.Infrastructure.Wrapper;

/// <summary>
/// Die konkrete Implementierung von <see cref="IProcessWrapper"/> für die Laufzeitumgebung.
/// <br/>
/// <b>Zweck:</b> Dient als "Thin Wrapper" um die statische Klasse <see cref="Process"/>.
/// Dies ermöglicht es, Systemaufrufe in Unit-Tests zu mocken, indem im Test eine andere Implementierung
/// des Interfaces verwendet wird.
/// </summary>
public class RealProcessWrapper : IProcessWrapper
{
    /// <summary>
    /// Startet eine Prozessressource, die durch den Parameter <see cref="ProcessStartInfo"/> angegeben wird,
    /// und verknüpft die Ressource mit einer neuen <see cref="Process"/>-Komponente.
    /// </summary>
    /// <param name="info">Die <see cref="ProcessStartInfo"/>, die die Startdaten (Dateiname, Argumente etc.) enthält.</param>
    /// <returns>
    /// Eine neue <see cref="Process"/>-Komponente, die der Prozessressource zugeordnet ist,
    /// oder <c>null</c>, wenn keine Prozessressource gestartet wurde.
    /// </returns>
    public IProcess? Start(ProcessStartInfo info)
    {
        var process = Process.Start(info);

        // Wenn der Start erfolgreich war, verpacken wir den echten Prozess in unseren Adapter
        if (process != null)
        {
            return new ProcessAdapter(process);
        }

        return null;
    }

    /// <summary>
    /// Prüft, ob aktuell mindestens eine Instanz eines Prozesses mit dem angegebenen Namen läuft.
    /// </summary>
    /// <param name="name">Der freundliche Name des Prozesses (ohne die Erweiterung .exe).</param>
    /// <returns><c>true</c>, wenn der Prozess läuft; andernfalls <c>false</c>.</returns>
    public bool IsProcessRunning(string name)
    {
        // Leitet direkt an die statische .NET API weiter
        return Process.GetProcessesByName(name).Length > 0;
    }

    /// <summary>
    /// Beendet alle laufenden Instanzen des angegebenen Prozesses sofort (harter Kill).
    /// </summary>
    /// <param name="name">Der Name des Prozesses, der beendet werden soll.</param>
    /// <remarks>
    /// Diese Methode enthält <b>kein Exception-Handling</b>. Fehler (z.B. "Zugriff verweigert")
    /// werden an den Aufrufer (den Service) weitergereicht und müssen dort behandelt werden.
    /// </remarks>
    public void KillProcess(string name)
    {
        var processes = Process.GetProcessesByName(name);
        foreach (var process in processes)
        {
            process.Kill();
        }
    }

    /// <summary>
    /// Holt alle laufenden Prozesse und verpackt sie in unsere testbaren Adapter.
    /// </summary>
    public IProcess[] GetProcesses()
    {
        var processes = Process.GetProcesses();

        // HIER IST DIE LÖSUNG: Wir nehmen jeden echten Prozess (p) und stecken ihn in den ProcessAdapter.
        // Das Ergebnis wandeln wir in ein Array von IProcess um.
        return processes.Select(p => (IProcess)new ProcessAdapter(p)).ToArray();
    }
}
