using eBRestarter.Infrastructure.Wrapper.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace eBRestarter.Infrastructure.Wrapper
{
    public class RealProcessWrapper : IProcessWrapper
    {
        // 1. Prozess starten (existierte schon)
        public void Start(ProcessStartInfo info) => Process.Start(info);

        // 2. Prozesse abrufen (existierte schon)
        public Process[] GetProcessesByName(string name) => Process.GetProcessesByName(name);

        // 3. Prüfen, ob Prozess läuft
        public bool IsProcessRunning(string name)
        {
            // Wir nutzen einfach die bestehende .NET API
            return Process.GetProcessesByName(name).Length > 0;
        }

        // 4. Prozess killen
        public void KillProcess(string name)
        {
            // Da die Methode "KillProcess" heißt, aber "name" (String) als Parameter nimmt,
            // müssen wir hier intern die Prozesse suchen und beenden.
            var processes = Process.GetProcessesByName(name);

            foreach (var process in processes)
            {
                // Wir rufen hier stumpf Kill auf.
                // Das Error-Handling (try/catch) bleibt im SERVICE (WindowsProcessService),
                // damit der Wrapper so "dumm" und einfach wie möglich bleibt.
                process.Kill();
            }
        }
    }
}
