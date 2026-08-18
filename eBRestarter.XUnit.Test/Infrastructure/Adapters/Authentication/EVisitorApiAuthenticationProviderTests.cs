using NSubstitute;
using Shouldly;
using System.Threading.Tasks;
using Xunit;
using eBRestarter.Core.Application.Common.Results;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.API.Authentication;
using eBRestarter.Infrastructure.Api.Interfaces;
using eBRestarter.Infrastructure.ObjectArchetypes.Enums;
using eBRestarter.Infrastructure.ObjectArchetypes.Model;

namespace eBRestarter.XUnit.Test.Infrastructure.Adapters.Authentication
{
    /// <summary>
    /// Unit tests for <see cref="AdapterEVisitorApiAuthenticationProvider"/> verifying API credential verification, response mapping, and error handling.
    /// </summary>
    public class EVisitorApiAuthenticationProviderTests
    {
        [Fact]
        public async Task VerifyCredentialsAsync_ShouldReturnTrue_WhenApiCallIsSuccessful()
        {
            // [R]IGHT: Valid credentials return true and success message, verifying correct request parameter mapping
            // Arrange
            var mockRestClient = Substitute.For<IRestClient>();

            string testUsername = "TestUser";
            string testApiKey = "SecretKey123";

            var fakeSuccessResponse = new ApiResponse
            {
                IsSuccess = true,
                StatusCode = ResponseCode.HttpRE200
            };

            mockRestClient
                .ExecuteGetAsync(Arg.Any<ApiRequest>())
                .Returns(fakeSuccessResponse);

            var apiUseCase = new AdapterEVisitorApiAuthenticationProvider(mockRestClient);

            // Act
            var (isValid, message) = await apiUseCase.VerifyCredentialsAsync(testUsername, testApiKey);

            // Assert
            isValid.ShouldBeTrue();
            message.ShouldBe("Connection successful!");

            await mockRestClient.Received(1).ExecuteGetAsync(Arg.Is<ApiRequest>(req =>
                req.Username == testUsername && req.Password == testApiKey));
        }

        [Fact]
        public async Task VerifyCredentialsAsync_ShouldReturnFalseAndSpecificMessage_WhenCredentialsAreInvalid()
        {
            // [E]RROR: HTTP 401 Unauthorized returns false with invalid credentials message
            // Arrange
            var mockRestClient = Substitute.For<IRestClient>();

            var fakeUnauthorizedResponse = new ApiResponse
            {
                IsSuccess = false,
                StatusCode = ResponseCode.HttpRE401
            };

            mockRestClient
                .ExecuteGetAsync(Arg.Any<ApiRequest>())
                .Returns(fakeUnauthorizedResponse);

            var apiUseCase = new AdapterEVisitorApiAuthenticationProvider(mockRestClient);

            // Act
            var (isValid, message) = await apiUseCase.VerifyCredentialsAsync("WrongUser", "WrongKey");

            // Assert
            isValid.ShouldBeFalse();
            message.ShouldBe("Invalid username or API key.");
        }

        [Fact]
        public async Task VerifyCredentialsAsync_ShouldReturnFalseAndErrorMessage_OnGeneralApiError()
        {
            // [E]RROR: General API error or non-401 failure returns false with formatted system error message
            // Arrange
            var mockRestClient = Substitute.For<IRestClient>();

            string systemErrorMessage = "Connection timed out.";

            var fakeGeneralErrorResponse = new ApiResponse
            {
                IsSuccess = false,
                StatusCode = ResponseCode.HttpRE500,
                ErrorMessage = systemErrorMessage
            };

            mockRestClient
                .ExecuteGetAsync(Arg.Any<ApiRequest>())
                .Returns(fakeGeneralErrorResponse);

            var apiUseCase = new AdapterEVisitorApiAuthenticationProvider(mockRestClient);

            // Act
            var (isValid, message) = await apiUseCase.VerifyCredentialsAsync("User", "Key");

            // Assert
            isValid.ShouldBeFalse();
            message.ShouldBe($"Error: {systemErrorMessage}");
        }
    }
}
