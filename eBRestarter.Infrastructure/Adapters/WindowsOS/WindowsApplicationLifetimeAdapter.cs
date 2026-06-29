using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Formatters;
using eBRestarter.Core.Application.Ports.Outbound;

namespace eBRestarter.Infrastructure.Adapters.WindowsOS;

public sealed class WindowsApplicationLifetimeAdapter : IApplicationLifetimePort
{
    public WindowsApplicationLifetimeAdapter()
    {
    }

    public void ExitApplication(int exitCode)
    {
        Environment.Exit(exitCode);
    }
}
