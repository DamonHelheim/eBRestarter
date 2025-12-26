using eBRestarter.Application.Facade.Interfaces;
using eBRestarter.Application.Services.Ports.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Application.Facade
{
    public class OperatingSystemFacade : IOperatingSystemFacade
    {
        public IProcessControlService Process { get; }
        public ISystemInfoService SystemInfo { get; }
        public IRegistryService Registry { get; }
        public IStartupManagerService Startup { get; }
        public IFileSystemService FileSystem { get; }

        // Constructor Injection: Der Container füllt hier die 3 Services ein
        public OperatingSystemFacade(
            IProcessControlService process,
            ISystemInfoService systemInfo,
            IRegistryService registry,
            IStartupManagerService startup,
            IFileSystemService fileSystem)
        {
            Process = process;
            SystemInfo = systemInfo;
            Registry = registry;
            Startup = startup;
            FileSystem = fileSystem;
        }
    }
}
