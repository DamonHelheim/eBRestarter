using System;
using eBRestarter.Core.Application.Interfaces;

namespace eBRestarter.Infrastructure.Services.WindowsOS;

public class WindowsPathProvider : IPathProvider
{
    public string GetAppDataDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    
    public string GetLocalAppDataDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    
    public string GetUserProfileDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    
    public string GetProgramFilesDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
    
    public string GetProgramFilesX86Directory() => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
}
