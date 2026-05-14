using eBRestarter.Core.Application.Interfaces;

namespace eBRestarter.Infrastructure.Services.WindowsOS;

public class WindowsPathProvider : IPathProvider
{
    public string RetrieveAppDataDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public string RetrieveLocalAppDataDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public string RetrieveUserProfileDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public string RetrieveProgramFilesDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    public string RetrieveProgramFilesX86Directory() => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
}
