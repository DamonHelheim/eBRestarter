using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;

/// <summary>
/// Adapter: Base Driven Adapter (Outbound) encapsulating common browser management, file paths, and OS process operations.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Dient als technologische Basisklasse im äußeren Ring (Infrastructure Layer) für alle spezifischen Browser-Adapter.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortBrowser"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt und einen Outbound Port implementiert, um OS-spezifische Browser-Operationen (Starten, Stoppen, Pfadermittlung) auszuführen.
/// </para>
/// </summary>
public abstract partial class AdapterBrowserBaseWrapper(
    IOutboundPortOsProcessControl processControlPort,
    IOutboundPortSystemConfigurationRepository settingsPort,
    IOutboundPortFileSystem fileSystemPort,
    ILogger logger)
    : IOutboundPortBrowser
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    private const string CurrentVersionValueName = "CurrentVersion";
    private const string DefaultVersion = "Unknown";
    private const string InstallLocationValueName = "InstallLocation";
    private const string VersionValueName = "version";


    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch) ──
    protected readonly IOutboundPortFileSystem _fileSystemPort = fileSystemPort;
    protected readonly ILogger _logger = logger;
    protected readonly IOutboundPortOsProcessControl _processControlPort = processControlPort;
    protected readonly IOutboundPortSystemConfigurationRepository _settingsPort = settingsPort;


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    public bool IsInstalled => ExecutablePaths.Any(path => _fileSystemPort.FileExists(path));

    public virtual string BrowserVersion
    {
        get
        {
            var rawVersionValue = _settingsPort.GetUserValue(RegistryKeyVersion, VersionValueName)
                                  ?? _settingsPort.GetUserValue(RegistryKeyVersion, CurrentVersionValueName);

            return CleanVersionString(rawVersionValue?.ToString());
        }
    }

    public abstract string DisplayName { get; }
    public abstract string DownloadUrl { get; }
    public abstract string ExtensionInstallUrl { get; }
    public abstract string IconPath { get; }
    public abstract string ProcessName { get; }
    protected abstract string RegistryKeyVersion { get; }

    // ── Block 3: Enums (alphabetisch) ──
    public abstract BrowserType Type { get; }

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente (alphabetisch) ──
    protected abstract List<string> ExecutablePaths { get; }


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    public virtual void Close()
    {
        _processControlPort.CloseApplication(ProcessName);
    }

    public abstract bool IsExtensionInstalled(string? extensionId = null);

    public abstract BrowserPaths ResolvePaths();

    public virtual void Start(string url, string arguments = "")
    {
        try
        {
            var exePath = RetrieveExecutablePath();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Starting {BrowserType} with URL {Url}...", Type, url);
            }

            var finalArguments = string.IsNullOrWhiteSpace(arguments)
                ? url.Trim()
                : $"{url} {arguments}".Trim();

            _processControlPort.OpenUrlInBrowser(exePath, finalArguments);
        }
        catch (Exception exception)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(exception, "Error while starting {BrowserType}", Type);
            }
        }
    }

    protected void AddPathFromUninstallKey(
        List<string> paths,
        string subKey,
        string exeName,
        bool isHklm)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var installLocationValue = isHklm
            ? _settingsPort.GetSystemValue(subKey, InstallLocationValueName)
            : _settingsPort.GetUserValue(subKey, InstallLocationValueName);

        if (installLocationValue?.ToString() is not { Length: > 0 } installLocation)
        {
            return;
        }

        var fullPath = _fileSystemPort.CombinePaths(installLocation, exeName);
        paths.Add(fullPath);
    }

    protected virtual string CleanVersionString(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return DefaultVersion;
        }

        var cleanRaw = raw.Trim();

        try
        {
            var match = VersionRegex().Match(cleanRaw);

            if (match.Success)
            {
                return match.Value;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return DefaultVersion;
        }

        return raw;
    }

    protected string RetrieveExecutablePath()
    {
        return ExecutablePaths.FirstOrDefault(path => _fileSystemPort.FileExists(path))
               ?? throw new FileNotFoundException($"{Type} executable not found.");
    }

    [GeneratedRegex(@"^[\d\.]+", RegexOptions.NonBacktracking, matchTimeoutMilliseconds: 100)]
    private static partial Regex VersionRegex();
}
