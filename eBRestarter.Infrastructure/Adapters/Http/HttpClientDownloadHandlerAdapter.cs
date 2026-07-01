using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Models.Records;
using System.Diagnostics;

namespace eBRestarter.Infrastructure.Adapters.Http;

public sealed class HttpClientDownloadHandlerAdapter : IHttpDownloadOutboundPort
{
    private readonly HttpClient _httpClient;

    public HttpClientDownloadHandlerAdapter()
    {
        _httpClient = new HttpClient();

        // We spoof the User-Agent to impersonate a standard Chrome browser running on Windows 10.
        // Without this header, the Brave server rejects the request or yields an HTML landing page (80KB) instead of the actual EXE installer binary (1.2MB).
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36");
    }

    public async Task DownloadFileAsync(string url, string destinationPath, IProgress<DownloadProgressStatus> progress, CancellationToken cancel)
    {
        // 1. Establish connection (Read response headers only)
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancel);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var canReportProgress = totalBytes != -1;

        // 2. Open underlying I/O streams
        await using var contentStream = await response.Content.ReadAsStreamAsync(cancel);

        // IMPORTANT: Use FileOptions.Asynchronous to guarantee real async background I/O operations on the disk file system.
        await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true);

        var buffer = new byte[8192];
        long totalRead = 0;
        int bytesRead;
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // We restrict progress reporting frequency to match a fixed 100ms interval
        long lastUpdateTicks = 0;
        long updateIntervalTicks = Stopwatch.Frequency / 10; // 1/10 of a second (=100ms)

        while ((bytesRead = await contentStream.ReadAsync(buffer, cancel)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancel);

            totalRead += bytesRead;

            if (canReportProgress && progress is not null)
            {
                // Dispatch progress updates only when:
                // A) The defined elapsed tick time interval threshold has passed OR
                // B) The entire operation payload has fully downloaded (totalRead == totalBytes)
                long currentTicks = stopwatch.ElapsedTicks;

                if (currentTicks - lastUpdateTicks > updateIntervalTicks || totalRead == totalBytes)
                {
                    lastUpdateTicks = currentTicks;

                    var percentage = (double)totalRead / totalBytes * 100;

                    progress.Report(new DownloadProgressStatus(totalRead, totalBytes, percentage));
                }
            }
        }
    }
}
