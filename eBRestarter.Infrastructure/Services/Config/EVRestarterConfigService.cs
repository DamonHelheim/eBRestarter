using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eBRestarter.Infrastructure.Services.Config;

/// <summary>
/// Implementiert den Basis-Konfigurations-Service basierend auf JSON-Dateien im Dateisystem.
/// Vollständig entkoppelt von System.IO über den IWindowsFileSystemService (Repository-Pattern).
/// </summary>
public class EVisitorConfigService(
    IPathService pathService, 
    IWindowsFileSystemService fileSystem,
    ILogger<EVisitorConfigService> logger) : IEVisitorConfigService
{
    private readonly IPathService _pathService = pathService;
    private readonly IWindowsFileSystemService _fileSystem = fileSystem;
    private readonly ILogger<EVisitorConfigService> _logger = logger;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = AppConfigJsonContext.Default
    };

    /// <summary>
    /// Lädt die Roh-Konfiguration aus der JSON-Datei über das FileSystem-Repository.
    /// </summary>
    public AppConfig LoadConfig()
    {
        var filePath = _pathService.RetrieveConfigFilePath();

        if (!_fileSystem.FileExists(filePath))
        {
            var defaultConfig = new AppConfig();
            SaveConfig(defaultConfig);
            return defaultConfig;
        }

        try
        {
            var jsonString = _fileSystem.ReadAllText(filePath);
            return JsonSerializer.Deserialize<AppConfig>(jsonString, _jsonOptions) ?? new AppConfig();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Config-Datei ist kein gültiges JSON.");
            return new AppConfig();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kritischer Fehler beim Laden der Konfiguration.");
            return new AppConfig();
        }
    }

    /// <summary>
    /// Speichert die Konfiguration über das FileSystem-Repository.
    /// </summary>
    public void SaveConfig(AppConfig config)
    {
        try
        {
            var filePath = _pathService.RetrieveConfigFilePath();
            var directory = _fileSystem.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !_fileSystem.DirectoryExists(directory))
            {
                _fileSystem.CreateDirectory(directory);
            }

            var jsonString = JsonSerializer.Serialize(config, _jsonOptions);
            _fileSystem.WriteAllText(filePath, jsonString);

            _logger.LogInformation("Konfiguration gespeichert.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Speichern der Konfiguration.");
        }
    }

    /// <summary>
    /// Setzt die Konfiguration auf Standardwerte zurück.
    /// </summary>
    public void ResetConfig()
    {
        _logger.LogWarning("Konfiguration wird auf Standardwerte zurückgesetzt.");
        var defaultConfig = new AppConfig();
        SaveConfig(defaultConfig);
    }
}

// SOURCE GENERATOR KONTEXT (Für Release/Trim-Kompatibilität)
[JsonSerializable(typeof(AppConfig))]
internal partial class AppConfigJsonContext : JsonSerializerContext;
