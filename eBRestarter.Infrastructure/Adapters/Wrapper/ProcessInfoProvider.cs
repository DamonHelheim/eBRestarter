using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

namespace eBRestarter.Infrastructure.Adapters.Wrapper;

public sealed class ProcessInfoProvider : IProcessInfoProviderOutboundPort
{
    public ProcessInfoProvider()
    {
    }

    public string GetCurrentExecutablePath()
    {
        return Environment.ProcessPath!;
    }
}


