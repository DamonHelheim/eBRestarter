namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public record SystemInformationResponse(
    string ProcessorName,
    string GraphicsCardName,
    string InstalledRam,
    string OsEdition,
    string OsDisplayVersion,
    string OsBuildVersion,
    string StandardBrowserName
);


