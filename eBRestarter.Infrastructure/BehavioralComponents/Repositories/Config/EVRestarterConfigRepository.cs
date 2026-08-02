using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.ValueObjects;

namespace eBRestarter.Infrastructure.BehavioralComponents.Repositories.Config;

/// <summary>
/// Implements the base configuration service based on JSON files within the file system.
/// Completely decoupled from System.IO via the IFileSystemPort (Repository Pattern).
/// </summary>
public sealed class EVRestarterConfigRepository(
    IInboundPortOsAppPathProvider pathProvider,
    IOutboundPortFileSystem fileSystem,
    ILogger<EVRestarterConfigRepository> logger)
    : IOutboundPortEVisitorConfigRepository
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch) ──
    private readonly IOutboundPortFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    private readonly ILogger<EVRestarterConfigRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IInboundPortOsAppPathProvider _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));

    // ── Block 4: Komplexe Typen & Kollektionen (alphabetisch) ──
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = AppConfigJsonContext.Default
    };


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    public AppConfig LoadConfig()
    {
        var filePath = _pathProvider.RetrieveConfigFilePath();

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
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Configuration file at {Path} is not valid JSON.", filePath);
            return new AppConfig();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Critical error occurred while loading configuration from {Path}.", filePath);
            return new AppConfig();
        }
    }

    public void ResetConfig()
    {
        _logger.LogWarning("Configuration is being reset to default values.");
        var defaultConfig = new AppConfig();
        SaveConfig(defaultConfig);
    }

    public void SaveConfig(AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var filePath = _pathProvider.RetrieveConfigFilePath();

        try
        {
            var directory = _fileSystem.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !_fileSystem.DirectoryExists(directory))
            {
                _fileSystem.CreateDirectory(directory);
            }

            var jsonString = JsonSerializer.Serialize(config, _jsonOptions);

            _fileSystem.WriteAllText(filePath, jsonString);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Configuration saved successfully to {Path}.", filePath);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while saving configuration to {Path}.", filePath);
        }
    }


}

// SOURCE GENERATOR CONTEXT (For Release/Trim compatibility scenarios)
[JsonSerializable(typeof(AppConfig))]
internal partial class AppConfigJsonContext : JsonSerializerContext;
