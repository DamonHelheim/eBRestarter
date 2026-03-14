namespace eBRestarter.Core.Application.Interfaces;

public interface IPathProvider
{
    string GetAppDataDirectory();
    string GetLocalAppDataDirectory();
    string GetUserProfileDirectory();
    string GetProgramFilesDirectory();
    string GetProgramFilesX86Directory();
}
