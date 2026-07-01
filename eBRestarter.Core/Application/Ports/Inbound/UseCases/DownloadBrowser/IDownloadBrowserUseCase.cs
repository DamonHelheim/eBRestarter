using eBRestarter.Core.Application.Models.Records;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.DownloadBrowser;

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
