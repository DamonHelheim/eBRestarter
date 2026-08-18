namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;

/// <summary>
/// Port: Provides OS-specific application directory and file paths to the application core and repositories.
/// <para>
/// <strong>Architectural Classification: OUTBOUND PORT (Driven Port)</strong><br/>
/// - <strong>Consumer:</strong> Located inside the Application Core (<see cref="Application.UseCases.DownloadBrowserUseCase"/>) and in the Infrastructure Layer.<br/>
/// - <strong>Implementer:</strong> Located in the Infrastructure Layer (<see cref="eBRestarter.Infrastructure.Providers.WindowsAppPathProvider"/> via OS API).<br/>
/// - <strong>Rationale:</strong> Accesses operating-system-specific directory locations and file system layouts.
/// </para>
/// </summary>
public interface IInboundPortOsAppPathProvider
{
    /// <summary>
    /// Retrieves the fully qualified path to the application data directory.
    /// </summary>
    /// <returns>The absolute path to the application data folder.</returns>
    string RetrieveAppDataPath();

    /// <summary>
    /// Retrieves the fully qualified path to the user's downloads folder.
    /// </summary>
    /// <returns>The absolute path to the downloads directory.</returns>
    string RetrieveDownloadsPath();

    /// <summary>
    /// Retrieves the fully qualified path to the application configuration file.
    /// </summary>
    /// <returns>The absolute path to the configuration JSON file.</returns>
    string RetrieveConfigFilePath();

    /// <summary>
    /// Retrieves the fully qualified path to the application log file.
    /// </summary>
    /// <returns>The absolute path to the log file.</returns>
    string RetrieveLogFilePath();
}
