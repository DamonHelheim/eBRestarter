using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Interfaces.RestClient;
using eBRestarter.Core.Application.Interfaces.Security;
using eBRestarter.Core.Application.Interfaces.Update;
using eBRestarter.Infrastructure.Browsers;
using eBRestarter.Infrastructure.Facades;
using eBRestarter.Infrastructure.Factories;
using eBRestarter.Infrastructure.Services;
using eBRestarter.Infrastructure.Services.Scheduling;
using eBRestarter.Infrastructure.Services.Authentication;
using eBRestarter.Infrastructure.Services.Config;
using eBRestarter.Infrastructure.Services.RestSharp;
using eBRestarter.Infrastructure.Services.Update;
using eBRestarter.Infrastructure.Services.WindowsOS;
using eBRestarter.Infrastructure.Services.WindowsOS.Security;
using eBRestarter.Infrastructure.Wrapper;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS.Process;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddDebug());
        services.AddSingleton<IProcessWrapper, RealProcessWrapperAdapter>();
        services.AddSingleton<IWindowsProcessControlService, WindowsProcessAdapter>();
        services.AddSingleton<IWindowsSystemInfoService, WindowsSystemInfoService>();

        services.AddSingleton<IProcessInfoUseCase, ProcessInfoAdapter>();
        services.AddSingleton<IWindowsRegistryService, WindowsRegistryAdapter>();
        services.AddSingleton<IWindowsFileSystemService, WindowsFileSystemServiceAdapter>();
        services.AddSingleton<IWindowsStartupManagerService, WindowsStartupServiceAdapter>();

        services.AddSingleton<IWindowsAutoLogonService, WindowsAutoLogonAdapter>();
        services.AddSingleton<IEncryptionUseCase, WindowsEncryptionHandler>();
        services.AddSingleton<IPathProvider, WindowsPathProviderAdapter>();
        services.AddSingleton<IApplicationLifetimeUseCase, WindowsApplicationLifetimeHandler>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<WmiHardwareAdapter>();

        services.AddSingleton<IHardwareInfoService>(provider => provider.GetRequiredService<WmiHardwareAdapter>());

        services.AddSingleton<IOsEditionService>(provider => provider.GetRequiredService<WmiHardwareAdapter>());

        services.AddSingleton<IOperatingSystemFacade, OperatingSystemFacadeAdapter>();

        services.AddSingleton<INetworkProvider, WindowsNetworkAdapter>();

        services.AddSingleton<IWindowsNetworkInfoUseCase, WindowsNetworkInfoAdapter>();

        services.AddTransient<ChromeBrowserAdapter>();
        services.AddTransient<FirefoxBrowserAdapter>();
        services.AddTransient<EdgeBrowserAdapter>();
        services.AddTransient<BraveBrowserAdapter>();
        services.AddTransient<VivaldiBrowserAdapter>();

        services.AddSingleton<IBrowserFactory, BrowserFactory>();

        services.AddSingleton<IBrowserService, WindowsBrowserService>();

        services.AddSingleton<IBrowserDownloadUseCase, HttpClientDownloadHandler>();

        services.AddSingleton<IRestClientUseCase, RestSharpClientHandler>();

        services.AddSingleton<IPathUseCase, WindowsPathHandler>();

        services.AddSingleton<EVisitorConfigService>();

        services.AddSingleton<IEVisitorConfigService>(provider =>
            new EncryptedEVisitorConfigServiceDecorator(
                provider.GetRequiredService<EVisitorConfigService>(),
                provider.GetRequiredService<IEncryptionUseCase>(),
                provider.GetRequiredService<ILogger<EncryptedEVisitorConfigServiceDecorator>>()
            ));

        services.AddSingleton<IAppInfoUseCase, AppInfoAdapter>();

        services.AddSingleton<IFileDeletionUseCase, FileDeletionHandler>();

        services.AddSingleton<IApiAuthenticationUseCase, EVisitorApiAuthenticationAdapter>();

        services.AddSingleton<ICredentialStore, JsonCredentialStoreRepository>();

        services.AddSingleton<IEVisitorApiService, EVisitorApiAdapter>();

        services.AddSingleton<IBrowserDisplayNameResolverUseCase, BrowserDisplayNameResolverHandler>();

        services.AddSingleton<IBrowserExtensionDeploymentUseCase, BrowserExtensionDeploymentHandler>();

        services.AddSingleton<IUpdateService, GitHubUpdateAdapter>();

        services.AddSingleton<IActiveDirectoryUseCase, WindowsActiveDirectoryAdapter>();

        services.AddSingleton<ICredentialValidationUseCase, CredentialValidationHandler>();

        services.AddSingleton<IAppVersionInfoUseCase, WindowsAppVersionInfoHandler>();

        services.AddSingleton<IComputerRestartScheduler, ComputerRestartScheduler>();

        return services;
    }
}











