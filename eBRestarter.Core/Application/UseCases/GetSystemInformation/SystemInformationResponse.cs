namespace eBRestarter.Core.Application.UseCases.GetSystemInformation;

public record SystemInformationResponse(
    string ProcessorName,
    string GraphicsCardName,
    string InstalledRam,
    string OsEdition,
    string OsDisplayVersion,
    string OsBuildVersion,
    string StandardBrowserName
);
