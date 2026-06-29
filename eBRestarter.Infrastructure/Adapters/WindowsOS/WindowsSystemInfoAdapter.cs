using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace eBRestarter.Infrastructure.Adapters.WindowsOS;

[SupportedOSPlatform("windows")]
public sealed class WindowsSystemInfoAdapter(ILogger<WindowsSystemInfoAdapter> logger, ISettingsPort registry) : ISystemInfoPort
{
    private const string RegistryPathUserChoiceHttp = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
    private const string RegistryPathUserChoiceHttps = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice";
    private const string RegistryPathCurrentVersion = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

    private readonly ILogger<WindowsSystemInfoAdapter> _logger = logger;
    private readonly ISettingsPort _registry = registry;

    public string RetrieveCurrentOsDisplayVersion()
    {
        try
        {
            var versionObj = _registry.GetSystemValue(RegistryPathCurrentVersion, "DisplayVersion");

            var version = versionObj?.ToString();

            return !string.IsNullOrWhiteSpace(version) ? version : "Unknown";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading the OS display version.");

            return "Error";
        }
    }

    public string RetrieveCurrentOsBuildVersion()
    {
        try
        {
            var ubrObj = _registry.GetSystemValue(RegistryPathCurrentVersion, "UBR");

            string ubr = ubrObj?.ToString() ?? "0";

            return $"{Environment.OSVersion.Version.Build}.{ubr}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading the OS build version.");

            return "Error";
        }
    }

    public string RetrieveCurrentStandardBrowserName()
    {
        string? progIdHttp = GetRegistryValueAsString(RegistryPathUserChoiceHttp, "ProgId");
        string? progIdHttps = GetRegistryValueAsString(RegistryPathUserChoiceHttps, "ProgId");

        if (string.IsNullOrEmpty(progIdHttp) || string.IsNullOrEmpty(progIdHttps))
        {
            return "-";
        }

        if (!string.Equals(progIdHttp, progIdHttps, StringComparison.OrdinalIgnoreCase))
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Different browsers detected for HTTP ({Http}) and HTTPS ({Https}).", progIdHttp, progIdHttps);
            }
        }

        // 2. Mapping
        return IdentifyBrowserByProgId(progIdHttp);
    }

    /// <summary>
    /// Helper method that utilizes the wrapper and casts directly into a string, including error handling.
    /// </summary>
    private string? GetRegistryValueAsString(string subKey, string valueName)
    {
        try
        {
            return _registry.GetUserValue(subKey, valueName)?.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to read the registry path: {Path}", subKey);
            return null;
        }
    }

    /// <summary>
    /// Pure logic method (Static, making it easily testable or used internally here).
    /// </summary>
    private static string IdentifyBrowserByProgId(string progId)
    {
        if (progId.Contains("ChromeHTML", StringComparison.OrdinalIgnoreCase)) return "Chrome";
        if (progId.Contains("Firefox", StringComparison.OrdinalIgnoreCase)) return "Firefox";
        if (progId.Contains("MSEdge", StringComparison.OrdinalIgnoreCase)) return "Edge";
        if (progId.Contains("Opera", StringComparison.OrdinalIgnoreCase)) return "Opera";
        if (progId.Contains("Brave", StringComparison.OrdinalIgnoreCase)) return "Brave";

        return "-";
    }

    public bool IsUserAdministrator()
    {
        using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }
}


