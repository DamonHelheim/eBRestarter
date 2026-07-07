using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Network;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using System.Diagnostics;

namespace eBRestarter.Core.Application.UseCases;

public sealed class DownloadBrowserUseCase(
    IOutboundPortHttpDownload downloadService,
    IInboundPortOsAppPathProvider pathProvider,
    IOutboundPortFileSystem fileSystemPort,
    IOutboundPortOsProcessControl processControlPort) : IUseCaseDownloadBrowser
{
    private const string InstallerFileNameSuffix = "_Installer.exe";

    private readonly IOutboundPortHttpDownload _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
    private readonly IInboundPortOsAppPathProvider _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
    private readonly IOutboundPortFileSystem _fileSystemPort = fileSystemPort ?? throw new ArgumentNullException(nameof(fileSystemPort));
    private readonly IOutboundPortOsProcessControl _processControlPort = processControlPort ?? throw new ArgumentNullException(nameof(processControlPort));

    public async Task<string> DownloadInstallerAsync(
        string browserName,
        string downloadUrl,
        IProgress<DownloadProgressStatus> progress,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(browserName);
        ArgumentException.ThrowIfNullOrWhiteSpace(downloadUrl);
        ArgumentNullException.ThrowIfNull(progress);

        var downloadPath = ResolveDownloadPath(browserName);

        await _downloadService.DownloadFileAsync(
            downloadUrl,
            downloadPath,
            progress,
            cancellationToken);

        return downloadPath;
    }

    public async Task StartInstallerAsync(string installerPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(installerPath);

        await _processControlPort.StartExecutableAsync(installerPath);
    }

    public void CleanupPartialDownload(string browserName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(browserName);

        try
        {
            var downloadPath = ResolveDownloadPath(browserName);

            if (_fileSystemPort.FileExists(downloadPath))
            {
                _fileSystemPort.DeleteFile(downloadPath);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ObjectDisposedException)
        {
            Debug.WriteLine(ex);
        }
    }

    private string ResolveDownloadPath(string browserName)
    {
        var downloadsFolder = _pathProvider.RetrieveDownloadsPath();
        var fileName = $"{browserName}{InstallerFileNameSuffix}";
        return _fileSystemPort.CombinePaths(downloadsFolder, fileName);
    }
}
