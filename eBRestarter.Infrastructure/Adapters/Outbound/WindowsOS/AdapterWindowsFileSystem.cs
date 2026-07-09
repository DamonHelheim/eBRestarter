using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) encapsulating Windows file system I/O operations.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core durch Kapselung von <see cref="File"/>, <see cref="Directory"/> und <see cref="Path"/>.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortFileSystem"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um Dateisystemzugriffe auszuführen.
/// </para>
/// </summary>
public sealed class AdapterWindowsFileSystem : IOutboundPortFileSystem
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
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
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

    public string[] GetDirectories(string path, string searchPattern)
    {
        return Directory.GetDirectories(path, searchPattern);
    }
}
