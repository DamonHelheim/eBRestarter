using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS
{
    public interface IWindowsStartupManagerService
    {
        void EnableAutoStart();
        void DisableAutoStart();
        Dictionary<string, object> GetStartupEntries();
        // Edge & AutoLogon Logik passt hier gut rein oder in einen "SystemConfigService"
        void SetEdgeStartupBoost(bool enable);
        void SetAutoLogon(bool enable);
    }
}
