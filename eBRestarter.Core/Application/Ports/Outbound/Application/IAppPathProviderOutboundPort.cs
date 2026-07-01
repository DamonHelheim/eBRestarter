namespace eBRestarter.Core.Application.Ports.Outbound.Application;

public interface IAppPathProviderOutboundPort
{
    string RetrieveAppDataDirectory();
    string RetrieveLocalAppDataDirectory();
    string RetrieveUserProfileDirectory();
    string RetrieveProgramFilesDirectory();
    string RetrieveProgramFilesX86Directory();
}


