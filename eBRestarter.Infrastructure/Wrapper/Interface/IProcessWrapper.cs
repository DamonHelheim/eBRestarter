using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace eBRestarter.Infrastructure.Wrapper.Interface
{
    public interface IProcessWrapper
    {
        Process[] GetProcessesByName(string name);
        void Start(ProcessStartInfo info);
        // NEU: Abstraktion für Prozess-Abfrage
        bool IsProcessRunning(string name);
        // NEU: Abstraktion fürs Killen
        void KillProcess(string name);
        
    }
}
