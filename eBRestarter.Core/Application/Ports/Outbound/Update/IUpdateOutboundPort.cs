using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.Update;

public interface IUpdateOutboundPort
{
    /// <summary>
    /// Validates against the source distribution repository (e.g., GitHub) to see if a newer version is available.
    /// </summary>
    Task<UpdateInfo> CheckForUpdateAsync();

    /// <summary>
    /// Downloads the update package payload (into the system temporary folder location) and launches the installer process.
    /// </summary>
    Task DownloadAndInstallAsync(UpdateInfo updateInfo);
}