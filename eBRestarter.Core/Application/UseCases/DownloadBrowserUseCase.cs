using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Network;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Core.Application.UseCases;

/// <summary>
/// Use case implementation for downloading browser installer executables, executing installations, and cleaning partial downloads.
/// </summary>
public sealed class DownloadBrowserUseCase(
    IOutboundPortHttpDownload downloadService,
    IOutboundPortFileSystem fileSystemPort,
    IInboundPortOsAppPathProvider pathProvider,
    IOutboundPortOsProcessControl processControlPort)
    : IUseCaseDownloadBrowser
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    private const string InstallerFileNameSuffix = "_Installer.exe";


    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch A–Z) ──
    private readonly IOutboundPortHttpDownload _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
    private readonly IOutboundPortFileSystem _fileSystemPort = fileSystemPort ?? throw new ArgumentNullException(nameof(fileSystemPort));
    private readonly IInboundPortOsAppPathProvider _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
    private readonly IOutboundPortOsProcessControl _processControlPort = processControlPort ?? throw new ArgumentNullException(nameof(processControlPort));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Deletes any existing partial download file for the specified browser.
    /// </summary>
    /// <param name="browserName">The display name of the browser whose installer file should be cleaned.</param>
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
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ObjectDisposedException)
        {
            Debug.WriteLine(exception);
        }
    }

    /// <summary>
    /// Downloads the browser installer file asynchronously to the local downloads folder.
    /// </summary>
    /// <param name="browserName">The display name of the target browser.</param>
    /// <param name="downloadUrl">The remote URL of the installer binary.</param>
    /// <param name="progress">Progress reporter for tracking download percentage.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The local file path of the downloaded installer executable.</returns>
    public Task<string> DownloadInstallerAsync(
        string browserName,
        string downloadUrl,
        IProgress<DownloadProgressStatus> progress,
        CancellationToken cancellationToken)
    {
        // ⚡ Immediate Guard-Clause Exception Timing (Guide Abs. 9.1)
        ArgumentException.ThrowIfNullOrWhiteSpace(browserName);
        ArgumentException.ThrowIfNullOrWhiteSpace(downloadUrl);
        ArgumentNullException.ThrowIfNull(progress);

        return DownloadInstallerCoreAsync(browserName, downloadUrl, progress, cancellationToken);
    }

    /// <summary>
    /// Launches the browser installer executable asynchronously.
    /// </summary>
    /// <param name="installerPath">The absolute path to the local installer file.</param>
    public Task StartInstallerAsync(string installerPath)
    {
        // ⚡ Immediate Guard-Clause Exception Timing (Guide Abs. 9.1)
        ArgumentException.ThrowIfNullOrWhiteSpace(installerPath);

        return StartInstallerCoreAsync(installerPath);
    }

    private async Task<string> DownloadInstallerCoreAsync(
        string browserName,
        string downloadUrl,
        IProgress<DownloadProgressStatus> progress,
        CancellationToken cancellationToken)
    {
        var downloadPath = ResolveDownloadPath(browserName);

        await _downloadService.DownloadFileAsync(
            downloadUrl,
            downloadPath,
            progress,
            cancellationToken).ConfigureAwait(false);

        return downloadPath;
    }

    private string ResolveDownloadPath(string browserName)
    {
        var downloadsFolder = _pathProvider.RetrieveDownloadsPath();
        var fileName = $"{browserName}{InstallerFileNameSuffix}";

        return _fileSystemPort.CombinePaths(downloadsFolder, fileName);
    }

    private async Task StartInstallerCoreAsync(string installerPath)
    {
        await _processControlPort.StartExecutableAsync(installerPath).ConfigureAwait(false);
    }
}
