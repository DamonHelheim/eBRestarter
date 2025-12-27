using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces
{
    public interface IPathService
    {
        string GetAppDataPath();
        string GetDownloadsPath();
        string GetConfigFilePath();
        string GetLogFilePath();
        // Weitere Pfade bei Bedarf...
    }
}
