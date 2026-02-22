using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models.Records;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Browsers.Abstract;

public abstract class BrowserBase : IBrowser
{
    protected readonly IOperatingSystemFacade _os;
    protected readonly ILogger _logger;

    protected BrowserBase(IOperatingSystemFacade os, ILogger logger)
    {
        _os = os;
        _logger = logger;
    }

    public abstract BrowserType Type { get; }

    // Diese abstrakten Properties müssen Chrome/Firefox liefern
    public abstract string DisplayName { get; }
    public abstract string IconPath { get; }
    public abstract string DownloadUrl { get; }

    // --- NEU: Extension URL muss von konkreter Klasse geliefert werden ---
    public abstract string ExtensionInstallUrl { get; }
    protected abstract string ProcessName { get; }
    protected abstract string RegistryKeyVersion { get; }
    protected abstract List<string> ExecutablePaths { get; }

    // --- NEU: Extension Check muss von konkreter Klasse (oder Zwischenklasse) implementiert werden ---
    public abstract bool IsExtensionInstalled(string? extensionId = null);

    public virtual void Start(string url, string arguments = "")
    {
        try
        {
            var exePath = GetExecutablePath();
            _logger.LogInformation($"Starte {Type} mit URL {url}...");

            // Wir kombinieren die URL und evtl. zusätzliche Argumente
            // Browser akzeptieren die URL einfach als erstes Argument in der Kommandozeile.
            string finalArguments = $"{url} {arguments}".Trim();

            // JETZT rufen wir die EXE auf und geben die URL als Argument mit
            _os.WindowsProcessControlService.OpenUrlInBrowser(exePath, finalArguments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Fehler beim Starten von {Type}");
        }
    }

    public virtual void Close()
    {
        _os.WindowsProcessControlService.CloseApplication(ProcessName);
    }

    public virtual string BrowserVersion
    {
        get
        {
            // Versucht Registry-Werte zu lesen (Chrome und Firefox nutzen unterschiedliche Keys)
            var raw = _os.WindowsRegistryService.GetCurrentUserValue(RegistryKeyVersion, "version") ?? _os.WindowsRegistryService.GetCurrentUserValue(RegistryKeyVersion, "CurrentVersion");

            return CleanVersionString(raw?.ToString());
        }
    }

    public bool IsInstalled => ExecutablePaths.Any(path => _os.WindowsFileSystemService.FileExists(path));

    protected string GetExecutablePath()
    {
        // Sucht den ersten Pfad aus der Liste, der wirklich existiert
        return ExecutablePaths.FirstOrDefault(path => _os.WindowsFileSystemService.FileExists(path)) ?? throw new FileNotFoundException($"{Type} executable not found.");
    }

    public abstract BrowserPaths GetPaths();

    // Neue Helper-Methode für alle Kinder
    protected void AddPathFromUninstallKey(List<string> paths, string subKey, string exeName, bool isHklm)
    {
        var val = isHklm
            ? _os.WindowsRegistryService.GetLocalMachineValue(subKey, "InstallLocation")
            : _os.WindowsRegistryService.GetCurrentUserValue(subKey, "InstallLocation");

        if (val != null && !string.IsNullOrEmpty(val.ToString()))
        {
            // Robustheit: Pfad kombinieren
            var fullPath = _os.WindowsFileSystemService.CombinePaths(val.ToString()!, exeName);
            paths.Add(fullPath);
        }
    }

    //Wichtig damit bei der Firefox Version die (x64 de) entfernt wird.
    protected virtual string CleanVersionString(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return "Unknown";

        // Trim() entfernt Leerzeichen am Anfang und Ende, falls vorhanden.
        // Das macht den Regex noch robuster.
        var cleanRaw = raw.Trim();

        // Nimmt Zahlen und Punkte am Anfang. Stoppt beim ersten Leerzeichen/Buchstaben.
        var match = System.Text.RegularExpressions.Regex.Match(cleanRaw, @"^[\d\.]+");

        if (match.Success)
        {
            return match.Value;
        }

        return raw;
    }
}
