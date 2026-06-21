using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using System;
using System.IO;

namespace eBRestarter.Infrastructure.Services.WindowsOS;

/// <summary>
/// Implementiert den Zugriff auf das Windows-Dateisystem.
/// </summary>
public class WindowsFileSystemServiceAdapter : IWindowsFileSystemService
{
    public bool FileExists(string path) => File.Exists(path);
    
    public bool DirectoryExists(string path) => Directory.Exists(path);
    
    public string CombinePaths(params string[] paths) => Path.Combine(paths);
    
    public string ResolveEnvironmentPath(string variable)
    {
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

    public void DeleteFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Pfad darf nicht null oder leer sein.", nameof(path));
        }

        if (File.Exists(path)) File.Delete(path);
    }

    public void WriteAllText(string path, string content)
    {
        File.WriteAllText(path, content);
    }

    public string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    public string[] ReadAllLines(string path)
    {
        return File.ReadAllLines(path);
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public string? GetDirectoryName(string path)
    {
        return Path.GetDirectoryName(path);
    }

    public Stream OpenRead(string path)
    {
        return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}

