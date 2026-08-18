using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Network;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Handler.Http;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) for executing HTTP file downloads and network I/O.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Encapsulates <see cref="HttpClient"/> and network streams for file downloads in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortHttpDownload"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterHttpClientDownloadHandler : IOutboundPortHttpDownload
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
    private const int BufferSizeBytes = 8192;
    private const string DefaultUserAgentHeaderValue = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36";
    private const double PercentageMultiplier = 100.0;
    private const int UpdateIntervalFrequencyDivider = 10; // 100ms
    private const long UnknownContentLength = -1L;
    private const string UserAgentHeaderName = "User-Agent";

    // 🔒 Security: HTTPS scheme verification and resource exhaustion protection.
    private const string InsecureSchemeExceptionMessage = "Only HTTPS downloads are permitted. Rejected URL scheme: ";

    /// <summary>
    /// Upper bound for a single download (512 MB maximum size to prevent resource exhaustion).
    /// </summary>
    private const long MaximumDownloadSizeBytes = 512L * 1024 * 1024;

    private const string SizeLimitExceededExceptionMessage = "The download exceeds the permitted maximum size of 512 MB and was aborted.";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies ──
    private readonly HttpClient _httpClient;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes a new instance of <see cref="AdapterHttpClientDownloadHandler"/>.
    /// </summary>
    /// <param name="httpClient">Optional HTTP client instance for dependency injection.</param>
    public AdapterHttpClientDownloadHandler(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();

        if (!_httpClient.DefaultRequestHeaders.Contains(UserAgentHeaderName))
        {
            _httpClient.DefaultRequestHeaders.Add(UserAgentHeaderName, DefaultUserAgentHeaderValue);
        }
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Asynchronously downloads a file from the specified URL to a local destination path with progress reporting.
    /// </summary>
    /// <param name="url">The HTTPS URL of the file to download.</param>
    /// <param name="destinationPath">The local file path where the download will be saved.</param>
    /// <param name="progress">Progress reporter for tracking download completion.</param>
    /// <param name="cancel">Cancellation token for aborting the operation.</param>
    public Task DownloadFileAsync(
        string url,
        string destinationPath,
        IProgress<DownloadProgressStatus> progress,
        CancellationToken cancel)
    {
        // ⚡ Guard clauses for parameter validation
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(destinationPath);

        // 🔒 Security: Require HTTPS scheme prior to executing request to prevent HTTP downgrade attacks.
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? requestUri)
            || !string.Equals(requestUri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal))
        {
            throw new ArgumentException($"{InsecureSchemeExceptionMessage}{url}", nameof(url));
        }

        return DownloadFileCoreAsync(url, destinationPath, progress, cancel);
    }

    /// <summary>
    /// Executes the core streaming download loop, enforcing size limits and updating progress.
    /// </summary>
    /// <param name="url">The HTTPS URL to download.</param>
    /// <param name="destinationPath">Target local file path.</param>
    /// <param name="progress">Progress reporter.</param>
    /// <param name="cancel">Cancellation token.</param>
    private async Task DownloadFileCoreAsync(
        string url,
        string destinationPath,
        IProgress<DownloadProgressStatus> progress,
        CancellationToken cancel)
    {
        // 1. Establish connection (Read response headers only)
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancel).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? UnknownContentLength;
        var canReportProgress = totalBytes != UnknownContentLength;

        // 🔒 Security: Validate Content-Length header against maximum download size limit.
        if (totalBytes > MaximumDownloadSizeBytes)
        {
            throw new InvalidOperationException(SizeLimitExceededExceptionMessage);
        }

        // 2. Open underlying I/O streams
        var contentStream = await response.Content.ReadAsStreamAsync(cancel).ConfigureAwait(false);
        await using (contentStream.ConfigureAwait(false))
        {
            // IMPORTANT: Use FileOptions.Asynchronous to guarantee real async background I/O operations on the disk file system.
            var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSizeBytes, useAsync: true);
            await using (fileStream.ConfigureAwait(false))
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSizeBytes);
                try
                {
                    long totalRead = 0;
                    int bytesRead;

                    long startTimestamp = Stopwatch.GetTimestamp();
                    long lastUpdateTicks = 0;
                    long updateIntervalTicks = Stopwatch.Frequency / UpdateIntervalFrequencyDivider;

                    while ((bytesRead = await contentStream.ReadAsync(buffer.AsMemory(0, BufferSizeBytes), cancel).ConfigureAwait(false)) > 0)
                    {
                        totalRead += bytesRead;

                        // 🔒 Security: Enforce size limit during streaming read.
                        if (totalRead > MaximumDownloadSizeBytes)
                        {
                            throw new InvalidOperationException(SizeLimitExceededExceptionMessage);
                        }

                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancel).ConfigureAwait(false);

                        if (canReportProgress && progress is not null)
                        {
                            // Dispatch progress updates only when:
                            // A) The defined elapsed tick time interval threshold has passed OR
                            // B) The entire operation payload has fully downloaded (totalRead == totalBytes)
                            long currentTicks = Stopwatch.GetElapsedTime(startTimestamp).Ticks;

                            if (currentTicks - lastUpdateTicks > updateIntervalTicks || totalRead == totalBytes)
                            {
                                lastUpdateTicks = currentTicks;

                                var percentage = (double)totalRead / totalBytes * PercentageMultiplier;

                                progress.Report(new DownloadProgressStatus(totalRead, totalBytes, percentage));
                            }
                        }
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }
    }
}
