using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using eBRestarter.Core.Application.Common.Results;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Infrastructure.API;
using eBRestarter.Infrastructure.ObjectArchetypes.Enums;
using eBRestarter.Infrastructure.ObjectArchetypes.Model;
using Microsoft.Extensions.Logging.Testing;
using System.Net.Http;
using System.Linq;
using Shouldly;
using System;

namespace eBRestarter.Tests.Infrastructure.Services
{
    /// <summary>
    /// Integration and unit tests for <see cref="RestSharpClient"/> using WireMock.Net to mock HTTP responses.
    /// </summary>
    public class RestSharpClientAdapterTests : IDisposable
    {
        private readonly FakeLogger<RestSharpClient> _loggerMock;
        private readonly RestSharpClient _sut;
        private readonly WireMockServer _server;

        public RestSharpClientAdapterTests()
        {
            _loggerMock = new FakeLogger<RestSharpClient>();
            // Uses a pooled HttpClient from IHttpClientFactory instead of instantiating per-request handlers.
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient());

            _sut = new RestSharpClient(_loggerMock, httpClientFactory);

            // Starts a local WireMock server on a random available port for each test.
            _server = WireMockServer.Start();
        }

        [Fact]
        public async Task ExecuteGetAsync_Success_ReturnsSuccessAndContent()
        {
            // [R]IGHT: Valid URL returns HTTP 200 with response body content
            // Arrange
            _server.Given(Request.Create().WithPath("/test").UsingGet())
                   .RespondWith(Response.Create()
                       .WithStatusCode(200)
                       .WithBody("Test Content"));

            var requestModel = new ApiRequest
            {
                Url = $"{_server.Urls[0]}/test",
                TimeoutSeconds = 5
            };

            // Act
            var result = await _sut.ExecuteGetAsync(requestModel);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ResponseCode.Success, result.StatusCode);
            Assert.Equal("Test Content", result.Content);
        }

        [Fact]
        public async Task ExecuteGetAsync_WithRateLimitWarning_ReturnsRequestLimitCode()
        {
            // [B]OUNDARY: Remaining rate limit header <= 10 maps to RequestLimit status code
            // Arrange
            _server.Given(Request.Create().WithPath("/ratelimit").UsingGet())
                   .RespondWith(Response.Create()
                       .WithStatusCode(200)
                       .WithHeader("X-Ratelimit-Remaining", "5")
                       .WithBody("OK"));

            var requestModel = new ApiRequest { Url = $"{_server.Urls[0]}/ratelimit", TimeoutSeconds = 5 };

            // Act
            var result = await _sut.ExecuteGetAsync(requestModel);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ResponseCode.RequestLimit, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteGetAsync_Http401_ReturnsMappedErrorCode()
        {
            // [E]RROR: HTTP 401 Unauthorized maps to HttpRE401 and logs warning
            // Arrange
            _server.Given(Request.Create().WithPath("/unauthorized").UsingGet())
                   .RespondWith(Response.Create().WithStatusCode(401));

            var requestModel = new ApiRequest { Url = $"{_server.Urls[0]}/unauthorized", TimeoutSeconds = 5 };

            // Act
            var result = await _sut.ExecuteGetAsync(requestModel);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ResponseCode.HttpRE401, result.StatusCode);

            VerifyLoggerWarningWasCalled();
        }

        [Fact]
        public async Task ExecuteGetAsync_WithBasicAuth_SendsCredentials()
        {
            // [R]IGHT: Basic authentication credentials are sent as Base64 Authorization header
            // Arrange
            _server.Given(
                Request.Create()
                    .WithPath("/auth")
                    .UsingGet()
                    .WithHeader("Authorization", "Basic dXNlcjpwYXNz")
            ).RespondWith(Response.Create().WithStatusCode(200));

            var requestModel = new ApiRequest
            {
                Url = $"{_server.Urls[0]}/auth",
                TimeoutSeconds = 5,
                Username = "user",
                Password = "pass"
            };

            // Act
            var result = await _sut.ExecuteGetAsync(requestModel);

            // Assert
            Assert.True(result.IsSuccess, "The request failed, indicating that the Authorization header was missing or improperly formatted.");
        }

        [Fact]
        public async Task ExecuteGetAsync_Timeout_ReturnsMappedTimeoutCode()
        {
            // [E]RROR: Request exceeding configured timeout returns HTTPTimeout status
            // Arrange
            _server.Given(Request.Create().WithPath("/timeout").UsingGet())
                   .RespondWith(Response.Create()
                       .WithStatusCode(200)
                       .WithDelay(TimeSpan.FromSeconds(6)));

            var requestModel = new ApiRequest { Url = $"{_server.Urls[0]}/timeout", TimeoutSeconds = 1 };

            // Act
            var result = await _sut.ExecuteGetAsync(requestModel);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.StatusCode == ResponseCode.HTTPTimeout || result.StatusCode == ResponseCode.GeneralExceptionError);
        }

        [Fact]
        public void ExecuteGet_SyncCall_Success_ReturnsSuccessAndContent()
        {
            // [C]ROSS-CHECK: Synchronous invocation executes GET request and returns success with content
            // Arrange
            _server.Given(Request.Create().WithPath("/sync-test").UsingGet())
                   .RespondWith(Response.Create().WithStatusCode(200).WithBody("Sync Content"));

            var requestModel = new ApiRequest { Url = $"{_server.Urls[0]}/sync-test", TimeoutSeconds = 5 };

            // Act
            var result = _sut.ExecuteGet(requestModel);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ResponseCode.Success, result.StatusCode);
            Assert.Equal("Sync Content", result.Content);
        }

        /// <summary>
        /// Verifies that a warning-level log entry matching the expected API error format was recorded.
        /// </summary>
        private void VerifyLoggerWarningWasCalled()
        {
            var matchingWarnings = _loggerMock.Collector.GetSnapshot()
                .Where(entry => entry.Level == LogLevel.Warning
                                && entry.Message.Contains("api error", StringComparison.OrdinalIgnoreCase))
                .ToList();

            matchingWarnings.ShouldNotBeEmpty();
        }

        /// <summary>
        /// Stops and disposes the WireMock server instance to release the allocated network port.
        /// </summary>
        public void Dispose()
        {
            _server.Stop();
            _server.Dispose();
        }
    }
}
