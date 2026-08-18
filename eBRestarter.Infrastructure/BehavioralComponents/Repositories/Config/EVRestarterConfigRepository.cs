using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.ValueObjects;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Infrastructure.BehavioralComponents.Repositories.Config;

/// <summary>
/// Implements the base configuration service based on JSON files within the file system.
/// Completely decoupled from System.IO via the IFileSystemPort (Repository Pattern).
/// </summary>
/// <param name="pathProvider">The inbound application path provider determining configuration file locations.</param>
/// <param name="fileSystem">The outbound file system port decoupling physical I/O operations.</param>
/// <param name="logger">The logger instance for telemetry and error tracking.</param>
public sealed class EVRestarterConfigRepository(
    IInboundPortOsAppPathProvider pathProvider,
    IOutboundPortFileSystem fileSystem,
    ILogger<EVRestarterConfigRepository> logger)
    : IOutboundPortEVisitorConfigRepository
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injected Dependencies (alphabetical) ──
    private readonly IOutboundPortFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    private readonly ILogger<EVRestarterConfigRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IInboundPortOsAppPathProvider _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));

    // ── Block 4: Complex Types & Collections (alphabetical) ──
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = AppConfigJsonContext.Default
    };

    // ⚡ Guide Section 14.1: LoadConfig is invoked frequently throughout the application.
    // The raw JSON string is cached instead of the AppConfig instance: AppConfig is a mutable record
    // that callers modify before saving. Returning a shared instance would leak uncommitted mutations
    // across independent callers. Caching raw JSON eliminates disk I/O while ensuring each caller
    // receives an isolated instance. Deterministic invalidation occurs in SaveConfig and ResetConfig.
    private readonly Lock _cacheLock = new();
    private string? _cachedConfigJson;


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <inheritdoc />
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
            var jsonString = ReadConfigJsonCached(filePath);
            return JsonSerializer.Deserialize<AppConfig>(jsonString, _jsonOptions) ?? new AppConfig();
        }
        catch (JsonException exception)
        {
            _logger.LogError(LogEventIds.Configuration.ConfigurationInvalidJson, exception, "Configuration file at {Path} is not valid JSON.", filePath);
            InvalidateCache();
            return new AppConfig();
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.Configuration.ConfigurationLoadFailed, exception, "Critical error occurred while loading configuration from {Path}.", filePath);
            InvalidateCache();
            return new AppConfig();
        }
    }

    /// <inheritdoc />
    public void ResetConfig()
    {
        _logger.LogWarning(LogEventIds.Configuration.ConfigurationReset, "Configuration is being reset to default values.");
        var defaultConfig = new AppConfig();
        SaveConfig(defaultConfig);
    }

    /// <inheritdoc />
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

            // The serialized payload represents the fresh cache content – no disk re-read required.
            lock (_cacheLock)
            {
                _cachedConfigJson = jsonString;
            }

            // 📝 Logging Guidelines Section 5 (Avoid Information Inflation): Logging on every configuration
            // write and restarter cycle generates excessive noise; kept at Debug level.
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(LogEventIds.Configuration.ConfigurationSaved, "Configuration saved successfully to {Path}.", filePath);
            }
        }
        catch (Exception exception)
        {
            // I/O write failure: invalidate cache so the next load reads actual state from disk.
            InvalidateCache();
            _logger.LogError(LogEventIds.Configuration.ConfigurationSaveFailed, exception, "Error while saving configuration to {Path}.", filePath);
        }
    }

    /// <summary>
    /// Clears the in-memory cached JSON configuration string.
    /// </summary>
    private void InvalidateCache()
    {
        lock (_cacheLock)
        {
            _cachedConfigJson = null;
        }
    }

    /// <summary>
    /// Returns the configuration file content, reading from disk only on a cache miss.
    /// </summary>
    /// <param name="filePath">The file path to the configuration JSON file.</param>
    /// <returns>The raw JSON string content.</returns>
    private string ReadConfigJsonCached(string filePath)
    {
        lock (_cacheLock)
        {
            _cachedConfigJson ??= _fileSystem.ReadAllText(filePath);
            return _cachedConfigJson;
        }
    }
}

/// <summary>
/// Source generator JSON serializer context for trim and AOT compatibility.
/// </summary>
[JsonSerializable(typeof(AppConfig))]
internal partial class AppConfigJsonContext : JsonSerializerContext;
