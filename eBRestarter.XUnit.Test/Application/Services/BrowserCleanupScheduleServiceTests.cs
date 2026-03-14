using eBRestarter.Core.Application.Services;
using eBRestarter.Core.Domain.Models.Records.Config;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using System;
using Xunit;

namespace eBRestarter.XUnit.Test.Application.Services;

public class BrowserCleanupScheduleServiceTests
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly BrowserCleanupScheduleService _sut;

    public BrowserCleanupScheduleServiceTests()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        // Set fixed start time: 2026-03-10 14:00:00
        _fakeTimeProvider.SetUtcNow(new DateTimeOffset(2026, 3, 10, 14, 0, 0, TimeSpan.Zero));
        _sut = new BrowserCleanupScheduleService(_fakeTimeProvider);
    }

    [Fact]
    public void ShouldRunCleanupNow_WhenConfigIsNull_ShouldReturnFalse()
    {
        // Act
        var result = _sut.ShouldRunCleanupNow(null!);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldRunCleanupNow_WhenIntervalIsZero_ShouldReturnFalse()
    {
        // Arrange
        var config = new AppConfig();
        config.Browser.DeleteBrowserCacheIntervalDays = 0;
        config.Browser.NextBrowserDeleteCacheDate = new DateTime(2026, 3, 9); // In the past

        // Act
        var result = _sut.ShouldRunCleanupNow(config);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldRunCleanupNow_WhenDateIsPastOrToday_ShouldReturnTrue()
    {
        // Arrange
        var config = new AppConfig();
        config.Browser.DeleteBrowserCacheIntervalDays = 1;

        // Today is 2026-03-10
        config.Browser.NextBrowserDeleteCacheDate = new DateTime(2026, 3, 10);

        // Act
        var result = _sut.ShouldRunCleanupNow(config);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ShouldRunCleanupNow_WhenDateIsInFuture_ShouldReturnFalse()
    {
        // Arrange
        var config = new AppConfig();
        config.Browser.DeleteBrowserCacheIntervalDays = 1;

        // Today is 2026-03-10
        config.Browser.NextBrowserDeleteCacheDate = new DateTime(2026, 3, 11);

        // Act
        var result = _sut.ShouldRunCleanupNow(config);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void GetNextCleanupDateAfterRun_ShouldAddIntervalDays()
    {
        // Arrange
        var fromDate = new DateTime(2026, 3, 10);
        int intervalDays = 7;

        // Act
        var result = _sut.GetNextCleanupDateAfterRun(fromDate, intervalDays);

        // Assert
        result.ShouldBe(new DateTime(2026, 3, 17));
    }
}