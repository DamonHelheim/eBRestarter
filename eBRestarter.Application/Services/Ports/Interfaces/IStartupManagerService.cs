using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Application.Services.Ports.Interfaces
{
    public interface IStartupManagerService
    {
        void EnableAutoStart();
        void DisableAutoStart();
        Dictionary<string, object> GetStartupEntries();
        // Edge & AutoLogon Logik passt hier gut rein oder in einen "SystemConfigService"
        void SetEdgeStartupBoost(bool enable);
        void SetAutoLogon(bool enable);
    }
}
