using eBRestarter.Core.Application.Ports.Outbound.Application;

namespace eBRestarter.Infrastructure.Adapters.Providers.WindowsOS;

public sealed class WindowsAppPathProviderAdapter : IAppPathPort
{
    public WindowsAppPathProviderAdapter()
    {
    }

    public string RetrieveAppDataDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public string RetrieveLocalAppDataDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public string RetrieveUserProfileDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public string RetrieveProgramFilesDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    public string RetrieveProgramFilesX86Directory() => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
}





