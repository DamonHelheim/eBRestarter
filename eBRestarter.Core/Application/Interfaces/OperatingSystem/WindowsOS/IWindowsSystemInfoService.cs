namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;

public interface IWindowsSystemInfoService
{
    string RetrieveCurrentStandardBrowserName(); // Zusammenfassung deiner Logik für Chrome/Firefox
    string RetrieveCurrentOsBuildVersion();
    string RetrieveCurrentOsDisplayVersion();
    bool IsUserAdministrator();
}
