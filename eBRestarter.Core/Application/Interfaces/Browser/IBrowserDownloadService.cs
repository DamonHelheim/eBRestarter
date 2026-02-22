using eBRestarter.Core.Domain.Models.Records;

namespace eBRestarter.Core.Application.Interfaces.Browser;

public interface IBrowserDownloadService
{
    Task DownloadFileAsync(
        string url,
        string destinationPath,
        IProgress<DownloadProgressStatus> progress,
        CancellationToken cancellationToken);
}
