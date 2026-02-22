using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using System.Runtime.Versioning;

namespace eBRestarter.Infrastructure.Services.WindowsOS;

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

    /// <summary>
    /// Löscht die Datei am angegebenen Pfad.
    /// </summary>
    /// <param name="path">Der vollständige Pfad zur Datei.</param>
    /// <exception cref="ArgumentException">Wenn der Pfad leer ist.</exception>
    /// <exception cref="IOException">Wenn die Datei gerade verwendet wird.</exception>
    /// <exception cref="UnauthorizedAccessException">Wenn Schreibrechte fehlen.</exception>
    public void DeleteFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Pfad darf nicht leer sein.", nameof(path));
        }

        // File.Delete wirft keine Exception, falls die Datei schon weg ist.
        // Das spart uns eine "if (Exists)" Abfrage und Race-Conditions.
        File.Delete(path);
    }

    public void WriteAllText(string path, string content)
    {
        File.WriteAllText(path, content);
    }

    public string ReadAllText(string path)
    {
        var content = File.ReadAllText(path);

        return content;
    }
}
