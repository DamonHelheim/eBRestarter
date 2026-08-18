using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.BehavioralComponents.Extensions;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;

/// <summary>
/// Adapter: Base Driven Adapter (Outbound) encapsulating common browser management, file paths, and OS process operations.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Abstract base adapter in the Infrastructure layer for browser-specific implementations.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortBrowser"/>.<br/>
/// </para>
/// </summary>
/// <param name="processControlPort">OS process control port.</param>
/// <param name="settingsPort">System configuration repository port.</param>
/// <param name="fileSystemPort">File system operations port.</param>
/// <param name="logger">Logger instance.</param>
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

    // ── Block 2: Primitives & strings ──
    private const string CurrentVersionValueName = "CurrentVersion";
    private const string DefaultVersion = "Unknown";
    private const string InstallLocationValueName = "InstallLocation";
    private const string VersionValueName = "version";


    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injected dependencies ──
    protected readonly IOutboundPortFileSystem _fileSystemPort = fileSystemPort;
    protected readonly ILogger _logger = logger;
    protected readonly IOutboundPortOsProcessControl _processControlPort = processControlPort;
    protected readonly IOutboundPortSystemConfigurationRepository _settingsPort = settingsPort;


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitives & strings ──
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

    // ── Block 3: Enums ──
    public abstract BrowserType Type { get; }

    // ── Block 4: Complex types & collections ──
    protected abstract List<string> ExecutablePaths { get; }


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Closes all active processes of the browser.
    /// </summary>
    public virtual void Close()
    {
        _processControlPort.CloseApplication(ProcessName);
    }

    /// <summary>
    /// Determines whether the specified browser extension is installed.
    /// </summary>
    /// <param name="extensionId">Optional extension ID string.</param>
    public abstract bool IsExtensionInstalled(string? extensionId = null);

    /// <summary>
    /// Resolves operational filesystem paths for the browser.
    /// </summary>
    public abstract BrowserPaths ResolvePaths();

    /// <summary>
    /// Starts the browser with the specified target URL and arguments.
    /// </summary>
    /// <param name="url">Target URL string.</param>
    /// <param name="arguments">Optional additional command-line arguments.</param>
    /// <inheritdoc />
    /// <remarks>
    /// Exception &amp; Return Rationale: Returns boolean indicating whether browser start succeeded instead of swallowing exceptions silently.
    /// Uses TryRetrieveExecutablePath to safely determine whether an executable exists prior to launching.
    /// </remarks>
    public virtual bool Start(string url, string arguments = "")
    {
        // ⚠️ Kap. 9: Guard Clause am Methodenbeginn. Ohne sie schlug ein null-Argument erst bei
        // url.Trim() zu und wurde vom catch unten als "Error while starting" fehletikettiert.
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        if (!TryRetrieveExecutablePath(out string? exePath))
        {
            _logger.LogWarning(
                LogEventIds.Browser.BrowserStartFailed,
                "{BrowserType} was not started because no installed executable could be located.",
                Type);

            return false;
        }

        try
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                // Redacts sensitive username segments from log output.
                _logger.LogDebug(
                    LogEventIds.Browser.BrowserStarting,
                    "Starting {BrowserType} with URL {Url}...",
                    Type,
                    LogRedaction.MaskUrlUserSegment(url));
            }

            // Encloses URL in quotes to enforce single-argument parsing and prevent command injection.
            var quotedUrl = $"\"{url.Trim()}\"";

            var finalArguments = string.IsNullOrWhiteSpace(arguments)
                ? quotedUrl
                : $"{quotedUrl} {arguments}".Trim();

            _processControlPort.OpenUrlInBrowser(exePath, finalArguments);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.Browser.BrowserStartFailed, exception, "Error while starting {BrowserType}", Type);

            return false;
        }
    }

    /// <summary>
    /// Discovers browser executable path from Windows registry uninstall keys.
    /// </summary>
    /// <param name="paths">Path list to populate.</param>
    /// <param name="subKey">Registry uninstall subkey path.</param>
    /// <param name="exeName">Executable file name.</param>
    /// <param name="isHklm"><see langword="true"/> if checking HKLM; otherwise HKCU.</param>
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

    /// <summary>
    /// Sanitizes a raw version string extracted from registry keys.
    /// </summary>
    /// <param name="raw">Raw version string.</param>
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

    /// <summary>
    /// Attempts to locate an installed executable for this browser.
    /// </summary>
    /// <param name="executablePath">The first existing executable path, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if an installed executable was found.</returns>
    /// <remarks>
    /// TryGet Rationale: Follows TryGet pattern for safe non-throwing executable path discovery.
    /// </remarks>
    protected bool TryRetrieveExecutablePath([NotNullWhen(true)] out string? executablePath)
    {
        executablePath = ExecutablePaths.FirstOrDefault(path => _fileSystemPort.FileExists(path));

        return executablePath is not null;
    }

    [GeneratedRegex(@"^[\d\.]+", RegexOptions.NonBacktracking, matchTimeoutMilliseconds: 100)]
    private static partial Regex VersionRegex();
}
