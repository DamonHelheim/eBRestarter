using eBRestarter.Application.Services.Ports.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Application.Facade.Interfaces
{
    // eBRestarter.Application.Interfaces.Facades
    public interface IOperatingSystemFacade
    {
        // Wir stellen die einzelnen Services als Properties zur Verfügung
        IProcessControlService Process { get; }
        ISystemInfoService SystemInfo { get; }
        IRegistryService Registry { get; }
        IStartupManagerService Startup { get; }
        IFileSystemService FileSystem { get; }
    }
}
