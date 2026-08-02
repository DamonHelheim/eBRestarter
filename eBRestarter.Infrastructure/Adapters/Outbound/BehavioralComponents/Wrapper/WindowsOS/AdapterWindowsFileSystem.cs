using System;
using System.IO;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.WindowsOS;

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
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    private const string AppDataEnvironmentVariableName = "appdata";
    private const string LocalAppDataEnvironmentVariableName = "localappdata";
    private const string ProgramFilesEnvironmentVariableName = "programfiles";
    private const string ProgramFilesX86EnvironmentVariableName = "programfiles(x86)";


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    public string CombinePaths(params string[] paths) => Path.Combine(paths);

    public void CreateDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Directory.CreateDirectory(path);
    }

    public void DeleteFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public bool FileExists(string path) => File.Exists(path);

    public string[] GetDirectories(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Directory.GetDirectories(path, searchPattern);
    }

    public string? GetDirectoryName(string path) => Path.GetDirectoryName(path);

    public Stream OpenRead(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return File.OpenRead(path);
    }

    public string[] ReadAllLines(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return File.ReadAllLines(path);
    }

    public string ReadAllText(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return File.ReadAllText(path);
    }

    public string ResolveEnvironmentPath(string variable)
    {
        var path = Environment.GetEnvironmentVariable(variable);

        if (!string.IsNullOrEmpty(path))
        {
            return path;
        }

        return variable.ToLowerInvariant() switch
        {
            AppDataEnvironmentVariableName => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            LocalAppDataEnvironmentVariableName => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ProgramFilesEnvironmentVariableName => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            ProgramFilesX86EnvironmentVariableName => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            _ => string.Empty
        };
    }

    public void WriteAllText(string path, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        File.WriteAllText(path, content);
    }
}
