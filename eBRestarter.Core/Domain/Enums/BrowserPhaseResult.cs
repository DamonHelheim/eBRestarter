using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Enums
{
    public enum BrowserPhaseResult
    {
        Completed,     // Die Laufzeit ist ganz normal abgelaufen
        BrowserClosed, // Der CheckBrowserAliveRoutine hat gegriffen (Browser tot)
        CleanupDue     // 0:00 Uhr wurde erreicht, sofort aufräumen!
    }
}
