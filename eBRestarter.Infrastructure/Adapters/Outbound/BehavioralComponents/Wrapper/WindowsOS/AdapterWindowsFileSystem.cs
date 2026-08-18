using System;
using System.IO;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) encapsulating Windows file system I/O operations.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Encapsulates Windows file system operations (<see cref="File"/>, <see cref="Directory"/>, <see cref="Path"/>) in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortFileSystem"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterWindowsFileSystem : IOutboundPortFileSystem
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitives & strings ──
    private const string AppDataEnvironmentVariableName = "appdata";
    private const string LocalAppDataEnvironmentVariableName = "localappdata";
    private const string ProgramFilesEnvironmentVariableName = "programfiles";
    private const string ProgramFilesX86EnvironmentVariableName = "programfiles(x86)";


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <inheritdoc />
    public string CombinePaths(params string[] paths) => Path.Combine(paths);

    /// <inheritdoc />
    public void CreateDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Directory.CreateDirectory(path);
    }

    /// <inheritdoc />
    public void DeleteFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <inheritdoc />
    public bool DirectoryExists(string path) => Directory.Exists(path);

    /// <inheritdoc />
    public bool FileExists(string path) => File.Exists(path);

    /// <inheritdoc />
    public string[] GetDirectories(string path, string searchPattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Directory.GetDirectories(path, searchPattern);
    }

    /// <inheritdoc />
    public string? GetDirectoryName(string path) => Path.GetDirectoryName(path);

    /// <inheritdoc />
    public Stream OpenRead(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return File.OpenRead(path);
    }

    /// <inheritdoc />
    public string[] ReadAllLines(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return File.ReadAllLines(path);
    }

    /// <inheritdoc />
    public string ReadAllText(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return File.ReadAllText(path);
    }

    /// <inheritdoc />
    public string ResolveEnvironmentPath(string variable)
    {
        var path = Environment.GetEnvironmentVariable(variable);

        if (!string.IsNullOrEmpty(path))
        {
            return path;
        }

        // OrdinalIgnoreCase avoids heap allocations and provides culture-agnostic string comparison.
        if (string.Equals(variable, AppDataEnvironmentVariableName, StringComparison.OrdinalIgnoreCase))
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        if (string.Equals(variable, LocalAppDataEnvironmentVariableName, StringComparison.OrdinalIgnoreCase))
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        if (string.Equals(variable, ProgramFilesEnvironmentVariableName, StringComparison.OrdinalIgnoreCase))
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }

        if (string.Equals(variable, ProgramFilesX86EnvironmentVariableName, StringComparison.OrdinalIgnoreCase))
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        }

        return string.Empty;
    }

    /// <inheritdoc />
    public void WriteAllText(string path, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        File.WriteAllText(path, content);
    }
}
