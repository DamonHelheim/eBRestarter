using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS
{
    public interface IWindowsProcessControlService
    {
        void StartMsiFile(string msiFilePath);
        void StartExecutable(string exeFilePath);
        void OpenUrlInBrowser(string url);
        void CloseApplication(string processName);
        void CloseAllOpenPrograms();
        void ShutdownComputer();
        bool IsProcessAlive(string processName);
    }
}
