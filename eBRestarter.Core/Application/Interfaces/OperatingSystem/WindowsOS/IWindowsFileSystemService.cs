using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS
{
    public interface IWindowsFileSystemService
    {
        bool FileExists(string path);
        bool DirectoryExists(string path);
        string GetEnvironmentPath(string variable); // Z.B. für %AppData%
        string CombinePaths(params string[] paths);
    }
}
