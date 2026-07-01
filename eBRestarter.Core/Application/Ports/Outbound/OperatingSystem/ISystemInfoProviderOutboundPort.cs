namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

public interface ISystemInfoProviderOutboundPort
{
    string RetrieveCurrentStandardBrowserName();
    string RetrieveCurrentOsBuildVersion();
    string RetrieveCurrentOsDisplayVersion();
    bool IsUserAdministrator();
}


