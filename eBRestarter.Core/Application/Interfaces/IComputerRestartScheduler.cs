using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces
{
    public interface IComputerRestartScheduler
    {
        // Startet die Überwachung im Hintergrund
        void StartScheduler();

        // Stoppt die Überwachung
        Task StopSchedulerAsync();
    }
}
