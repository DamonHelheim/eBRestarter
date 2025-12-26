using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Facade
{
    public class OperatingSystemFacade : IOperatingSystemFacade
    {
        public IWindowsProcessControlService WindowsProcessControlService { get; }
        public IWindowsSystemInfoService WindowsSystemInfoService { get; }
        public IWindowsRegistryService WindowsRegistryService { get; }
        public IWindowsStartupManagerService WindowsStartupManagerService { get; }
        public IWindowsFileSystemService WindowsFileSystemService { get; }

        // Constructor Injection: Der Container füllt hier die 3 Services ein
        public OperatingSystemFacade(
            IWindowsProcessControlService windowsProcessControlService,
            IWindowsSystemInfoService windowsSystemInfoService,
            IWindowsRegistryService windowsRegistryService,
            IWindowsStartupManagerService windowsStartupManagerService,
            IWindowsFileSystemService windowsFileSystemService)
        {
            WindowsProcessControlService = windowsProcessControlService;
            WindowsSystemInfoService = windowsSystemInfoService;
            WindowsRegistryService = windowsRegistryService;
            WindowsStartupManagerService = windowsStartupManagerService;
            WindowsFileSystemService = windowsFileSystemService;
        }


    }
}
