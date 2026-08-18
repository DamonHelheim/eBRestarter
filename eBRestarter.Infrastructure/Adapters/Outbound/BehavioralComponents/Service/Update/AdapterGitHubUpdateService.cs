using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Network;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Security;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Update;
using eBRestarter.Infrastructure.Api.Interfaces;
using eBRestarter.Infrastructure.ObjectArchetypes.Model;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Service.Update;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) for checking and processing application updates via GitHub releases.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Checks for updates via GitHub API, downloads assets, and initiates installation in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortUpdate"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterGitHubUpdateService : IOutboundPortUpdate
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
    private const int DefaultVersionBuild = 0;
    private const int DefaultVersionMajor = 1;
    private const int DefaultVersionMinor = 0;
    private const string DefaultZeroVersionString = "0.0.0";
    private const string DownloadAndInstallFailedExceptionMessage = "Failed to download or install the application update.";
    private const string ErrorParsingGitHubResponseLogMessage = "Error parsing the GitHub response.";
    private const string ExecutableFileExtension = ".exe";
    private const string GitHubApiUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
    private const string JsonPropertyAssets = "assets";
    private const string JsonPropertyBody = "body";
    private const string JsonPropertyBrowserDownloadUrl = "browser_download_url";
    private const string JsonPropertyName = "name";
    private const string JsonPropertyPublishedAt = "published_at";
    private const string JsonPropertyTagName = "tag_name";
    private const string MsiInstallerFileExtension = ".msi";
    private const string NoDownloadUrlFoundLogMessage = "No download URL found in update information.";
    private const string RepoName = "eBRestarter";
    private const string RepoOwner = "NeoVelora";
    private const string TempEnvironmentVariable = "TEMP";
    private const string UpdateCheckFailedLogMessage = "Update check failed: {Status}";
    private const string UpdateDownloadedLogMessage = "Update downloaded to: {Path}";
    private const string UpdateFileNameFormat = "eBRestarter_Update_{0}.exe";
    private const char VersionTagPrefix = 'v';

    // Security: Supply chain and download target host validation.
    private const string RejectedDownloadHostLogMessage = "Refusing update download: {Url} does not point to an approved GitHub release host.";
    private const string RejectedDownloadSchemeLogMessage = "Refusing update download: {Url} does not use HTTPS.";
    private const string SignatureRejectedAbortLogMessage = "Update aborted: the downloaded package {Path} does not carry a valid, trusted code signature. The file was deleted and NOT executed.";
    private const string UpdateStagingFolderFormat = "eBRestarter_Update_{0}";

    /// <summary>
    /// Hosts from which a release asset may legitimately be served. GitHub currently redirects
    /// <c>browser_download_url</c> to <c>objects.githubusercontent.com</c> or
    /// <c>release-assets.githubusercontent.com</c>; both are listed so a redirect target stays valid.
    /// </summary>
    private static readonly string[] ApprovedDownloadHosts =
    [
        "github.com",
        "www.github.com",
        "objects.githubusercontent.com",
        "release-assets.githubusercontent.com"
    ];

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies ──
    private readonly IOutboundPortApplicationLifetime _applicationLifetimeUseCase;
    private readonly IOutboundPortFileSystem _fileSystemPort;
    private readonly IOutboundPortHttpDownload _httpDownloadPort;
    private readonly ILogger<AdapterGitHubUpdateService> _logger;
    private readonly IOutboundPortOsProcessControl _processControlPort;
    private readonly IRestClient _restClientUseCase;
    private readonly IOutboundPortExecutableSignatureVerifier _signatureVerifier;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes a new instance of <see cref="AdapterGitHubUpdateService"/>.
    /// </summary>
    /// <param name="applicationLifetimeUseCase">Application lifetime management port.</param>
    /// <param name="fileSystemPort">File system operations port.</param>
    /// <param name="httpDownloadPort">HTTP file download port.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="processControlPort">OS process control port.</param>
    /// <param name="restClientUseCase">REST API client port.</param>
    /// <param name="signatureVerifier">Executable code signature verifier port.</param>
    public AdapterGitHubUpdateService(
        IOutboundPortApplicationLifetime applicationLifetimeUseCase,
        IOutboundPortFileSystem fileSystemPort,
        IOutboundPortHttpDownload httpDownloadPort,
        ILogger<AdapterGitHubUpdateService> logger,
        IOutboundPortOsProcessControl processControlPort,
        IRestClient restClientUseCase,
        IOutboundPortExecutableSignatureVerifier signatureVerifier)
    {
        ArgumentNullException.ThrowIfNull(applicationLifetimeUseCase);
        ArgumentNullException.ThrowIfNull(fileSystemPort);
        ArgumentNullException.ThrowIfNull(httpDownloadPort);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(processControlPort);
        ArgumentNullException.ThrowIfNull(restClientUseCase);
        ArgumentNullException.ThrowIfNull(signatureVerifier);

        _applicationLifetimeUseCase = applicationLifetimeUseCase;
        _fileSystemPort = fileSystemPort;
        _httpDownloadPort = httpDownloadPort;
        _logger = logger;
        _processControlPort = processControlPort;
        _restClientUseCase = restClientUseCase;
        _signatureVerifier = signatureVerifier;
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Asynchronously checks GitHub releases API for application updates.
    /// </summary>
    public async Task<UpdateInfo> CheckForUpdateAsync()
    {
        var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(DefaultVersionMajor, DefaultVersionMinor, DefaultVersionBuild);

        var request = new ApiRequest
        {
            Url = GitHubApiUrl,
            Method = ObjectArchetypes.Enums.HttpMethod.GET
        };

        var response = await _restClientUseCase.ExecuteGetAsync(request).ConfigureAwait(false);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
        {
            _logger.LogWarning(LogEventIds.Update.UpdateCheckFailed, UpdateCheckFailedLogMessage, response.StatusCode);

            return new UpdateInfo { IsUpdateAvailable = false, CurrentVersion = currentVersion.ToString() };
        }

        try
        {
            using var doc = JsonDocument.Parse(response.Content);
            var root = doc.RootElement;

            // 1. Extract and sanitize the version string from the tag (e.g., "v1.2.0" -> "1.2.0")
            string tagName = root.GetProperty(JsonPropertyTagName).GetString() ?? DefaultZeroVersionString;
            var cleanVersion = tagName.TrimStart(VersionTagPrefix); // Strip the leading "v" prefix if present

            if (!Version.TryParse(cleanVersion, out Version? latestVersion))
            {
                return new UpdateInfo { IsUpdateAvailable = false, CurrentVersion = currentVersion.ToString() };
            }

            // 2. Discover the specific raw binary distribution asset download target URL (e.g., Installer.msi or Setup.exe)
            var downloadUrl = string.Empty;

            if (root.TryGetProperty(JsonPropertyAssets, out JsonElement assets) && assets.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assets.EnumerateArray())
                {
                    string name = asset.GetProperty(JsonPropertyName).GetString() ?? string.Empty;

                    // Filter and check against standard executable and installation extensions
                    if (name.EndsWith(ExecutableFileExtension, StringComparison.OrdinalIgnoreCase) ||
                        name.EndsWith(MsiInstallerFileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset.GetProperty(JsonPropertyBrowserDownloadUrl).GetString() ?? string.Empty;
                        break;
                    }
                }
            }

            bool updateAvailable = latestVersion > currentVersion;

            return new UpdateInfo
            {
                IsUpdateAvailable = updateAvailable,
                CurrentVersion = currentVersion.ToString(),
                LatestVersion = cleanVersion,
                DownloadUrl = downloadUrl,
                Changelog = root.GetProperty(JsonPropertyBody).GetString() ?? string.Empty,
                PublishedAt = root.GetProperty(JsonPropertyPublishedAt).GetDateTime()
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.Update.UpdateResponseParsingFailed, exception, ErrorParsingGitHubResponseLogMessage);

            return new UpdateInfo { IsUpdateAvailable = false };
        }
    }

    /// <summary>
    /// Downloads and installs the specified application update.
    /// </summary>
    /// <param name="updateInfo">Update information containing download URL and target version.</param>
    public Task DownloadAndInstallAsync(UpdateInfo updateInfo)
    {
        // ⚡ Immediate Guard-Clause Exception Timing (Guide Abs. 9.1)
        ArgumentNullException.ThrowIfNull(updateInfo);

        return DownloadAndInstallCoreAsync(updateInfo);
    }

    /// <summary>
    /// Downloads the release asset into an unpredictable staging directory, verifies its code
    /// signature and only then launches it.
    /// </summary>
    /// <param name="updateInfo">Update information containing download URL and target version.</param>
    /// <remarks>
    /// Security: Downloads release assets into an unpredictable staging directory to prevent file planting / TOCTOU attacks.
    /// Validates HTTPS scheme, approved download hosts, and Authenticode code signatures before execution.
    /// Unsigned files are deleted without execution (fail-secure response).
    /// </remarks>
    private async Task DownloadAndInstallCoreAsync(UpdateInfo updateInfo)
    {
        if (string.IsNullOrWhiteSpace(updateInfo.DownloadUrl))
        {
            _logger.LogError(LogEventIds.Update.UpdateDownloadUrlMissing, NoDownloadUrlFoundLogMessage);
            return;
        }

        if (!IsApprovedDownloadUrl(updateInfo.DownloadUrl))
        {
            return;
        }

        try
        {
            // Staging directory with an unpredictable name: nothing can pre-create or swap a path
            // an attacker cannot guess, which removes the file-planting window on the fixed name.
            var tempDir = _fileSystemPort.ResolveEnvironmentPath(TempEnvironmentVariable);
            var stagingFolderName = string.Format(UpdateStagingFolderFormat, Guid.NewGuid().ToString("N"));
            var stagingDirectory = _fileSystemPort.CombinePaths(tempDir, stagingFolderName);

            _fileSystemPort.CreateDirectory(stagingDirectory);

            var updateFileName = string.Format(UpdateFileNameFormat, updateInfo.LatestVersion);
            var tempFile = _fileSystemPort.CombinePaths(stagingDirectory, updateFileName);

            await _httpDownloadPort.DownloadFileAsync(
                updateInfo.DownloadUrl,
                tempFile,
                new Progress<DownloadProgressStatus>(),
                CancellationToken.None).ConfigureAwait(false);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(LogEventIds.Update.UpdateDownloaded, UpdateDownloadedLogMessage, tempFile);
            }

            // Trust verification immediately before execution to minimize TOCTOU window.
            if (!_signatureVerifier.IsTrustedPublisher(tempFile))
            {
                _logger.LogError(LogEventIds.Security.SignatureVerificationRejected, SignatureRejectedAbortLogMessage, tempFile);
                _fileSystemPort.DeleteFile(tempFile);

                return;
            }

            // Fire and launch the fetched discrete file payload installer process directly
            await _processControlPort.StartExecutableAsync(tempFile).ConfigureAwait(false);

            // Terminate the active application process immediately so the installer can overwrite locked runtime assemblies safely
            _applicationLifetimeUseCase.ExitApplication(0);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(DownloadAndInstallFailedExceptionMessage, exception);
        }
    }

    /// <summary>
    /// Validates that the release asset URL is absolute, uses HTTPS, and points to an approved GitHub host.
    /// </summary>
    /// <param name="downloadUrl">Target download URL string.</param>
    private bool IsApprovedDownloadUrl(string downloadUrl)
    {
        if (!Uri.TryCreate(downloadUrl, UriKind.Absolute, out Uri? uri)
            || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal))
        {
            _logger.LogError(LogEventIds.Security.DownloadSchemeRejected, RejectedDownloadSchemeLogMessage, downloadUrl);
            return false;
        }

        if (Array.Exists(ApprovedDownloadHosts, approvedHost => string.Equals(uri.Host, approvedHost, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        _logger.LogError(LogEventIds.Security.DownloadHostRejected, RejectedDownloadHostLogMessage, downloadUrl);

        return false;
    }
}
