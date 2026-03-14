using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.UseCases.GetSystemInformation;
using eBRestarter.Core.Domain.Models.Records;
using Moq;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace eBRestarter.XUnit.Test.Application.UseCases;

public class GetSystemInformationServiceTests
{
    private readonly Mock<IHardwareInfoService> _mockHardwareService;
    private readonly Mock<IOsEditionService> _mockOsEditionService;
    private readonly Mock<IWindowsSystemInfoService> _mockSystemInfoService;
    private readonly GetSystemInformationService _sut;

    public GetSystemInformationServiceTests()
    {
        _mockHardwareService = new Mock<IHardwareInfoService>();
        _mockOsEditionService = new Mock<IOsEditionService>();
        _mockSystemInfoService = new Mock<IWindowsSystemInfoService>();

        _sut = new GetSystemInformationService(
            _mockHardwareService.Object,
            _mockOsEditionService.Object,
            _mockSystemInfoService.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnCombinedSystemInformation()
    {
        // Arrange
        var hardwareInfo = new HardwareInfo { ProcessorName = "Intel Core i9", GraphicsCardName = "NVIDIA RTX 4090", InstalledRam = "32 GB" };
        _mockHardwareService.Setup(h => h.GetHardwareInfoAsync()).ReturnsAsync(hardwareInfo);
        
        _mockOsEditionService.Setup(o => o.GetOsEditionAsync()).ReturnsAsync("Windows 11 Pro");
        
        _mockSystemInfoService.Setup(s => s.GetCurrentOsDisplayVersion()).Returns("23H2");
        _mockSystemInfoService.Setup(s => s.GetCurrentOsBuildVersion()).Returns("22631");
        _mockSystemInfoService.Setup(s => s.GetCurrentStandardBrowserName()).Returns("Google Chrome");

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.ShouldNotBeNull();
        result.ProcessorName.ShouldBe("Intel Core i9");
        result.GraphicsCardName.ShouldBe("NVIDIA RTX 4090");
        result.InstalledRam.ShouldBe("32 GB");
        result.OsEdition.ShouldBe("Windows 11 Pro");
        result.OsDisplayVersion.ShouldBe("23H2");
        result.OsBuildVersion.ShouldBe("22631");
        result.StandardBrowserName.ShouldBe("Google Chrome");

        _mockHardwareService.Verify(h => h.GetHardwareInfoAsync(), Times.Once);
        _mockOsEditionService.Verify(o => o.GetOsEditionAsync(), Times.Once);
        _mockSystemInfoService.Verify(s => s.GetCurrentOsDisplayVersion(), Times.Once);
        _mockSystemInfoService.Verify(s => s.GetCurrentOsBuildVersion(), Times.Once);
        _mockSystemInfoService.Verify(s => s.GetCurrentStandardBrowserName(), Times.Once);
    }
}
