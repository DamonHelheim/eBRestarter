using System.IO;

namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;

/// <summary>
/// Port-Interface fÃ¼r den Zugriff auf das Windows-Dateisystem.
/// Kapselt System.IO Aufrufe fÃ¼r maximale Testbarkeit (Mocking).
/// </summary>
public interface IWindowsFileSystemService
{
    bool FileExists(string path);
    bool DirectoryExists(string path);
    string CombinePaths(params string[] paths);
    string ResolveEnvironmentPath(string variable);
    void DeleteFile(string path);
    void WriteAllText(string path, string content);
    string ReadAllText(string path);
    string[] ReadAllLines(string path);
    
    // NEU: Erweiterte Methoden fÃ¼r strikte hexagonale Isolation
    void CreateDirectory(string path);
    string? GetDirectoryName(string path);
    Stream OpenRead(string path);
}
