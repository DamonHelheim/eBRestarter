using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Domain.ValueObjects;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.API;
using eBRestarter.Infrastructure.Api;
using eBRestarter.Infrastructure.Api.Interfaces;
using eBRestarter.Infrastructure.ObjectArchetypes.Model;
using Microsoft.Extensions.Logging.Testing;

namespace eBRestarter.Tests.Infrastructure.Adapters
{
    /// <summary>
    /// Unit tests for <see cref="AdapterEVisitorApiProvider"/> verifying API response parsing, error handling, and payload aggregation.
    /// </summary>
    public class EVisitorApiProviderTests
    {
        private readonly IRestClient _mockRestClient;
        private readonly IOutboundPortEVisitorConfigRepository _mockConfigService;
        private readonly FakeLogger<AdapterEVisitorApiProvider> _mockLogger;
        private readonly AdapterEVisitorApiProvider _sut;

        public EVisitorApiProviderTests()
        {
            _mockRestClient = Substitute.For<IRestClient>();
            _mockConfigService = Substitute.For<IOutboundPortEVisitorConfigRepository>();
            _mockLogger = new FakeLogger<AdapterEVisitorApiProvider>();

            _sut = new AdapterEVisitorApiProvider(
                _mockConfigService,
                _mockLogger,
                _mockRestClient);
        }

        [Fact]
        public async Task GetIpInfoAsync_ShouldParseJsonCorrectly_WhenApiReturnsValidData()
        {
            // [R]IGHT: Maps valid IP info JSON payload into strongly typed IpInfoData record
            // Arrange
            string json = @"{ ""ip"": ""192.168.1.1"", ""host"": ""test.host.com"", ""countryCode"": ""DE"", ""countryName"": ""Germany"" }";

            _mockRestClient
                .ExecuteGetAsync(Arg.Is<ApiRequest>(req => req.Url == ApiWebLinks.IpLink))
                .Returns(new ApiResponse { IsSuccess = true, Content = json });

            // Act
            var result = await _sut.RetrieveIpInfoAsync();

            // Assert
            result.ShouldNotBeNull();
            result.IpAddress.ShouldBe("192.168.1.1");
            result.Hostname.ShouldBe("test.host.com");
            result.CountryCode.ShouldBe("DE");
            result.CountryName.ShouldBe("Germany");
        }

        [Fact]
        public async Task GetIpInfoAsync_ShouldReturnNull_OnFailureOrInvalidJson()
        {
            // [E]RROR / [B]OUNDARY: Returns null when API request fails or returns malformed JSON
            // Arrange
            _mockRestClient
                .ExecuteGetAsync(Arg.Any<ApiRequest>())
                .Returns(new ApiResponse { IsSuccess = false });

            // Act
            var resultFail = await _sut.RetrieveIpInfoAsync();

            _mockRestClient
                .ExecuteGetAsync(Arg.Any<ApiRequest>())
                .Returns(new ApiResponse { IsSuccess = true, Content = "Invalid { JSON [" });

            var resultInvalid = await _sut.RetrieveIpInfoAsync();

            // Assert
            resultFail.ShouldBeNull();
            resultInvalid.ShouldBeNull();
        }

        [Fact]
        public async Task GetEarningsAsync_ShouldReturnNull_WhenCredentialsAreMissing()
        {
            // [B]OUNDARY: Aborts early and returns null when configuration lacks credentials without calling REST client
            // Arrange
            var emptyConfig = new AppConfig();
            _mockConfigService.LoadConfig().Returns(emptyConfig);

            // Act
            var result = await _sut.RetrieveEarningsAsync();

            // Assert
            result.ShouldBeNull();
            await _mockRestClient.DidNotReceive().ExecuteGetAsync(Arg.Any<ApiRequest>());
        }

        [Fact]
        public async Task GetEarningsAsync_ShouldFetchAndAggregateDataCorrectly()
        {
            // [R]IGHT: Fetches hourly, daily, and monthly earnings and correctly aggregates sums
            // Arrange
            var validConfig = new AppConfig();
            validConfig.Settings.ApiUsername = "user";
            validConfig.Settings.ApiKey = "pass";
            _mockConfigService.LoadConfig().Returns(validConfig);

            _mockRestClient
                .ExecuteGetAsync(Arg.Any<ApiRequest>())
                .Returns(ci => {
                    if (ci.Arg<ApiRequest>().Url == ApiWebLinks.HourlyEarnings)
                    {
                        return new ApiResponse { IsSuccess = true, Content = @"{""1"": 10.5, ""3"": 20.25}" };
                    }

                    string historicalJson = @"[
                        { ""from_w3c"": ""2026-01-05T00:00:00Z"", ""value"": 100.0 },
                        { ""from_w3c"": ""2026-01-05T12:00:00Z"", ""value"": ""50.5"" },
                        { ""from_w3c"": ""2026-02-15T00:00:00Z"", ""value"": 200.0 }
                    ]";

                    return new ApiResponse { IsSuccess = true, Content = historicalJson };
                });

            // Act
            var result = await _sut.RetrieveEarningsAsync();

            // Assert
            result.ShouldNotBeNull();
            result.HourlyEarnings[0].ShouldBe(10.5);
            result.HourlyEarnings[2].ShouldBe(20.25);
            result.TodaySum.ShouldBe(30.75);
            result.DailyEarnings[4].ShouldBe(150.5);
            result.DailyEarnings[14].ShouldBe(200.0);
            result.MonthlySum.ShouldBe(350.5);
            result.MonthlyEarnings[0].ShouldBe(150.5);
            result.MonthlyEarnings[1].ShouldBe(200.0);
            result.YearlySum.ShouldBe(350.5);
        }

        [Fact]
        public async Task GetEarningsAsync_ShouldHandleHourlyArrayFallback_Correctly()
        {
            // [B]OUNDARY: Correctly parses hourly earnings fallback when API returns an array instead of a key-value object
            // Arrange
            var validConfig = new AppConfig();
            validConfig.Settings.ApiUsername = "u";
            validConfig.Settings.ApiKey = "p";
            _mockConfigService.LoadConfig().Returns(validConfig);

            _mockRestClient
                .ExecuteGetAsync(Arg.Any<ApiRequest>())
                .Returns(ci => {
                    if (ci.Arg<ApiRequest>().Url == ApiWebLinks.HourlyEarnings)
                    {
                        return new ApiResponse { IsSuccess = true, Content = @"[15.0, 5.0]" };
                    }
                    return new ApiResponse { IsSuccess = true, Content = "[]" };
                });

            // Act
            var result = await _sut.RetrieveEarningsAsync();

            // Assert
            result.ShouldNotBeNull();
            result.HourlyEarnings[0].ShouldBe(15.0);
            result.HourlyEarnings[1].ShouldBe(5.0);
            result.TodaySum.ShouldBe(20.0);
        }
    }
}
