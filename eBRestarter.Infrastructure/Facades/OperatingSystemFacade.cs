using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;

namespace eBRestarter.Infrastructure.Facades;

public class OperatingSystemFacade(
    IWindowsProcessControlService windowsProcessControlService,
    IWindowsSystemInfoService windowsSystemInfoService,
    IWindowsRegistryService windowsRegistryService,
    IWindowsStartupManagerService windowsStartupManagerService,
    IWindowsFileSystemService windowsFileSystemService) : IOperatingSystemFacade
{
    public IWindowsProcessControlService WindowsProcessControlService { get; } = windowsProcessControlService;
    public IWindowsSystemInfoService WindowsSystemInfoService { get; } = windowsSystemInfoService;
    public IWindowsRegistryService WindowsRegistryService { get; } = windowsRegistryService;
    public IWindowsStartupManagerService WindowsStartupManagerService { get; } = windowsStartupManagerService;
    public IWindowsFileSystemService WindowsFileSystemService { get; } = windowsFileSystemService;
}
