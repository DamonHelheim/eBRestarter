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
    /// PrÃ¼ft, ob die Antworten des Rest-Clients korrekt in boolesche Ergebnisse
    /// und lesbare UI-Nachrichten Ã¼bersetzt werden.
    /// </summary>
    public class EVisitorApiAuthenticationUseCaseTests
    {
        /// <summary>
        /// Das ist der "Happy Path". Wenn der RestClient einen Erfolg meldet,
        /// muss die Methode 'true' und eine Erfolgsmeldung zurÃ¼ckgeben.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren eine erfolgreiche API-Antwort. ZusÃ¤tzlich prÃ¼fen wir streng,
        /// ob der Service den Ã¼bergebenen Username und API-Key auch WIRKLICH in das
        /// 'ApiRequest'-Objekt gemappt hat, bevor er es an den RestClient schickt.
        /// </summary>
        [Fact]
        public async Task VerifyCredentialsAsync_ShouldReturnTrue_WhenApiCallIsSuccessful()
        {
            // ARRANGE
            var mockRestClient = new Mock<IRestClientUseCase>();

            string testUsername = "TestUser";
            string testApiKey = "SecretKey123";

            // Wir definieren die gefÃ¤lschte "Erfolgs-Antwort"
            var fakeSuccessResponse = new ApiResponse
            {
                IsSuccess = true,
                StatusCode = ResponseCode.HttpRE200 // Angenommen, das ist dein 200 OK Enum
            };

            // Setup: Wenn ExecuteGetAsync aufgerufen wird, gib den Erfolg zurÃ¼ck.
            // Gleichzeitig fangen wir ab, mit welchem Request die Methode aufgerufen wurde.
            mockRestClient
                .Setup(client => client.ExecuteGetAsync(It.IsAny<ApiRequest>()))
                .ReturnsAsync(fakeSuccessResponse);

            var apiUseCase = new EVisitorApiAuthenticationAdapter(mockRestClient.Object);

            // ACT
            var (isValid, message) = await apiUseCase.VerifyCredentialsAsync(testUsername, testApiKey);

            // ASSERT
            isValid.ShouldBeTrue();
            message.ShouldBe("Verbindung erfolgreich!");

            // SICHERHEITS-CHECK: Hat der Service die Parameter richtig ins Request-Objekt gesteckt?
            // Wir prÃ¼fen, ob ExecuteGetAsync mit einem Objekt aufgerufen wurde, das genau
            // unseren Username und das Passwort (ApiKey) enthÃ¤lt.
            mockRestClient.Verify(client => client.ExecuteGetAsync(It.Is<ApiRequest>(req =>
                req.Username == testUsername && req.Password == testApiKey)), Times.Once);
        }

        /// <summary>
        /// Wenn der Nutzer sich vertippt, meldet die API einen 401 Unauthorized Fehler.
        /// Diesen kryptischen Fehler wollen wir fÃ¼r den Nutzer in eine verstÃ¤ndliche
        /// deutsche Fehlermeldung Ã¼bersetzen.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren eine API-Antwort mit IsSuccess = false und dem spezifischen
        /// StatusCode = 401. Wir prÃ¼fen, ob exakt die geplante Fehlermeldung generiert wird.
        /// </summary>
        [Fact]
        public async Task VerifyCredentialsAsync_ShouldReturnFalseAndSpecificMessage_WhenCredentialsAreInvalid()
        {
            // ARRANGE
            var mockRestClient = new Mock<IRestClientUseCase>();

            var fakeUnauthorizedResponse = new ApiResponse
            {
                IsSuccess = false,
                StatusCode = ResponseCode.HttpRE401 // Der spezifische Fehler aus deinem Code
            };

            mockRestClient
                .Setup(client => client.ExecuteGetAsync(It.IsAny<ApiRequest>()))
                .ReturnsAsync(fakeUnauthorizedResponse);

            var apiUseCase = new EVisitorApiAuthenticationAdapter(mockRestClient.Object);

            // ACT
            var (isValid, message) = await apiUseCase.VerifyCredentialsAsync("WrongUser", "WrongKey");

            // ASSERT
            isValid.ShouldBeFalse();
            message.ShouldBe("Benutzername oder API-SchlÃ¼ssel falsch.");
        }

        /// <summary>
        /// Was passiert bei einem Timeout, wenn das Internet des Nutzers weg ist oder
        /// die eBesucher-API komplett down ist (500 Internal Server Error)?
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren einen beliebigen anderen Fehlercode inkl. einer 'ErrorMessage' vom RestClient.
        /// Wir prÃ¼fen, ob die Methode gracefully mit 'false' abbricht und die System-Fehlermeldung
        /// sauber in den Ausgabestring einbaut.
        /// </summary>
        [Fact]
        public async Task VerifyCredentialsAsync_ShouldReturnFalseAndErrorMessage_OnGeneralApiError()
        {
            // ARRANGE
            var mockRestClient = new Mock<IRestClientUseCase>();

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

            var apiUseCase = new EVisitorApiAuthenticationAdapter(mockRestClient.Object);

            // ACT
            var (isValid, message) = await apiUseCase.VerifyCredentialsAsync("User", "Key");

            // ASSERT
            isValid.ShouldBeFalse();
            // PrÃ¼ft, ob der String mit "Fehler: " beginnt und die original Message anhÃ¤ngt
            message.ShouldBe($"Fehler: {systemErrorMessage}");
        }
    }
}


