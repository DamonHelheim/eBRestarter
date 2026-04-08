using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.RestClient;
using eBRestarter.Core.Domain.Models;
using eBRestarter.Core.Domain.Models.Records.Config;
using eBRestarter.Infrastructure.Constants;
using eBRestarter.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace eBRestarter.Tests.Infrastructure.Services
{
    /// <summary>
    /// Testet den EVisitorApiAdapter.
    /// Der Fokus liegt hier auf dem korrekten Parsen der verschiedenen JSON-Strukturen,
    /// dem Abfangen von Fehlern und der Parallelverarbeitung der API-Aufrufe.
    /// </summary>
    public class EVisitorApiAdapterTests
    {
        private readonly Mock<IRestClientService> _mockRestClient;
        private readonly Mock<IEVisitorConfigService> _mockConfigService;
        private readonly Mock<ILogger<EVisitorApiAdapter>> _mockLogger;
        private readonly EVisitorApiAdapter _sut;

        public EVisitorApiAdapterTests()
        {
            _mockRestClient = new Mock<IRestClientService>();
            _mockConfigService = new Mock<IEVisitorConfigService>();
            _mockLogger = new Mock<ILogger<EVisitorApiAdapter>>();

            _sut = new EVisitorApiAdapter(
                _mockRestClient.Object,
                _mockConfigService.Object,
                _mockLogger.Object);
        }

        // =========================================================
        // 1. IP INFO TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Stellt sicher, dass das einfache JSON der IP-Schnittstelle korrekt
        /// in das IpInfoData-Record gemappt wird.
        /// </summary>
        [Fact]
        public async Task GetIpInfoAsync_ShouldParseJsonCorrectly_WhenApiReturnsValidData()
        {
            // ARRANGE
            string json = @"{ ""ip"": ""192.168.1.1"", ""host"": ""test.host.com"", ""countryCode"": ""DE"", ""countryName"": ""Germany"" }";

            _mockRestClient
                .Setup(r => r.ExecuteGetAsync(It.Is<ApiRequest>(req => req.Url == ApiWebLinks.IpLink)))
                .ReturnsAsync(new ApiResponse { IsSuccess = true, Content = json });

            // ACT
            var result = await _sut.GetIpInfoAsync();

            // ASSERT
            result.ShouldNotBeNull();
            result.IpAddress.ShouldBe("192.168.1.1");
            result.Hostname.ShouldBe("test.host.com");
            result.CountryCode.ShouldBe("DE");
            result.CountryName.ShouldBe("Germany");
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn die API streikt (z.B. Timeout oder HTTP 500) oder kaputtes JSON liefert,
        /// darf die App nicht abstürzen, sondern muss graceful "null" zurückgeben.
        /// </summary>
        [Fact]
        public async Task GetIpInfoAsync_ShouldReturnNull_OnFailureOrInvalidJson()
        {
            // ARRANGE - Fall 1: API Fehler
            _mockRestClient
                .Setup(r => r.ExecuteGetAsync(It.IsAny<ApiRequest>()))
                .ReturnsAsync(new ApiResponse { IsSuccess = false });

            // ACT 1
            var resultFail = await _sut.GetIpInfoAsync();

            // ARRANGE - Fall 2: Defektes JSON
            _mockRestClient
                .Setup(r => r.ExecuteGetAsync(It.IsAny<ApiRequest>()))
                .ReturnsAsync(new ApiResponse { IsSuccess = true, Content = "Kein { gueltiges ] JSON" });

            // ACT 2
            var resultInvalid = await _sut.GetIpInfoAsync();

            // ASSERT
            resultFail.ShouldBeNull();
            resultInvalid.ShouldBeNull();
        }

        // =========================================================
        // 2. EARNINGS: GUARD CLAUSES & CONFIG
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Die API-Aufrufe kosten Zeit und Ressourcen. Wenn keine Zugangsdaten hinterlegt
        /// sind, muss der Prozess sofort abgebrochen werden.
        /// </summary>
        [Fact]
        public async Task GetEarningsAsync_ShouldReturnNull_WhenCredentialsAreMissing()
        {
            // ARRANGE
            // Config ohne Username und Key
            var emptyConfig = new AppConfig();
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(emptyConfig);

            // ACT
            var result = await _sut.GetEarningsAsync();

            // ASSERT
            result.ShouldBeNull();
            // Sicherstellen, dass KEIN API Aufruf stattfand
            _mockRestClient.Verify(r => r.ExecuteGetAsync(It.IsAny<ApiRequest>()), Times.Never);
        }

        // =========================================================
        // 3. EARNINGS: FULL WORKFLOW & PARSING
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Das ist der Kern des Adapters. Er ruft drei Endpunkte parallel ab und muss
        /// Datums-Strings (ISO 8601), JSON-Objekte, JSON-Arrays und gemischte Datentypen
        /// (Zahlen vs. Strings wie "50.5") sauber in das EarningsData-Record überführen.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren gültige Config-Daten und statten den RestClient-Mock mit Logik aus:
        /// - Für Hourly-URLs liefern wir ein Objekt ({"1": 10.5}).
        /// - Für Zeitspannen (Daily/Monthly) liefern wir ein Array mit historischen Daten,
        ///   das sowohl Zahlen als auch Strings enthält.
        /// </summary>
        [Fact]
        public async Task GetEarningsAsync_ShouldFetchAndAggregateDataCorrectly()
        {
            // ARRANGE
            var validConfig = new AppConfig();
            validConfig = validConfig with
            {
                Settings = validConfig.Settings with { ApiUsername = "user", ApiKey = "pass" }
            };
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(validConfig);

            // Dynamischer Mock für den RestClient
            _mockRestClient
                .Setup(r => r.ExecuteGetAsync(It.IsAny<ApiRequest>()))
                .ReturnsAsync((ApiRequest req) =>
                {
                    // 1. Hourly Earnings (Erwartet oft ein JSON Object, Stunden als Keys)
                    if (req.Url == ApiWebLinks.HourlyEarnings)
                    {
                        return new ApiResponse { IsSuccess = true, Content = @"{""1"": 10.5, ""3"": 20.25}" };
                    }

                    // 2. Daily & Monthly Earnings (Erwartet ein JSON Array mit Datum)
                    // Wir nutzen absichtlich ein Datum im Januar und eines im Februar.
                    // Außerdem mischen wir echte JSON-Numbers (100.0) und JSON-Strings ("50.5"),
                    // um deine GetValueSafe-Methode zu prüfen!
                    string historicalJson = @"[
                        { ""from_w3c"": ""2026-01-05T00:00:00Z"", ""value"": 100.0 },
                        { ""from_w3c"": ""2026-01-05T12:00:00Z"", ""value"": ""50.5"" },
                        { ""from_w3c"": ""2026-02-15T00:00:00Z"", ""value"": 200.0 }
                    ]";

                    return new ApiResponse { IsSuccess = true, Content = historicalJson };
                });

            // ACT
            var result = await _sut.GetEarningsAsync();

            // ASSERT
            result.ShouldNotBeNull();

            // --- Check 1: Hourly Array (Heute) ---
            // "1" = Index 0, "3" = Index 2
            result.HourlyEarnings[0].ShouldBe(10.5);
            result.HourlyEarnings[2].ShouldBe(20.25);
            result.TodaySum.ShouldBe(30.75); // 10.5 + 20.25

            // --- Check 2: Daily Array (Tage des Monats) ---
            // Januar 5 = Index 4. Werte: 100.0 + 50.5 (String-Parsing Test!) = 150.5
            result.DailyEarnings[4].ShouldBe(150.5);
            // Februar 15 = Index 14. Wert: 200.0
            result.DailyEarnings[14].ShouldBe(200.0);

            result.MonthlySum.ShouldBe(350.5); // Summe aller Daily-Werte

            // --- Check 3: Monthly Array (Monate des Jahres) ---
            // Januar = Index 0 (100.0 + 50.5 = 150.5)
            result.MonthlyEarnings[0].ShouldBe(150.5);
            // Februar = Index 1 (200.0)
            result.MonthlyEarnings[1].ShouldBe(200.0);

            result.YearlySum.ShouldBe(350.5); // Summe aller Monthly-Werte
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Du hast eine spezielle Fallback-Logik in GetHourlyEarningsRawAsync eingebaut,
        /// falls die API statt eines Objekts {"1": 10.5} plötzlich ein Array [10.5, 20.0] sendet.
        /// </summary>
        [Fact]
        public async Task GetEarningsAsync_ShouldHandleHourlyArrayFallback_Correctly()
        {
            // ARRANGE
            var validConfig = new AppConfig();
            validConfig = validConfig with { Settings = validConfig.Settings with { ApiUsername = "u", ApiKey = "p" } };
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(validConfig);

            _mockRestClient
                .Setup(r => r.ExecuteGetAsync(It.IsAny<ApiRequest>()))
                .ReturnsAsync((ApiRequest req) =>
                {
                    if (req.Url == ApiWebLinks.HourlyEarnings)
                    {
                        // Senden eines Arrays statt eines Objekts!
                        return new ApiResponse { IsSuccess = true, Content = @"[15.0, 5.0]" };
                    }
                    // Leere Historie für den Rest
                    return new ApiResponse { IsSuccess = true, Content = "[]" };
                });

            // ACT
            var result = await _sut.GetEarningsAsync();

            // ASSERT
            result.ShouldNotBeNull();
            result.HourlyEarnings[0].ShouldBe(15.0);
            result.HourlyEarnings[1].ShouldBe(5.0);
            result.TodaySum.ShouldBe(20.0);
        }
    }
}