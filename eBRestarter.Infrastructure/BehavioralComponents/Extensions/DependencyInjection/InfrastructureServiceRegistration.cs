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

/// <summary>
/// Extension methods for registering Infrastructure adapter services and outbound port implementations into the dependency injection container.
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Registers infrastructure outbound adapters, repositories, wrappers, and external API clients with the DI container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same service collection for method chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // 📝 Logging-Guideline Kap. 8: Hier stand bis zuletzt
        // "services.AddLogging(builder => builder.AddDebug());" – eine Provider-Entscheidung
        // in einer Klassenbibliothek. Die Registrierung liegt jetzt im Composition Root der
        // Anwendung (LoggingServiceExtensions.AddApplicationLogging im Desktop-Projekt).
        // Diese Bibliothek konsumiert ausschließlich ILogger<T> per DI (Kap. 2).

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

        // 🔒 Security-Guideline Kap. 7.3: Vertrauensprüfung für heruntergeladene Binaries
        // (Update-Pakete und Browser-Installer) vor deren Ausführung.
        services.AddSingleton<IOutboundPortExecutableSignatureVerifier, AdapterWindowsAuthenticodeVerifier>();
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

        services.AddKeyedTransient<IOutboundPortBrowser, AdapterChromeBrowserWrapper>(BrowserType.Chrome);
        services.AddKeyedTransient<IOutboundPortBrowser, AdapterFirefoxBrowserWrapper>(BrowserType.Firefox);
        services.AddKeyedTransient<IOutboundPortBrowser, AdapterEdgeBrowserWrapper>(BrowserType.Edge);
        services.AddKeyedTransient<IOutboundPortBrowser, AdapterBraveBrowserWrapper>(BrowserType.Brave);
        services.AddKeyedTransient<IOutboundPortBrowser, AdapterVivaldiBrowserWrapper>(BrowserType.Vivaldi);

        services.AddSingleton<IOutboundPortHttpDownload, AdapterHttpClientDownloadHandler>();

        // ⚡ Guide Kap. 16.1: "new HttpClient() pro Request erzeugt keinen neuen Pool, sondern hält
        // Sockets offen → Socket-Exhaustion und DNS-Stale-Bugs." RestSharp erzeugte bisher pro
        // Request einen eigenen Handler. Die Factory hält stattdessen einen gemeinsamen
        // Connection-Pool und rotiert den Handler alle 5 Minuten (DNS-Rotation).
        services.AddHttpClient(RestSharpClient.HttpClientName)
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

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
