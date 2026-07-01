using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.Network;

public interface IHttpDownloadOutboundPort
{
    Task DownloadFileAsync(
        string url,
        string destinationPath,
        IProgress<DownloadProgressStatus> progress,
        CancellationToken cancel);
}
