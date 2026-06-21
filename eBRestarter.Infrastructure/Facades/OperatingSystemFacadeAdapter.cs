using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;

namespace eBRestarter.Infrastructure.Facades;

public class OperatingSystemFacadeAdapter(
    IWindowsProcessControlService windowsProcessControlService,
    IWindowsSystemInfoService windowsSystemInfoService,
    IWindowsRegistryService WindowsRegistryAdapter,
    IWindowsStartupManagerService windowsStartupManagerService,
    IWindowsFileSystemService WindowsFileSystemServiceAdapter,
    IWindowsAutoLogonService WindowsAutoLogonAdapter) : IOperatingSystemFacade
{
    public IWindowsProcessControlService WindowsProcessControlService { get; } = windowsProcessControlService;
    public IWindowsSystemInfoService WindowsSystemInfoService { get; } = windowsSystemInfoService;
    public IWindowsRegistryService WindowsRegistryAdapter { get; } = WindowsRegistryAdapter;
    public IWindowsStartupManagerService WindowsStartupManagerService { get; } = windowsStartupManagerService;
    public IWindowsFileSystemService WindowsFileSystemServiceAdapter { get; } = WindowsFileSystemServiceAdapter;
    public IWindowsAutoLogonService WindowsAutoLogonAdapter { get; } = WindowsAutoLogonAdapter;
}





