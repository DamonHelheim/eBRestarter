using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;

namespace eBRestarter.Infrastructure.Services.WindowsOS
{
    [SupportedOSPlatform("windows")]
    public class WindowsFileSystemService : IWindowsFileSystemService
    {
        public bool FileExists(string path) => File.Exists(path);
        public bool DirectoryExists(string path) => Directory.Exists(path);
        public string CombinePaths(params string[] paths) => Path.Combine(paths);
        public string GetEnvironmentPath(string variable)
        {
            // Deine Logik aus dem alten Helper
            var path = Environment.GetEnvironmentVariable(variable);

            if (!string.IsNullOrEmpty(path)) return path;

            return variable.ToLower() switch
            {
                "appdata" => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "localappdata" => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "programfiles" => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "programfiles(x86)" => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                _ => string.Empty
            };
        }
    }
}
