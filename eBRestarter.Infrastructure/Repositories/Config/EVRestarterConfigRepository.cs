using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eBRestarter.Infrastructure.Repositories.Config;

/// <summary>
/// Implements the base configuration service based on JSON files within the file system.
/// Completely decoupled from System.IO via the IFileSystemPort (Repository Pattern).
/// </summary>
public sealed class EVRestarterConfigRepository(
    IInboundPortOsAppPathProvider pathProvider,
    IFileSystemOutboundPort fileSystem,
    ILogger<EVRestarterConfigRepository> logger) : IEVisitorConfigRepositoryOutboundPort
{
    private readonly IInboundPortOsAppPathProvider _pathProvider = pathProvider;
    private readonly IFileSystemOutboundPort _fileSystem = fileSystem;
    private readonly ILogger<EVRestarterConfigRepository> _logger = logger;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = AppConfigJsonContext.Default
    };

    /// <summary>
    /// Loads the raw configuration from the JSON file via the FileSystem repository adapter.
    /// </summary>
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
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Configuration file is not valid JSON.");
            return new AppConfig();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error occurred while loading the configuration.");
            return new AppConfig();
        }
    }

    /// <summary>
    /// Saves the configuration using the FileSystem repository adapter.
    /// </summary>
    public void SaveConfig(AppConfig config)
    {
        try
        {
            var filePath = _pathProvider.RetrieveConfigFilePath();

            var directory = _fileSystem.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !_fileSystem.DirectoryExists(directory))
            {
                _fileSystem.CreateDirectory(directory);
            }

            var jsonString = JsonSerializer.Serialize(config, _jsonOptions);

            _fileSystem.WriteAllText(filePath, jsonString);

            _logger.LogInformation("Configuration saved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while saving the configuration.");
        }
    }

    /// <summary>
    /// Resets the configuration back to its default values.
    /// </summary>
    public void ResetConfig()
    {
        _logger.LogWarning("Configuration is being reset to default values.");
        var defaultConfig = new AppConfig();
        SaveConfig(defaultConfig);
    }
}

// SOURCE GENERATOR CONTEXT (For Release/Trim compatibility scenarios)
[JsonSerializable(typeof(AppConfig))]
internal partial class AppConfigJsonContext : JsonSerializerContext;



