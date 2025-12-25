using eBRestarter.Infrastructure.Wrapper.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace eBRestarter.Infrastructure.Wrapper
{
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
        public Process? Start(ProcessStartInfo info) => Process.Start(info);

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
        /// Erstellt ein Array neuer <see cref="Process"/>-Komponenten und ordnet sie 
        /// allen Prozessressourcen zu, die aktuell auf dem lokalen Computer ausgeführt werden.
        /// </summary>
        /// <returns>Ein Array vom Typ <see cref="Process"/>, das alle laufenden Prozesse repräsentiert.</returns>
        public Process[] GetProcesses() => Process.GetProcesses();
    }
}
