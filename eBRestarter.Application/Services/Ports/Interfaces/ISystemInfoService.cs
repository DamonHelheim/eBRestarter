using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Application.Services.Ports.Interfaces
{
    public interface ISystemInfoService
    {
        string GetCurrentStandardBrowserName(); // Zusammenfassung deiner Logik für Chrome/Firefox
        string GetCurrentOsBuildVersion();
        string GetCurrentOsDisplayVersion();
    }
}
