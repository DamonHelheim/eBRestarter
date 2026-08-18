using System;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Extensions.DependencyInjections;

/// <summary>
/// Composition root for application logging.
/// </summary>
/// <remarks>
/// Configures Serilog with a daily rolling file sink to provide persistent file logging for unpackaged WinUI 3 deployments.
/// Enriches log events with LogContext properties for structured diagnostic output.
/// </remarks>
public static class LoggingServiceExtensions
{
    private const string AppFolderName = "eBRestarter";
    private const int DefaultRetainedFileCount = 7;
    private const long DefaultFileSizeLimitBytes = 10L * 1024 * 1024;
    private const string LogFileNameTemplate = "ebrestarter-.log";
    private const string LogFolderName = "logs";

    /// <summary>Configuration key holding the minimum level, e.g. <c>"Logging:MinimumLevel"</c>.</summary>
    private const string MinimumLevelConfigurationKey = "Logging:MinimumLevel";

    private const string OutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Properties:j}{NewLine}{Exception}";

    private const string SkylarFolderName = "Skylar";

    /// <summary>
    /// Registers Serilog as the application-wide <c>ILoggerProvider</c> with a rolling file sink.
    /// </summary>
    /// <param name="services">The service collection to add logging to.</param>
    /// <param name="configuration">Application configuration; supplies the minimum log level.</param>
    /// <returns>The modified <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddApplicationLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var logDirectory = ResolveLogDirectory();
        Directory.CreateDirectory(logDirectory);

        var logFilePath = Path.Combine(logDirectory, LogFileNameTemplate);
        var minimumLevel = ResolveMinimumLevel(configuration);

        var serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: logFilePath,
                outputTemplate: OutputTemplate,
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: DefaultFileSizeLimitBytes,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: DefaultRetainedFileCount,
                shared: true)
            .CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddDebug();

            builder.AddSerilog(serilogLogger, dispose: true);
            builder.SetMinimumLevel(ToMicrosoftLevel(minimumLevel));
        });

        return services;
    }

    /// <summary>
    /// Returns the directory that log files are written to.
    /// </summary>
    /// <returns>The resolved absolute directory path for log files.</returns>
    /// <remarks>
    /// Uses the local application data path (<c>%LocalAppData%\Skylar\eBRestarter\logs</c>) to unify configuration and log outputs.
    /// </remarks>
    private static string ResolveLogDirectory()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(localAppData, SkylarFolderName, AppFolderName, LogFolderName);
    }

    /// <summary>
    /// Reads the configured minimum level, defaulting to <see cref="LogEventLevel.Information"/>.
    /// </summary>
    /// <param name="configuration">Application configuration source.</param>
    /// <returns>The parsed <see cref="LogEventLevel"/>.</returns>
    /// <remarks>
    /// Minimum log level can be controlled via application configuration (<c>appsettings.json</c>).
    /// Parsed explicitly to maintain compatibility with trimming.
    /// </remarks>
    private static LogEventLevel ResolveMinimumLevel(IConfiguration configuration)
    {
        var configuredValue = configuration[MinimumLevelConfigurationKey];

        return Enum.TryParse(configuredValue, ignoreCase: true, out LogEventLevel parsedLevel)
            ? parsedLevel
            : LogEventLevel.Information;
    }

    /// <summary>
    /// Maps a Serilog level onto the equivalent <see cref="LogLevel"/> so the
    /// <c>Microsoft.Extensions.Logging</c> pipeline filters at the same threshold.
    /// </summary>
    /// <param name="level">The Serilog log event level to map.</param>
    /// <returns>The equivalent Microsoft <see cref="LogLevel"/>.</returns>
    private static LogLevel ToMicrosoftLevel(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose => LogLevel.Trace,
        LogEventLevel.Debug => LogLevel.Debug,
        LogEventLevel.Information => LogLevel.Information,
        LogEventLevel.Warning => LogLevel.Warning,
        LogEventLevel.Error => LogLevel.Error,
        LogEventLevel.Fatal => LogLevel.Critical,
        _ => LogLevel.Information
    };
}
