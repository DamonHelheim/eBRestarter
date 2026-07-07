using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters.Browsers;

/// <summary>
/// Adapter: Base Driven Adapter (Outbound) encapsulating common browser management, file paths, and OS process operations.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Dient als technologische Basisklasse im äußeren Ring (Infrastructure Layer) für alle spezifischen Browser-Adapter.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IBrowserOutboundPort"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt und einen Outbound Port implementiert, um OS-spezifische Browser-Operationen (Starten, Stoppen, Pfadermittlung) auszuführen.
/// </para>
/// </summary>
public abstract class BrowserBase(IOsProcessControlOutboundPort processControlPort, ISettingsRepositoryOutboundPort settingsPort, IFileSystemOutboundPort fileSystemPort, ILogger logger) : IBrowserOutboundPort
{
    protected readonly IOsProcessControlOutboundPort _processControlPort = processControlPort;
    protected readonly ISettingsRepositoryOutboundPort _settingsPort = settingsPort;
    protected readonly IFileSystemOutboundPort _fileSystemPort = fileSystemPort;
    protected readonly ILogger _logger = logger;

    public abstract BrowserType Type { get; }

    public abstract BrowserPaths ResolvePaths();

    public abstract bool IsExtensionInstalled(string? extensionId = null);
    public abstract string DisplayName { get; }
    public abstract string IconPath { get; }
    public abstract string DownloadUrl { get; }
    public abstract string ExtensionInstallUrl { get; }
    public abstract string ProcessName { get; }
    protected abstract string RegistryKeyVersion { get; }
    protected abstract List<string> ExecutablePaths { get; }

    public virtual string BrowserVersion
    {
        get
        {
            // Attempts to read registry values (Chrome and Firefox use different keys)
            var raw = _settingsPort.GetUserValue(RegistryKeyVersion, "version")
                      ?? _settingsPort.GetUserValue(RegistryKeyVersion, "CurrentVersion");

            return CleanVersionString(raw?.ToString());
        }
    }

    public bool IsInstalled => ExecutablePaths.Any(path => _fileSystemPort.FileExists(path));

    public virtual void Start(string url, string arguments = "")
    {
        try
        {
            var exePath = RetrieveExecutablePath();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Starting {BrowserType} with URL {Url}...", Type, url);
            }

            // We combine the URL and any additional arguments.
            // Browsers simply accept the URL as the first command-line argument.
            var finalArguments = $"{url} {arguments}".Trim();

            // NOW we launch the EXE and pass the URL along as an argument
            _processControlPort.OpenUrlInBrowser(exePath, finalArguments);
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(ex, "Error while starting {BrowserType}", Type);
            }
        }
    }

    public virtual void Close()
    {
        _processControlPort.CloseApplication(ProcessName);
    }

    protected string RetrieveExecutablePath()
    {
        // Finds the first path in the list that actually exists
        return ExecutablePaths.FirstOrDefault(path => _fileSystemPort.FileExists(path))
               ?? throw new FileNotFoundException($"{Type} executable not found.");
    }

    protected void AddPathFromUninstallKey(List<string> paths, string subKey, string exeName, bool isHklm)
    {
        var val = isHklm
            ? _settingsPort.GetSystemValue(subKey, "InstallLocation")
            : _settingsPort.GetUserValue(subKey, "InstallLocation");

        if (val is not null && !string.IsNullOrEmpty(val.ToString()))
        {
            var fullPath = _fileSystemPort.CombinePaths(val.ToString()!, exeName);
            paths.Add(fullPath);
        }
    }

    // Important to strip out suffixes like "(x64 en)" or "(x64 de)" from the Firefox version string.
    protected virtual string CleanVersionString(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return "Unknown";

        var cleanRaw = raw.Trim();

        try
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                cleanRaw,
                @"^[\d\.]+",
                System.Text.RegularExpressions.RegexOptions.NonBacktracking,
                TimeSpan.FromMilliseconds(100));

            if (match.Success)
            {
                return match.Value;
            }
        }
        catch (System.Text.RegularExpressions.RegexMatchTimeoutException)
        {
            return "Unknown";
        }

        return raw;
    }
}


