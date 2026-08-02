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
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core durch Kapselung von <see cref="HttpClient"/> und Netzwerk-Streams für Datei-Downloads.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortHttpDownload"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um Netzwerk-I/O auszuführen.
/// </para>
/// </summary>
public sealed class AdapterHttpClientDownloadHandler : IOutboundPortHttpDownload
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    private const int BufferSizeBytes = 8192;
    private const string DefaultUserAgentHeaderValue = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36";
    private const double PercentageMultiplier = 100.0;
    private const int UpdateIntervalFrequencyDivider = 10; // 100ms
    private const long UnknownContentLength = -1L;
    private const string UserAgentHeaderName = "User-Agent";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (Dependencies) ──
    private readonly HttpClient _httpClient;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    public AdapterHttpClientDownloadHandler(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();

        if (!_httpClient.DefaultRequestHeaders.Contains(UserAgentHeaderName))
        {
            _httpClient.DefaultRequestHeaders.Add(UserAgentHeaderName, DefaultUserAgentHeaderValue);
        }
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    public Task DownloadFileAsync(
        string url,
        string destinationPath,
        IProgress<DownloadProgressStatus> progress,
        CancellationToken cancel)
    {
        // ⚡ Immediate Guard-Clause Exception Timing (Guide Abs. 9.1)
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(destinationPath);

        return DownloadFileCoreAsync(url, destinationPath, progress, cancel);
    }

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
                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancel).ConfigureAwait(false);

                        totalRead += bytesRead;

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
