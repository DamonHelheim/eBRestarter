using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Enums
{
    public enum RestartTaskState
    {
        Idle,           // Nichts läuft
        InitialDelay,   // "StartRestartTimer" (Die ersten 5 Sekunden)
        Running,        // "mainTimer" (Browser ist offen)
        Cooldown        // "BrowserStartsInTimer" (Pause / Neustart in...)
    }
}
