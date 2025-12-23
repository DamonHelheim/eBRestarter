using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Application.Services.Ports.Interfaces
{
    public interface IProcessControlService
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
