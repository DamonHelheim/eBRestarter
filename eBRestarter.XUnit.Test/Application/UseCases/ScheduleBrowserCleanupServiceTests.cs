using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.UseCases.ScheduleBrowserCleanup;
using eBRestarter.Core.Domain.Models.Records.Config;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shouldly;
using System;
using Xunit;

namespace eBRestarter.XUnit.Test.Application.UseCases;

public class ScheduleBrowserCleanupServiceTests
{
    private readonly Mock<IEVisitorConfigService> _mockConfigService;
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly ScheduleBrowserCleanupService _sut;

    public ScheduleBrowserCleanupServiceTests()
    {
        _mockConfigService = new Mock<IEVisitorConfigService>();
        _fakeTimeProvider = new FakeTimeProvider();
        
        // Start time at 2026-03-10
        _fakeTimeProvider.SetUtcNow(new DateTimeOffset(2026, 3, 10, 12, 0, 0, TimeSpan.Zero));

        _sut = new ScheduleBrowserCleanupService(_mockConfigService.Object, _fakeTimeProvider);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(7, true)]
    [InlineData(14, true)]
    [InlineData(5, false)] // 5 days is not allowed
    [InlineData(0, false)] // 0 is off
    public void UpdateSchedule_WhenCalled_ShouldCalculateNextDateCorrectly(int intervalDays, bool expectedActive)
    {
        // Arrange
        var config = new AppConfig();
        _mockConfigService.Setup(c => c.LoadConfig()).Returns(config);
        var request = new ScheduleBrowserCleanupRequest(intervalDays);

        // Act
        var response = _sut.UpdateSchedule(request);

        // Assert
        response.IsActive.ShouldBe(expectedActive);
        
        if (expectedActive)
        {
            var expectedDate = _fakeTimeProvider.GetLocalNow().Date.AddDays(intervalDays);
            response.NextDate.ShouldBe(expectedDate);
            config.Browser.NextBrowserDeleteCacheDate.ShouldBe(expectedDate);
        }
        else
        {
            response.NextDate.ShouldBeNull();
            config.Browser.NextBrowserDeleteCacheDate.ShouldBe(DateTime.MinValue);
        }

        config.Browser.DeleteBrowserCacheIntervalDays.ShouldBe(intervalDays);
        _mockConfigService.Verify(c => c.SaveConfig(config), Times.Once);
    }
}
