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
        services.AddSingleton<IProcessWrapper, RealProcessWrapper>();
        services.AddSingleton<IWindowsProcessControlService, WindowsProcessService>();
        services.AddSingleton<IWindowsSystemInfoService, WindowsSystemInfoService>();

        services.AddSingleton<IProcessInfoService, ProcessInfoService>();
        services.AddSingleton<IWindowsRegistryService, WindowsRegistryService>();
        services.AddSingleton<IWindowsFileSystemService, WindowsFileSystemService>();
        services.AddSingleton<IWindowsStartupManagerService, WindowsStartupService>();

        services.AddSingleton<IWindowsAutoLogonService, WindowsAutoLogonService>();
        services.AddSingleton<IEncryptionService, WindowsEncryptionService>();
        services.AddSingleton<IPathProvider, WindowsPathProvider>();
        services.AddSingleton<IApplicationLifetime, WindowsApplicationLifetime>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<WindowsWmiHardwareService>();

        services.AddSingleton<IHardwareInfoService>(provider => provider.GetRequiredService<WindowsWmiHardwareService>());

        services.AddSingleton<IOsEditionService>(provider => provider.GetRequiredService<WindowsWmiHardwareService>());

        services.AddSingleton<IOperatingSystemFacade, OperatingSystemFacade>();

        services.AddSingleton<INetworkProvider, NetworkProvider>();

        services.AddSingleton<IWindowsNetworkInfoService, WindowsNetworkInfoService>();

        services.AddTransient<ChromeBrowser>();
        services.AddTransient<FirefoxBrowser>();
        services.AddTransient<EdgeBrowser>();
        services.AddTransient<BraveBrowser>();
        services.AddTransient<VivaldiBrowser>();

        services.AddSingleton<IBrowserFactory, BrowserFactory>();

        services.AddSingleton<IBrowserService, WindowsBrowserService>();

        services.AddSingleton<IBrowserDownloadService, HttpClientDownloadService>();

        services.AddSingleton<IRestClientService, RestSharpClientService>();

        services.AddSingleton<IPathService, WindowsPathService>();

        services.AddSingleton<IEVisitorConfigService, EVisitorConfigService>();

        services.AddSingleton<IAppInfoService, AppInfoService>();

        services.AddSingleton<IFileDeletionService, FileDeletionService>();

        services.AddSingleton<IApiAuthenticationService, EVisitorApiAuthenticationService>();

        services.AddSingleton<ICredentialStore, JsonCredentialStore>();

        services.AddSingleton<IEVisitorApiService, EVisitorApiAdapter>();

        services.AddSingleton<IBrowserDisplayNameResolver, BrowserDisplayNameResolverService>();

        services.AddSingleton<IBrowserExtensionDeploymentService, BrowserExtensionDeploymentService>();

        services.AddSingleton<IUpdateService, GitHubUpdateAdapter>();

        services.AddSingleton<IActiveDirectoryService, WindowsActiveDirectoryService>();

        services.AddSingleton<ICredentialValidationService, PrincipalContextCredentialValidationService>();

        services.AddSingleton<IAppVersionInfoService, WindowsAppVersionInfoService>();

        services.AddSingleton<IComputerRestartScheduler, ComputerRestartScheduler>();

        return services;
    }
}
