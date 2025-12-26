using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS
{
    public interface IWindowsSystemInfoService
    {
        string GetCurrentStandardBrowserName(); // Zusammenfassung deiner Logik für Chrome/Firefox
        string GetCurrentOsBuildVersion();
        string GetCurrentOsDisplayVersion();
    }
}
