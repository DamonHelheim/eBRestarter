using eBRestarter.Core.Application.Services;
using Shouldly;
using Xunit;

namespace eBRestarter.XUnit.Test.Application.Services;

public class CacheDeletionIntervalValidatorTests
{
    private readonly CacheDeletionIntervalValidator _sut;

    public CacheDeletionIntervalValidatorTests()
    {
        _sut = new CacheDeletionIntervalValidator();
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(7, true)]
    [InlineData(14, true)]
    public void IsValidIntervalDays_WhenValidDays_ShouldReturnTrue(int days, bool expected)
    {
        // Act
        var result = _sut.IsValidIntervalDays(days);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(2, false)]
    [InlineData(5, false)]
    [InlineData(-1, false)]
    [InlineData(30, false)]
    public void IsValidIntervalDays_WhenInvalidDays_ShouldReturnFalse(int days, bool expected)
    {
        // Act
        var result = _sut.IsValidIntervalDays(days);

        // Assert
        result.ShouldBe(expected);
    }
}