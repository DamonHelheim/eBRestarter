using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Interfaces.Browser;

public interface IBrowserDownloadUseCase
{
    Task DownloadFileAsync(
        string url,
        string destinationPath,
        IProgress<DownloadProgressStatus> progress,
        CancellationToken cancel);
}
