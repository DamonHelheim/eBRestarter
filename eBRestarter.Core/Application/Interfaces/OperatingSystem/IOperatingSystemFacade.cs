using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;

namespace eBRestarter.Core.Application.Interfaces.OperatingSystem;

public interface IOperatingSystemFacade
{
    // Wir stellen die einzelnen Services als Properties zur VerfÃ¼gung
    IWindowsProcessControlService WindowsProcessControlService { get; }
    IWindowsSystemInfoService WindowsSystemInfoService { get; }
    IWindowsRegistryService WindowsRegistryAdapter { get; }
    IWindowsStartupManagerService WindowsStartupManagerService { get; }
    IWindowsFileSystemService WindowsFileSystemServiceAdapter { get; }
    IWindowsAutoLogonService WindowsAutoLogonAdapter { get; }
}



