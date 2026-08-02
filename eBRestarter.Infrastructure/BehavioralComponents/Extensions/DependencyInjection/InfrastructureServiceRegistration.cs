using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Validators;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Authentication;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Logging;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Network;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Security;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Update;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Handler.Http;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Handler.WindowsOS;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.API;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.API.Authentication;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS.Authentication;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Repositories.WindowsOS;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Service.Update;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Service.WindowsOS;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Utility;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Logging;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Validators;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.WindowsOS;
using eBRestarter.Infrastructure.Api.Interfaces;
using eBRestarter.Infrastructure.API;
using eBRestarter.Infrastructure.BehavioralComponents.Providers;
using eBRestarter.Infrastructure.BehavioralComponents.Repositories.Authentication;
using eBRestarter.Infrastructure.BehavioralComponents.Repositories.Config;
using eBRestarter.Infrastructure.BehavioralComponents.Wrappers;
using eBRestarter.Infrastructure.BehavioralComponents.Factories;
using eBRestarter.Infrastructure.BehavioralComponents.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.BehavioralComponents.Extensions.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddDebug());

        services.AddSingleton<IProcessWrapper, ProcessWrapper>();
        services.AddSingleton<IOutboundPortOsProcessControl, AdapterWindowsProcessControlWrapper>();
        services.AddSingleton<IOutboundPortFileSystem, AdapterWindowsFileSystem>();
        services.AddSingleton<IOutboundPortApplicationLifetime, AdapterWindowsApplicationLifetimeHandler>();
        services.AddSingleton<AdapterWindowsBrowserExtensionDeploymentProvider>();
        services.AddSingleton<IOutboundPortBrowserExtensionDeployment>(sp => sp.GetRequiredService<AdapterWindowsBrowserExtensionDeploymentProvider>());
        services.AddSingleton<IOutboundPortBrowserExtensionPathProvider>(sp => sp.GetRequiredService<AdapterWindowsBrowserExtensionDeploymentProvider>());
        services.AddSingleton<IOutboundPortUpdate, AdapterGitHubUpdateService>();
        services.AddSingleton<IOutboundPortFileDeletion, AdapterWindowsFileDeletionService>();
        services.AddSingleton<IOutboundPortEncryption, AdapterWindowsEncryptionUtility>();
        services.AddSingleton<IOutboundPortBrowserFactory, BrowserFactory>();
        services.AddSingleton<IOutboundPortBrowserDiscoveryProvider, AdapterWindowsBrowserDiscoveryProvider>();
        services.AddSingleton<IOutboundPortSystemInfoProvider, AdapterWindowsSystemInfoProvider>();
        services.AddSingleton<IOutboundPortEVisitorApiProvider, AdapterEVisitorApiProvider>();

        services.AddSingleton<IOutboundPortProcessInfoProvider, AdapterProcessInfoProvider>();
        services.AddSingleton<IOutboundPortAppPathProvider, AdapterWindowsAppPathProvider>();
        services.AddSingleton<IOutboundPortHardwareInfoProvider>(provider => provider.GetRequiredService<AdapterWmiHardwareProvider>());
        services.AddSingleton<IOutboundPortOsEditionProvider>(provider => provider.GetRequiredService<AdapterWmiHardwareProvider>());
        services.AddSingleton<IOutboundPortNetworkProvider, AdapterWindowsNetworkProvider>();
        services.AddSingleton<IInboundPortNetworkInfoProvider, NetworkInfoProvider>();
        services.AddSingleton<IInboundPortOsAppPathProvider, WindowsAppPathProvider>();
        services.AddSingleton<AdapterWmiHardwareProvider>();

        services.AddTransient<AdapterChromeBrowserWrapper>();
        services.AddTransient<AdapterFirefoxBrowserWrapper>();
        services.AddTransient<AdapterEdgeBrowserWrapper>();
        services.AddTransient<AdapterBraveBrowserWrapper>();
        services.AddTransient<AdapterVivaldiBrowserWrapper>();

        // Keyed Services DI Registrierung für Factory Pattern (.NET 10 Enterprise Standard)
        services.AddKeyedTransient<IOutboundPortBrowser, AdapterChromeBrowserWrapper>(BrowserType.Chrome);
        services.AddKeyedTransient<IOutboundPortBrowser, AdapterFirefoxBrowserWrapper>(BrowserType.Firefox);
        services.AddKeyedTransient<IOutboundPortBrowser, AdapterEdgeBrowserWrapper>(BrowserType.Edge);
        services.AddKeyedTransient<IOutboundPortBrowser, AdapterBraveBrowserWrapper>(BrowserType.Brave);
        services.AddKeyedTransient<IOutboundPortBrowser, AdapterVivaldiBrowserWrapper>(BrowserType.Vivaldi);

        services.AddSingleton<IOutboundPortHttpDownload, AdapterHttpClientDownloadHandler>();

        services.AddSingleton<IRestClient, RestSharpClient>();
        services.AddSingleton<IOutboundPortApiAuthenticationProvider, AdapterEVisitorApiAuthenticationProvider>();
        services.AddSingleton<IOutboundPortActiveDirectoryProvider, AdapterWindowsActiveDirectoryProvider>();
        services.AddSingleton<IOutboundPortCredentialValidationProvider, AdapterWindowsCredentialValidationProvider>();
        services.AddSingleton<IOutboundPortAppVersionInfoProvider, AdapterWindowsAppVersionInfoProvider>();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IOutboundPortSystemConfigurationRepository, AdapterWindowsRegistryRepository>();
        services.AddSingleton<IOutboundPortAutoStartRepository, AdapterWindowsStartupRepository>();
        services.AddSingleton<IOutboundPortBrowserConfigRepository, AdapterWindowsStartupRepository>();
        services.AddSingleton<IOutboundPortOsAutoLogonRepository, AdapterWindowsAutoLogonRepository>();
        services.AddSingleton<EVRestarterConfigRepository>();

        services.AddSingleton<IOutboundPortEVisitorConfigRepository>(provider =>

            new EncryptedEVisitorConfigRepositoryDecorator(

                provider.GetRequiredService<EVRestarterConfigRepository>(),
                provider.GetRequiredService<IOutboundPortEncryption>(),
                provider.GetRequiredService<ILogger<EncryptedEVisitorConfigRepositoryDecorator>>()

            ));

        services.AddSingleton<IOutboundPortCredentialStoreRepository, JsonCredentialStoreRepository>();

        services.AddValidatorsFromAssemblyContaining<ConfigureAutoLogonValidator>();
        services.AddTransient(typeof(IInboundPortApplicationValidator<>), typeof(AdapterFluentValidationWrapper<>));
        services.AddSingleton(typeof(IOutboundPortApplicationLogger<>), typeof(AdapterMicrosoftExtensionsLoggerWrapper<>));

        return services;
    }
}







































