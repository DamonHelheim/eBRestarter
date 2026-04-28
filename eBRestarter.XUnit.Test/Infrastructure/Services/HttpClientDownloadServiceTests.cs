using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Infrastructure.Services;
using Moq;
using Moq.Protected;
using Shouldly;
using System.Net;
using System.Text;
using Xunit;

namespace eBRestarter.Tests.Infrastructure.Services
{
    /// <summary>
    /// Testet den HttpClientDownloadService.
    /// Wir mocken den zugrundeliegenden HttpMessageHandler, um echte Netzwerkaufrufe zu vermeiden.
    /// </summary>
    public class HttpClientDownloadServiceTests : IDisposable
    {
        private readonly string _tempFilePath;
        private readonly Mock<HttpMessageHandler> _handlerMock;

        public HttpClientDownloadServiceTests()
        {
            _tempFilePath = Path.Combine(Path.GetTempPath(), $"download_test_{Guid.NewGuid()}.exe");
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        }

        public void Dispose()
        {
            if (File.Exists(_tempFilePath)) File.Delete(_tempFilePath);
        }

        // =========================================================
        // 1. DOWNLOAD LOGIK TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Prüft, ob der Service Daten vom Server liest und physisch korrekt auf die Festplatte schreibt.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren eine HTTP-Antwort mit 100 Bytes Testdaten.
        /// Der Test validiert, ob die Datei erstellt wurde und den exakten Inhalt hat.
        /// </summary>
        [Fact]
        public async Task DownloadFileAsync_ShouldWriteContentToFileSystem()
        {
            // ARRANGE
            var testData = "Dies sind simulierte Binärdaten für den Download-Test.";
            var contentBytes = Encoding.UTF8.GetBytes(testData);

            SetupMockResponse(HttpStatusCode.OK, contentBytes);

            // Um den Mock-Handler zu nutzen, müssen wir den Service mit dem Handler instanziieren.
            // Falls dein Service den HttpClient fix im CTOR erstellt, müsstest du ihn ggf.
            // für Dependency Injection anpassen (empfohlen).
            var sut = CreateServiceWithMockHandler();

            // ACT
            await sut.DownloadFileAsync(
                "https://example.com/setup.exe",
                _tempFilePath,
                null!,
                CancellationToken.None);

            // ASSERT
            File.Exists(_tempFilePath).ShouldBeTrue();
            var downloadedData = await File.ReadAllBytesAsync(_tempFilePath);
            downloadedData.ShouldBe(contentBytes);
        }

        // =========================================================
        // 2. PROGRESS REPORTING TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Die UI benötigt Fortschrittsmeldungen. Da du eine Drosselung (Throttling) eingebaut hast,
        /// müssen wir sicherstellen, dass trotz Drosselung am Ende 100% gemeldet werden.
        /// </summary>
        [Fact]
        public async Task DownloadFileAsync_ShouldReportProgress_AndReach100Percent()
        {
            // ARRANGE
            var contentSize = 10000; // Genug Daten, damit mehrere Chunks gelesen werden
            var contentBytes = new byte[contentSize];
            new Random().NextBytes(contentBytes);

            SetupMockResponse(HttpStatusCode.OK, contentBytes);

            var progressReports = new List<DownloadProgressStatus>();
            var progressMock = new Progress<DownloadProgressStatus>(p => progressReports.Add(p));

            var sut = CreateServiceWithMockHandler();

            // ACT
            await sut.DownloadFileAsync("https://test.com", _tempFilePath, progressMock, CancellationToken.None);

            // Wir müssen kurz warten, da IProgress asynchron meldet
            await Task.Delay(100);

            // ASSERT
            progressReports.ShouldNotBeEmpty();
            var lastReport = progressReports.Last();
            lastReport.Percentage.ShouldBe(100.0);
            lastReport.TotalBytes.ShouldBe(contentSize);
        }

        // =========================================================
        // 3. ERROR & CANCELLATION TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn der Server einen Fehler (z.B. 404) liefert, muss der Service eine Exception werfen,
        /// damit der User informiert werden kann.
        /// </summary>
        [Fact]
        public async Task DownloadFileAsync_ShouldThrowException_OnHttpError()
        {
            // ARRANGE
            SetupMockResponse(HttpStatusCode.NotFound, Array.Empty<byte>());
            var sut = CreateServiceWithMockHandler();

            // ACT & ASSERT
            await Should.ThrowAsync<HttpRequestException>(async () =>
                await sut.DownloadFileAsync("https://test.com/404", _tempFilePath, null!, CancellationToken.None));
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn der Benutzer den Download abbricht, muss der HttpClient-Request sofort gestoppt werden.
        /// </summary>
        [Fact]
        public async Task DownloadFileAsync_ShouldRespectCancellationToken()
        {
            // ARRANGE
            SetupMockResponse(HttpStatusCode.OK, new byte[1024]);
            var sut = CreateServiceWithMockHandler();
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Sofort abbrechen

            // ACT & ASSERT
            await Should.ThrowAsync<OperationCanceledException>(async () =>
                await sut.DownloadFileAsync("https://test.com", _tempFilePath, null!, cts.Token));
        }

        // =========================================================
        // HELPER METHODEN
        // =========================================================

        private void SetupMockResponse(HttpStatusCode statusCode, byte[] content)
        {
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new ByteArrayContent(content)
                });
        }

        /// <summary>
        /// Da dein Service den HttpClient intern per 'new' erstellt,
        /// ist ein kleiner Trick nötig: Wir nutzen Reflection, um das private Feld zu setzen,
        /// ODER (besser) du änderst deinen Service so, dass er HttpClient im CTOR akzeptiert.
        ///
        /// Hier die Variante, die davon ausgeht, dass wir das Feld setzen können:
        /// </summary>
        private HttpClientDownloadService CreateServiceWithMockHandler()
        {
            var service = new HttpClientDownloadService();
            var client = new HttpClient(_handlerMock.Object);

            // Per Reflection den gemockten Client einimpfen
            var field = typeof(HttpClientDownloadService).GetField("_httpClient",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            field?.SetValue(service, client);

            return service;
        }
    }
}