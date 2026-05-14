namespace eBRestarter.Core.Application.Interfaces;

public interface IPathProvider
{
    string RetrieveAppDataDirectory();
    string RetrieveLocalAppDataDirectory();
    string RetrieveUserProfileDirectory();
    string RetrieveProgramFilesDirectory();
    string RetrieveProgramFilesX86Directory();
}
