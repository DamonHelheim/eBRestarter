using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Logging;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Network;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Security;

namespace eBRestarter.Core.Application.UseCases;

/// <summary>
/// Use case implementation for downloading browser installer executables, executing installations, and cleaning partial downloads.
/// </summary>
/// <param name="downloadService">Outbound service for HTTP file downloads.</param>
/// <param name="fileSystemPort">Outbound port for file system operations.</param>
/// <param name="logger">Application logger instance.</param>
/// <param name="pathProvider">Inbound provider for operating system application paths.</param>
/// <param name="processControlPort">Outbound port for launching OS processes.</param>
/// <param name="signatureVerifier">Outbound verifier for executable Authenticode signatures.</param>
public sealed class DownloadBrowserUseCase(
    IOutboundPortHttpDownload downloadService,
    IOutboundPortFileSystem fileSystemPort,
    IOutboundPortApplicationLogger<DownloadBrowserUseCase> logger,
    IInboundPortOsAppPathProvider pathProvider,
    IOutboundPortOsProcessControl processControlPort,
    IOutboundPortExecutableSignatureVerifier signatureVerifier)
    : IUseCaseDownloadBrowser
{
    private const string InstallerFileNameSuffix = "_Installer.exe";
    private const string UntrustedInstallerExceptionMessage = "The downloaded browser installer does not carry a valid, trusted code signature and was not executed.";

    private readonly IOutboundPortHttpDownload _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
    private readonly IOutboundPortFileSystem _fileSystemPort = fileSystemPort ?? throw new ArgumentNullException(nameof(fileSystemPort));
    private readonly IOutboundPortApplicationLogger<DownloadBrowserUseCase> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IInboundPortOsAppPathProvider _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
    private readonly IOutboundPortOsProcessControl _processControlPort = processControlPort ?? throw new ArgumentNullException(nameof(processControlPort));
    private readonly IOutboundPortExecutableSignatureVerifier _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));

    /// <inheritdoc />
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
            _logger.LogWarning(
                LogEventIds.Update.BrowserInstallerCleanupFailed,
                exception,
                "Partial installer download for {BrowserName} could not be removed.",
                browserName);
        }
    }

    /// <inheritdoc />
    public Task<string> DownloadInstallerAsync(
        string browserName,
        string downloadUrl,
        IProgress<DownloadProgressStatus> progress,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(browserName);
        ArgumentException.ThrowIfNullOrWhiteSpace(downloadUrl);
        ArgumentNullException.ThrowIfNull(progress);

        return DownloadInstallerCoreAsync(browserName, downloadUrl, progress, cancellationToken);
    }

    /// <inheritdoc />
    public Task StartInstallerAsync(string installerPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(installerPath);

        return StartInstallerCoreAsync(installerPath);
    }

    /// <summary>
    /// Executes the underlying HTTP download operation for the browser installer binary.
    /// </summary>
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

    /// <summary>
    /// Resolves the absolute local destination file path for a browser installer.
    /// </summary>
    private string ResolveDownloadPath(string browserName)
    {
        var downloadsFolder = _pathProvider.RetrieveDownloadsPath();
        var fileName = $"{browserName}{InstallerFileNameSuffix}";

        return _fileSystemPort.CombinePaths(downloadsFolder, fileName);
    }

    /// <summary>
    /// Verifies the Authenticode signature of the installer binary before initiating execution.
    /// </summary>
    /// <remarks>
    /// Verifies the Authenticode signature of downloaded browser installers to ensure authenticity before execution.
    /// Prevents execution of unverified binaries from user-writable directories.
    /// </remarks>
    private async Task StartInstallerCoreAsync(string installerPath)
    {
        if (!_signatureVerifier.IsTrustedPublisher(installerPath))
        {
            _fileSystemPort.DeleteFile(installerPath);

            throw new InvalidOperationException(UntrustedInstallerExceptionMessage);
        }

        await _processControlPort.StartExecutableAsync(installerPath).ConfigureAwait(false);
    }
}
