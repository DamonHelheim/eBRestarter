using eBRestarter.Core.Application.Interfaces.RestClient;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Api;
using eBRestarter.Infrastructure.Services.RestSharp;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace eBRestarter.Tests.Infrastructure.Services
{
    /// <summary>
    /// Testet den RestSharpClientService. Da dieser echte HTTP-Aufrufe macht,
    /// nutzen wir WireMock.Net, um einen lokalen, echten HTTP-Server hochzufahren,
    /// der API-Antworten (Mock-Responses) simuliert, ohne externe Dienste aufzurufen.
    /// </summary>
    public class RestSharpClientServiceTests : IDisposable
    {
        private readonly Mock<ILogger<RestSharpClientService>> _loggerMock;
        private readonly RestSharpClientService _sut; // SUT = System Under Test
        private readonly WireMockServer _server;

        public RestSharpClientServiceTests()
        {
            _loggerMock = new Mock<ILogger<RestSharpClientService>>();
            _sut = new RestSharpClientService(_loggerMock.Object);

            // Startet einen lokalen HTTP-Mock-Server für jeden Testdurchlauf.
            // Der Server läuft auf einem zufälligen, freien Port (z.B. localhost:51234).
            _server = WireMockServer.Start();
        }

        // =========================================================
        // 1. ASYNC TESTS (ExecuteGetAsync)
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Das ist der "Happy Path". Wenn die API sauber antwortet (HTTP 200),
        /// muss der Service den Inhalt (Body) extrahieren und als erfolgreich markieren.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren eine API, die "Test Content" zurückgibt. Wir prüfen, ob IsSuccess auf true
        /// steht, der Statuscode Success ist und der Content exakt übereinstimmt.
        /// </summary>
        [Fact]
        public async Task ExecuteGetAsync_Success_ReturnsSuccessAndContent()
        {
            // ARRANGE
            _server.Given(Request.Create().WithPath("/test").UsingGet())
                   .RespondWith(Response.Create()
                       .WithStatusCode(200)
                       .WithBody("Test Content"));

            var requestModel = new ApiRequest
            {
                Url = $"{_server.Urls[0]}/test",
                TimeoutSeconds = 5
            };

            // ACT
            var result = await _sut.ExecuteGetAsync(requestModel);

            // ASSERT
            Assert.True(result.IsSuccess);
            Assert.Equal(ResponseCode.Success, result.StatusCode);
            Assert.Equal("Test Content", result.Content);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Viele APIs haben Rate Limits. Wenn wir zu viele Anfragen stellen, müssen wir rechtzeitig drosseln.
        /// Deine eigene Logik prüft, ob "X-Ratelimit-Remaining" kleiner oder gleich 10 ist.
        ///
        /// WAS WIRD GETESTET?
        /// Wir senden absichtlich einen Header mit dem Wert "5". Der Test prüft, ob der Service das erkennt
        /// und trotz eines HTTP 200 den internen Enum-Wert auf "ResponseCode.RequestLimit" ändert.
        /// </summary>
        [Fact]
        public async Task ExecuteGetAsync_WithRateLimitWarning_ReturnsRequestLimitCode()
        {
            // ARRANGE
            _server.Given(Request.Create().WithPath("/ratelimit").UsingGet())
                   .RespondWith(Response.Create()
                       .WithStatusCode(200)
                       .WithHeader("X-Ratelimit-Remaining", "5") // Wert <= 10
                       .WithBody("OK"));

            var requestModel = new ApiRequest { Url = $"{_server.Urls[0]}/ratelimit", TimeoutSeconds = 5 };

            // ACT
            var result = await _sut.ExecuteGetAsync(requestModel);

            // ASSERT
            Assert.True(result.IsSuccess);
            Assert.Equal(ResponseCode.RequestLimit, result.StatusCode);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn die API einen Fehler wirft (z.B. fehlende Berechtigung), darf die App nicht abstürzen.
        /// Der HTTP-Fehlercode muss in unseren eigenen ResponseCode-Enum übersetzt und der Fehler geloggt werden.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren einen HTTP 401 (Unauthorized). Wir prüfen, ob das Mapping auf ResponseCode.HttpRE401
        /// funktioniert und ob der Logger aufgerufen wurde.
        /// </summary>
        [Fact]
        public async Task ExecuteGetAsync_Http401_ReturnsMappedErrorCode()
        {
            // ARRANGE
            _server.Given(Request.Create().WithPath("/unauthorized").UsingGet())
                   .RespondWith(Response.Create().WithStatusCode(401));

            var requestModel = new ApiRequest { Url = $"{_server.Urls[0]}/unauthorized", TimeoutSeconds = 5 };

            // ACT
            var result = await _sut.ExecuteGetAsync(requestModel);

            // ASSERT
            Assert.False(result.IsSuccess);
            Assert.Equal(ResponseCode.HttpRE401, result.StatusCode);

            VerifyLoggerWarningWasCalled();
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn Benutzername und Passwort übergeben werden, muss der Client diese als
        /// Base64-codierten Basic-Auth Header an die API senden.
        ///
        /// WAS WIRD GETESTET?
        /// Der Mock-Server antwortet NUR mit 200 OK, wenn exakt der Header "Basic dXNlcjpwYXNz" (user:pass) ankommt.
        /// Wenn der Client den Header nicht baut, schlägt die Anfrage beim Mock-Server fehl.
        /// </summary>
        [Fact]
        public async Task ExecuteGetAsync_WithBasicAuth_SendsCredentials()
        {
            // ARRANGE
            _server.Given(
                Request.Create()
                    .WithPath("/auth")
                    .UsingGet()
                    .WithHeader("Authorization", "Basic dXNlcjpwYXNz") // Base64 für "user:pass"
            ).RespondWith(Response.Create().WithStatusCode(200));

            var requestModel = new ApiRequest
            {
                Url = $"{_server.Urls[0]}/auth",
                TimeoutSeconds = 5,
                Username = "user",
                Password = "pass"
            };

            // ACT
            var result = await _sut.ExecuteGetAsync(requestModel);

            // ASSERT
            Assert.True(result.IsSuccess, "Die Anfrage ist fehlgeschlagen. Das bedeutet, dass der Auth-Header nicht oder falsch gesendet wurde.");
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Netzwerkverbindungen können hängen bleiben. Der Client muss nach der konfigurierten Zeit (TimeoutSeconds)
        /// abbrechen und darf den Thread nicht endlos blockieren.
        ///
        /// WAS WIRD GETESTET?
        /// Der Server braucht künstliche 6 Sekunden für die Antwort. Der Client darf aber nur 1 Sekunde warten.
        /// Erwartet wird, dass der Client abbricht und einen entsprechenden Timeout/General Fehler zurückgibt.
        /// </summary>
        [Fact]
        public async Task ExecuteGetAsync_Timeout_ReturnsMappedTimeoutCode()
        {
            // ARRANGE
            _server.Given(Request.Create().WithPath("/timeout").UsingGet())
                   .RespondWith(Response.Create()
                       .WithStatusCode(200)
                       .WithDelay(TimeSpan.FromSeconds(6))); // Server trödelt 6 Sekunden

            var requestModel = new ApiRequest { Url = $"{_server.Urls[0]}/timeout", TimeoutSeconds = 1 };

            // ACT
            var result = await _sut.ExecuteGetAsync(requestModel);

            // ASSERT
            Assert.False(result.IsSuccess);
            Assert.True(result.StatusCode == ResponseCode.HTTPTimeout || result.StatusCode == ResponseCode.GeneralExceptionError);
        }

        // =========================================================
        // 2. SYNC TESTS (ExecuteGet)
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wir stellen auch eine synchrone Methode zur Verfügung. Diese muss exakt dieselben
        /// Ergebnisse liefern wie die asynchrone Variante.
        ///
        /// WAS WIRD GETESTET?
        /// Ein normaler HTTP 200 Aufruf über ExecuteGet(). Es wird auf korrekten Status und Body geprüft.
        /// </summary>
        [Fact]
        public void ExecuteGet_SyncCall_Success_ReturnsSuccessAndContent()
        {
            // ARRANGE
            _server.Given(Request.Create().WithPath("/sync-test").UsingGet())
                   .RespondWith(Response.Create().WithStatusCode(200).WithBody("Sync Content"));

            var requestModel = new ApiRequest { Url = $"{_server.Urls[0]}/sync-test", TimeoutSeconds = 5 };

            // ACT
            var result = _sut.ExecuteGet(requestModel);

            // ASSERT
            Assert.True(result.IsSuccess);
            Assert.Equal(ResponseCode.Success, result.StatusCode);
            Assert.Equal("Sync Content", result.Content);
        }

        // =========================================================
        // 3. HELPER METHODEN
        // =========================================================

        private void VerifyLoggerWarningWasCalled()
        {
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("API Fehler")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        // =========================================================
        // CLEANUP (wird nach JEDEM Test automatisch ausgeführt)
        // =========================================================
        public void Dispose()
        {
            // Server nach jedem Test ordnungsgemäÃŸ herunterfahren,
            // um Port-Blockaden bei parallel laufenden Tests zu vermeiden.
            _server.Stop();
            _server.Dispose();
        }
    }
}