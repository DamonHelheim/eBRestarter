using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using System;
using Xunit;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;
using eBRestarter.Infrastructure.BehavioralComponents.Factories;
using Microsoft.Extensions.Logging.Testing;

namespace eBRestarter.XUnit.Test.Infrastructure.Factories
{
    /// <summary>
    /// Unit tests for <see cref="BrowserFactory"/> verifying keyed service resolution and fallback instantiation.
    /// </summary>
    public class BrowserFactoryProviderAdapterTests
    {
        [Theory]
        [InlineData(BrowserType.Chrome, typeof(AdapterChromeBrowserWrapper))]
        [InlineData(BrowserType.Firefox, typeof(AdapterFirefoxBrowserWrapper))]
        [InlineData(BrowserType.Edge, typeof(AdapterEdgeBrowserWrapper))]
        [InlineData(BrowserType.Brave, typeof(AdapterBraveBrowserWrapper))]
        [InlineData(BrowserType.Vivaldi, typeof(AdapterVivaldiBrowserWrapper))]
        public void Create_ShouldReturnCorrectBrowserInstance_ForValidBrowserType(BrowserType inputType, Type expectedClassType)
        {
            // [R]IGHT: Valid browser type resolves to the corresponding concrete browser wrapper type
            // Arrange
            // A mock IServiceProvider is insufficient: BrowserFactory resolves via GetKeyedService, which
            // throws on providers that do not implement IKeyedServiceProvider. ServiceCollection natively
            // supports keyed services and verifies registration.
            var mockProcess = Substitute.For<IOutboundPortOsProcessControl>();
            var mockSettings = Substitute.For<IOutboundPortSystemConfigurationRepository>();
            var mockFs = Substitute.For<IOutboundPortFileSystem>();

            var fakeChrome = new AdapterChromeBrowserWrapper(mockProcess, mockSettings, mockFs, new FakeLogger<AdapterChromeBrowserWrapper>());
            var fakeFirefox = new AdapterFirefoxBrowserWrapper(mockProcess, mockSettings, mockFs, new FakeLogger<AdapterFirefoxBrowserWrapper>());
            var fakeEdge = new AdapterEdgeBrowserWrapper(mockProcess, mockSettings, mockFs, new FakeLogger<AdapterEdgeBrowserWrapper>());
            var fakeBrave = new AdapterBraveBrowserWrapper(mockProcess, mockSettings, mockFs, new FakeLogger<AdapterBraveBrowserWrapper>());
            var fakeVivaldi = new AdapterVivaldiBrowserWrapper(mockProcess, mockSettings, mockFs, new FakeLogger<AdapterVivaldiBrowserWrapper>());

            var services = new ServiceCollection();
            services.AddKeyedSingleton<IOutboundPortBrowser>(BrowserType.Chrome, fakeChrome);
            services.AddKeyedSingleton<IOutboundPortBrowser>(BrowserType.Firefox, fakeFirefox);
            services.AddKeyedSingleton<IOutboundPortBrowser>(BrowserType.Edge, fakeEdge);
            services.AddKeyedSingleton<IOutboundPortBrowser>(BrowserType.Brave, fakeBrave);
            services.AddKeyedSingleton<IOutboundPortBrowser>(BrowserType.Vivaldi, fakeVivaldi);

            var factory = new BrowserFactory(services.BuildServiceProvider());

            // Act
            var result = factory.Create(inputType);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBeOfType(expectedClassType);
        }

        [Fact]
        public void Create_ShouldThrowNotSupportedException_ForInvalidBrowserType()
        {
            // [E]RROR: Unsupported browser type enum value throws NotSupportedException
            // Arrange
            // An empty container ensures GetKeyedService returns null, routing execution
            // to the switch default branch which throws NotSupportedException.
            var factory = new BrowserFactory(new ServiceCollection().BuildServiceProvider());

            var invalidBrowserType = (BrowserType)999;

            // Act
            Action act = () => factory.Create(invalidBrowserType);

            // Assert
            var exception = act.ShouldThrow<NotSupportedException>();
            exception.Message.ShouldContain("is not supported");
            exception.Message.ShouldContain(nameof(BrowserFactory));
        }

        [Fact]
        public void Create_ShouldResolveBrowser_ViaKeyedServices()
        {
            // [R]IGHT: Registered keyed browser singleton is resolved directly from DI container
            // Arrange
            var services = new ServiceCollection();
            var mockProcess = Substitute.For<IOutboundPortOsProcessControl>();
            var mockSettings = Substitute.For<IOutboundPortSystemConfigurationRepository>();
            var mockFs = Substitute.For<IOutboundPortFileSystem>();
            var fakeChrome = new AdapterChromeBrowserWrapper(mockProcess, mockSettings, mockFs, new FakeLogger<AdapterChromeBrowserWrapper>());

            services.AddKeyedSingleton<IOutboundPortBrowser>(BrowserType.Chrome, fakeChrome);
            var sp = services.BuildServiceProvider();
            var factory = new BrowserFactory(sp);

            // Act
            var result = factory.Create(BrowserType.Chrome);

            // Assert
            result.ShouldBeSameAs(fakeChrome);
        }
    }
}
