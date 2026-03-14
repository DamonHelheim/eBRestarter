using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.UseCases.ManageRestarterCycle;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models.Records.Config;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace eBRestarter.XUnit.Test.Application.UseCases;

public class ManageRestarterCycleServiceTests
{
    private readonly Mock<IBrowserFactory> _mockBrowserFactory;
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly Mock<IBrowserDisplayNameResolver> _mockBrowserResolver;
    private readonly Mock<IEVisitorConfigService> _mockConfigService;
    private readonly Mock<IBrowserCleanupScheduleService> _mockCleanupScheduleService;
    private readonly Mock<IBrowser> _mockBrowser;
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly ManageRestarterCycleService _sut;

    public ManageRestarterCycleServiceTests()
    {
        _mockBrowserFactory = new Mock<IBrowserFactory>();
        _mockLocalizationService = new Mock<ILocalizationService>();
        _mockBrowserResolver = new Mock<IBrowserDisplayNameResolver>();
        _mockConfigService = new Mock<IEVisitorConfigService>();
        _mockCleanupScheduleService = new Mock<IBrowserCleanupScheduleService>();
        _mockBrowser = new Mock<IBrowser>();
        
        _fakeTimeProvider = new FakeTimeProvider();

        _mockLocalizationService.Setup(l => l.GetString(It.IsAny<string>())).Returns((string key) => key);
        _mockBrowserFactory.Setup(f => f.Create(It.IsAny<BrowserType>())).Returns(_mockBrowser.Object);
        _mockBrowserResolver.Setup(r => r.GetBrowserTypeFromDisplayName(It.IsAny<string>(), It.IsAny<string>())).Returns(BrowserType.Chrome);

        _sut = new ManageRestarterCycleService(
            _mockBrowserFactory.Object,
            _mockLocalizationService.Object,
            _mockBrowserResolver.Object,
            _mockConfigService.Object,
            _mockCleanupScheduleService.Object,
            _fakeTimeProvider);
    }

    [Fact]
    public async Task StartAsync_ShouldRunInitialDelayAndThenLaunchBrowser()
    {
        // Arrange
        var request = new ManageRestarterCycleRequest("Chrome", "testUser", 10, 5);
        var progressUpdates = new List<RestarterCycleProgress>();
        
        _sut.ProgressChanged += (s, e) => progressUpdates.Add(e);

        var cleanupInvoked = false;
        Func<Task> cleanupCallback = () => { cleanupInvoked = true; return Task.CompletedTask; };
        
        _mockConfigService.Setup(c => c.LoadConfig()).Returns(new AppConfig());
        _mockCleanupScheduleService.Setup(s => s.ShouldRunCleanupNow(It.IsAny<AppConfig>())).Returns(false); // No cleanup needed

        // Act
        var task = _sut.StartAsync(request, cleanupCallback);

        // Advance until Running phase
        for (int i = 0; i < 20; i++)
        {
            _fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));
            await Task.Delay(10); // allow continuations to run
            if (progressUpdates.Exists(p => p.State == RestartTaskState.Running)) break;
        }

        // Advance until Cooldown phase
        for (int i = 0; i < 20; i++)
        {
            _fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));
            await Task.Delay(10); // allow continuations to run
            if (progressUpdates.Exists(p => p.State == RestartTaskState.Cooldown)) break;
        }

        // Stop the cycle
        _sut.Stop();
        
        await task; // Now we await to let it finish gracefully

        // Assert
        progressUpdates.ShouldNotBeEmpty();
        progressUpdates.ShouldContain(p => p.State == RestartTaskState.InitialDelay);
        progressUpdates.ShouldContain(p => p.State == RestartTaskState.Running);
        progressUpdates.ShouldContain(p => p.State == RestartTaskState.Cooldown);
        
        _mockBrowserFactory.Verify(f => f.Create(BrowserType.Chrome), Times.AtLeastOnce);
        _mockBrowser.Verify(b => b.Start("https://www.ebesucher.com/surfbar/testUser"), Times.AtLeastOnce);
        _mockBrowser.Verify(b => b.Close(), Times.AtLeastOnce);
        cleanupInvoked.ShouldBeFalse();
    }

    [Fact]
    public async Task StartAsync_WhenCleanupIsDue_ShouldInvokeCleanupCallback()
    {
        // Arrange
        var request = new ManageRestarterCycleRequest("Chrome", "testUser", 2, 2);
        var cleanupInvoked = false;
        Func<Task> cleanupCallback = () => { cleanupInvoked = true; return Task.CompletedTask; };

        _mockConfigService.Setup(c => c.LoadConfig()).Returns(new AppConfig());
        _mockCleanupScheduleService.Setup(s => s.ShouldRunCleanupNow(It.IsAny<AppConfig>())).Returns(true); // Cleanup IS due
        _mockCleanupScheduleService.Setup(s => s.GetNextCleanupDateAfterRun(It.IsAny<DateTime>(), It.IsAny<int>())).Returns(DateTime.Today.AddDays(7));

        // Act
        var task = _sut.StartAsync(request, cleanupCallback);

        // Advance until cleanup is invoked or timeout
        for (int i = 0; i < 30; i++)
        {
            _fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));
            await Task.Delay(10); // allow continuations to run
            if (cleanupInvoked) break;
        }
        
        _sut.Stop();
        await task;

        // Assert
        cleanupInvoked.ShouldBeTrue();
        _mockConfigService.Verify(c => c.SaveConfig(It.IsAny<AppConfig>()), Times.Once); // Verify config updated with new date
    }
}
