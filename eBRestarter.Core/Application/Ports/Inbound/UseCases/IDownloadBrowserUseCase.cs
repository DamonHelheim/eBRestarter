using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.UseCases;

/// <summary>
/// Port: Use Case Interface for downloading and installing browser setup packages from the presentation layer.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT (Use Case Interface)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelBrowserItem"/> und ViewModels im Presentation Layer via MVVM).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt INNERHALB des Application Cores (<see cref="eBRestarter.Core.Application.UseCases.DownloadBrowserUseCase"/>).<br/>
/// - <strong>Begründung:</strong> Dient der Benutzeroberfläche als Eingangstür (Inbound Port) in den Anwendungskern, um den Download und die Installation von Webbrowsern inklusive Fortschrittsüberwachung auszuführen. Suffix <c>UseCase</c> ist vorbildlich.
/// </para>
/// </summary>
public interface IDownloadBrowserUseCase
{
    Task<string> DownloadInstallerAsync(
        string browserName,
        string downloadUrl,
        IProgress<DownloadProgressStatus> progress,
        CancellationToken cancellationToken);

    Task StartInstallerAsync(string installerPath);

    void CleanupPartialDownload(string browserName);
}
