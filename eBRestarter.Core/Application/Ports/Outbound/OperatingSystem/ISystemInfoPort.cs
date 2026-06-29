namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

public interface ISystemInfoPort
{
    string RetrieveCurrentStandardBrowserName();
    string RetrieveCurrentOsBuildVersion();
    string RetrieveCurrentOsDisplayVersion();
    bool IsUserAdministrator();
}


