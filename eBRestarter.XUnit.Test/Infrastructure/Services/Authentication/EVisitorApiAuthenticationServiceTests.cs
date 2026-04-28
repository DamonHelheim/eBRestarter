using eBRestarter.Core.Application.Interfaces.RestClient;

using eBRestarter.Core.Application.Enums;

using eBRestarter.Core.Application.Models.Api;

using eBRestarter.Infrastructure.Services.Authentication;

using Moq;

using Shouldly;

using System.Threading.Tasks;

using Xunit;



namespace eBRestarter.XUnit.Test.Infrastructure.Services.Authentication

{

    /// <summary>

    /// Testet die Logik zur Verifizierung der API-Zugangsdaten.

    /// Prüft, ob die Antworten des Rest-Clients korrekt in boolesche Ergebnisse

    /// und lesbare UI-Nachrichten übersetzt werden.

    /// </summary>

    public class EVisitorApiAuthenticationServiceTests

    {

        /// <summary>

        /// WARUM WIRD DAS GETESTET?

        /// Das ist der "Happy Path". Wenn der RestClient einen Erfolg meldet,

        /// muss die Methode 'true' und eine Erfolgsmeldung zurückgeben.

        ///

        /// WAS WIRD GETESTET?

        /// Wir simulieren eine erfolgreiche API-Antwort. Zusätzlich prüfen wir streng,

        /// ob der Service den übergebenen Username und API-Key auch WIRKLICH in das

        /// 'ApiRequest'-Objekt gemappt hat, bevor er es an den RestClient schickt.

        /// </summary>

        [Fact]

        public async Task VerifyCredentialsAsync_ShouldReturnTrue_WhenApiCallIsSuccessful()

        {

            // ==========================================

            // ARRANGE

            // ==========================================

            var mockRestClient = new Mock<IRestClientService>();



            string testUsername = "TestUser";

            string testApiKey = "SecretKey123";



            // Wir definieren die gefälschte "Erfolgs-Antwort"

            var fakeSuccessResponse = new ApiResponse

            {

                IsSuccess = true,

                StatusCode = ResponseCode.HttpRE200 // Angenommen, das ist dein 200 OK Enum

            };



            // Setup: Wenn ExecuteGetAsync aufgerufen wird, gib den Erfolg zurück.

            // Gleichzeitig fangen wir ab, mit welchem Request die Methode aufgerufen wurde.

            mockRestClient

                .Setup(client => client.ExecuteGetAsync(It.IsAny<ApiRequest>()))

                .ReturnsAsync(fakeSuccessResponse);



            var apiService = new EVisitorApiAuthenticationService(mockRestClient.Object);



            // ==========================================

            // ACT

            // ==========================================

            var (isValid, message) = await apiService.VerifyCredentialsAsync(testUsername, testApiKey);



            // ==========================================

            // ASSERT

            // ==========================================

            isValid.ShouldBeTrue();

            message.ShouldBe("Verbindung erfolgreich!");



            // SICHERHEITS-CHECK: Hat der Service die Parameter richtig ins Request-Objekt gesteckt?

            // Wir prüfen, ob ExecuteGetAsync mit einem Objekt aufgerufen wurde, das genau

            // unseren Username und das Passwort (ApiKey) enthält.

            mockRestClient.Verify(client => client.ExecuteGetAsync(It.Is<ApiRequest>(req =>

                req.Username == testUsername &&

                req.Password == testApiKey

            )), Times.Once);

        }



        /// <summary>

        /// WARUM WIRD DAS GETESTET?

        /// Wenn der Nutzer sich vertippt, meldet die API einen 401 Unauthorized Fehler.

        /// Diesen kryptischen Fehler wollen wir für den Nutzer in eine verständliche

        /// deutsche Fehlermeldung übersetzen.

        ///

        /// WAS WIRD GETESTET?

        /// Wir simulieren eine API-Antwort mit IsSuccess = false und dem spezifischen

        /// StatusCode = 401. Wir prüfen, ob exakt die geplante Fehlermeldung generiert wird.

        /// </summary>

        [Fact]

        public async Task VerifyCredentialsAsync_ShouldReturnFalseAndSpecificMessage_WhenCredentialsAreInvalid()

        {

            // ARRANGE

            var mockRestClient = new Mock<IRestClientService>();



            var fakeUnauthorizedResponse = new ApiResponse

            {

                IsSuccess = false,

                StatusCode = ResponseCode.HttpRE401 // Der spezifische Fehler aus deinem Code

            };



            mockRestClient

                .Setup(client => client.ExecuteGetAsync(It.IsAny<ApiRequest>()))

                .ReturnsAsync(fakeUnauthorizedResponse);



            var apiService = new EVisitorApiAuthenticationService(mockRestClient.Object);



            // ACT

            var (isValid, message) = await apiService.VerifyCredentialsAsync("WrongUser", "WrongKey");



            // ASSERT

            isValid.ShouldBeFalse();

            message.ShouldBe("Benutzername oder API-Schlüssel falsch.");

        }



        /// <summary>

        /// WARUM WIRD DAS GETESTET?

        /// Was passiert bei einem Timeout, wenn das Internet des Nutzers weg ist oder

        /// die eBesucher-API komplett down ist (500 Internal Server Error)?

        ///

        /// WAS WIRD GETESTET?

        /// Wir simulieren einen beliebigen anderen Fehlercode inkl. einer 'ErrorMessage' vom RestClient.

        /// Wir prüfen, ob die Methode gracefully mit 'false' abbricht und die System-Fehlermeldung

        /// sauber in den Ausgabestring einbaut.

        /// </summary>

        [Fact]

        public async Task VerifyCredentialsAsync_ShouldReturnFalseAndErrorMessage_OnGeneralApiError()

        {

            // ARRANGE

            var mockRestClient = new Mock<IRestClientService>();



            string systemErrorMessage = "Es konnte keine Verbindung zum Zielserver hergestellt werden (Timeout).";



            var fakeGeneralErrorResponse = new ApiResponse

            {

                IsSuccess = false,

                StatusCode = ResponseCode.HttpRE500, // Irgendein anderer Code als 401

                ErrorMessage = systemErrorMessage

            };



            mockRestClient

                .Setup(client => client.ExecuteGetAsync(It.IsAny<ApiRequest>()))

                .ReturnsAsync(fakeGeneralErrorResponse);



            var apiService = new EVisitorApiAuthenticationService(mockRestClient.Object);



            // ACT

            var (isValid, message) = await apiService.VerifyCredentialsAsync("User", "Key");



            // ASSERT

            isValid.ShouldBeFalse();

            // Prüft, ob der String mit "Fehler: " beginnt und die original Message anhängt

            message.ShouldBe($"Fehler: {systemErrorMessage}");

        }

    }

}

