using eBRestarter.Core.Application.Services;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using System;
using Xunit;

namespace eBRestarter.XUnit.Test.Application.Services;

public class RestartCalculationServiceTests
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly RestartCalculationService _sut;

    public RestartCalculationServiceTests()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        // Set fixed start time: 2026-03-10 14:00:00
        _fakeTimeProvider.SetUtcNow(new DateTimeOffset(2026, 3, 10, 14, 0, 0, TimeSpan.Zero));
        _sut = new RestartCalculationService(_fakeTimeProvider);
    }

    [Fact]
    public void GetNextRestartDate_WhenIntervalIsGreaterThanZero_ShouldCalculateDate()
    {
        // Arrange
        int intervalDays = 2;
        int restartClockTime = 9; // 09:00

        // Act
        var result = _sut.GetNextRestartDate(intervalDays, restartClockTime);

        // Assert
        // Expected: Today (2026-03-10) + 2 days = 2026-03-12. Plus 9 hours = 2026-03-12 09:00:00
        result.ShouldBe(new DateTime(2026, 3, 12, 9, 0, 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetNextRestartDate_WhenIntervalIsZeroOrNegative_ShouldReturnMinValue(int intervalDays)
    {
        // Arrange
        int restartClockTime = 9;

        // Act
        var result = _sut.GetNextRestartDate(intervalDays, restartClockTime);

        // Assert
        result.ShouldBe(DateTime.MinValue);
    }
}