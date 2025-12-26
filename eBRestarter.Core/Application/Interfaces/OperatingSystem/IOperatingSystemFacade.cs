using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.OperatingSystem
{
    public interface IOperatingSystemFacade
    {
        // Wir stellen die einzelnen Services als Properties zur Verfügung
        IWindowsProcessControlService WindowsProcessControlService { get; }
        IWindowsSystemInfoService WindowsSystemInfoService { get; }
        IWindowsRegistryService WindowsRegistryService { get; }
        IWindowsStartupManagerService WindowsStartupManagerService { get; }
        IWindowsFileSystemService WindowsFileSystemService { get; }
    }
}
