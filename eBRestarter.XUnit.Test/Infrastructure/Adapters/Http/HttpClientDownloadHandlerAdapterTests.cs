using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Handler.Http;
using eBRestarter.Tests.TestDoubles;

namespace eBRestarter.Tests.Infrastructure.Handlers
{
    /// <summary>
    /// Unit and integration tests for <see cref="AdapterHttpClientDownloadHandler"/> using a stub HTTP message handler to verify file downloads and progress reporting.
    /// </summary>
    /// <remarks>
    /// Uses a lightweight <see cref="StubHttpMessageHandler"/> to simulate HTTP status codes and binary payloads without external network calls.
    /// </remarks>
    public class HttpClientDownloadHandlerAdapterTests : IDisposable
    {
        /// <summary>Fixed seed to ensure repeatable test data generation across test runs.</summary>
        private const int RandomSeed = 20260811;

        private readonly string _tempFilePath;

        public HttpClientDownloadHandlerAdapterTests()
        {
            _tempFilePath = Path.Combine(Path.GetTempPath(), $"download_test_{Guid.NewGuid()}.exe");
        }

        /// <summary>Deletes the temporary file created for download test execution.</summary>
        public void Dispose()
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }

            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task DownloadFileAsync_ServerReturnsContent_WritesExactBytesToDisk()
        {
            // [R]IGHT: Successfully downloads file payload and writes exact bytes to disk
            // Arrange
            var contentBytes = Encoding.UTF8.GetBytes("Dies sind simulierte Binärdaten für den Download-Test.");
            var sut = CreateSut(HttpStatusCode.OK, contentBytes);

            // Act
            await sut.DownloadFileAsync("https://example.com/setup.exe", _tempFilePath, null!, TestContext.Current.CancellationToken);

            // Assert
            File.Exists(_tempFilePath).ShouldBeTrue();
            (await File.ReadAllBytesAsync(_tempFilePath, TestContext.Current.CancellationToken)).ShouldBe(contentBytes);
        }

        [Fact]
        public async Task DownloadFileAsync_ContentSpansMultipleChunks_ReportsProgressReaching100Percent()
        {
            // [R]IGHT: Multi-chunk download reports incremental progress culminating in 100 percent
            // Arrange
            const int contentSize = 10000;
            var contentBytes = new byte[contentSize];
            new Random(RandomSeed).NextBytes(contentBytes);

            var progressReports = new List<DownloadProgressStatus>();
            var progress = new SynchronousProgress<DownloadProgressStatus>(progressReports.Add);
            var sut = CreateSut(HttpStatusCode.OK, contentBytes);

            // Act
            await sut.DownloadFileAsync("https://test.com", _tempFilePath, progress, TestContext.Current.CancellationToken);

            // Assert
            progressReports.ShouldNotBeEmpty();
            progressReports.Last().Percentage.ShouldBe(100.0);
            progressReports.Last().TotalBytes.ShouldBe(contentSize);
        }

        [Fact]
        public async Task DownloadFileAsync_ServerReturnsNotFound_ThrowsHttpRequestException()
        {
            // [E]RROR: HTTP 404 response throws HttpRequestException
            // Arrange
            var sut = CreateSut(HttpStatusCode.NotFound, []);

            // Act
            var act = () => sut.DownloadFileAsync("https://test.com/404", _tempFilePath, null!, CancellationToken.None);

            // Assert
            await Should.ThrowAsync<HttpRequestException>(act);
        }

        [Fact]
        public async Task DownloadFileAsync_TokenAlreadyCancelled_ThrowsOperationCanceledException()
        {
            // [B]OUNDARY / [E]RROR: Pre-cancelled token throws OperationCanceledException
            // Arrange
            var sut = CreateSut(HttpStatusCode.OK, new byte[1024]);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var act = () => sut.DownloadFileAsync("https://test.com", _tempFilePath, null!, cts.Token);

            // Assert
            await Should.ThrowAsync<OperationCanceledException>(act);
        }

        /// <summary>
        /// Creates an <see cref="AdapterHttpClientDownloadHandler"/> instance configured with the specified HTTP status code and payload.
        /// </summary>
        /// <param name="statusCode">The HTTP status code for the stub handler.</param>
        /// <param name="content">The byte payload for the stub handler.</param>
        /// <returns>A configured <see cref="AdapterHttpClientDownloadHandler"/> instance.</returns>
        private static AdapterHttpClientDownloadHandler CreateSut(HttpStatusCode statusCode, byte[] content)
        {
            var handler = new StubHttpMessageHandler(statusCode, content);

            return new AdapterHttpClientDownloadHandler(new HttpClient(handler));
        }

        /// <summary>
        /// Stub HTTP message handler returning a predefined status code and byte payload.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to return in stub responses.</param>
        /// <param name="content">The byte payload to return in stub responses.</param>
        private sealed class StubHttpMessageHandler(HttpStatusCode statusCode, byte[] content) : HttpMessageHandler
        {
            /// <inheritdoc />
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new ByteArrayContent(content)
                });
            }
        }
    }
}
