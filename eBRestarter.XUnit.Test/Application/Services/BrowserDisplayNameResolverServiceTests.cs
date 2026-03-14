using eBRestarter.Core.Application.Services;
using eBRestarter.Core.Domain.Enums;
using Shouldly;
using Xunit;

namespace eBRestarter.XUnit.Test.Application.Services;

public class BrowserDisplayNameResolverServiceTests
{
    private readonly BrowserDisplayNameResolverService _sut;

    public BrowserDisplayNameResolverServiceTests()
    {
        _sut = new BrowserDisplayNameResolverService();
    }

    [Theory]
    [InlineData("Chrome", "Default", BrowserType.Chrome)]
    [InlineData("Edge", "Default", BrowserType.Edge)]
    [InlineData("Firefox", "Default", BrowserType.Firefox)]
    [InlineData("Brave", "Default", BrowserType.Brave)]
    [InlineData("Vivaldi", "Default", BrowserType.Vivaldi)]
    [InlineData("CHROME", "Default", BrowserType.Chrome)] // Case insensitive
    public void GetBrowserTypeFromDisplayName_WhenValidEnumName_ShouldReturnEnum(string displayName, string defaultText, BrowserType expected)
    {
        // Act
        var result = _sut.GetBrowserTypeFromDisplayName(displayName, defaultText);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("", "Default", BrowserType.Chrome)]
    [InlineData(" ", "Default", BrowserType.Chrome)]
    [InlineData(null, "Default", BrowserType.Chrome)]
    [InlineData("Default", "Default", BrowserType.Chrome)]
    [InlineData("Nicht gewählt", "Default", BrowserType.Chrome)]
    [InlineData("UnknownBrowser", "Default", BrowserType.Chrome)] // Invalid enum
    public void GetBrowserTypeFromDisplayName_WhenInvalidOrFallback_ShouldReturnChrome(string? displayName, string defaultText, BrowserType expected)
    {
        // Act
        var result = _sut.GetBrowserTypeFromDisplayName(displayName!, defaultText);

        // Assert
        result.ShouldBe(expected);
    }
}