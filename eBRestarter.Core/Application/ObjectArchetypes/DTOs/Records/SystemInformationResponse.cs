namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Aggregates system hardware, operating system, and default browser information for presentation.
/// </summary>
/// <param name="ProcessorName">The processor model name.</param>
/// <param name="GraphicsCardName">The graphics adapter name.</param>
/// <param name="InstalledRam">The formatted installed memory capacity.</param>
/// <param name="OsEdition">The Windows operating system edition.</param>
/// <param name="OsDisplayVersion">The Windows operating system display version.</param>
/// <param name="OsBuildVersion">The Windows operating system build number.</param>
/// <param name="StandardBrowserName">The default system web browser name.</param>
public record SystemInformationResponse(
    string ProcessorName,
    string GraphicsCardName,
    string InstalledRam,
    string OsEdition,
    string OsDisplayVersion,
    string OsBuildVersion,
    string StandardBrowserName
);


