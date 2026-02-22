using eBRestarter.Core.Domain.Models.Records;

namespace eBRestarter.Core.Application.Interfaces.Update;

public interface IUpdateService
{
    /// <summary>
    /// Prüft gegen die Quelle (z.B. GitHub), ob eine neue Version vorliegt.
    /// </summary>
    Task<UpdateInfo> CheckForUpdateAsync();

    /// <summary>
    /// Lädt das Update herunter (in Temp) und startet den Installer.
    /// </summary>
    Task DownloadAndInstallAsync(UpdateInfo updateInfo);
}
