using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

namespace eBRestarter.Infrastructure.Adapters.Wrapper;

public sealed class ProcessInfoAdapter : IProcessInfoPort
{
    public ProcessInfoAdapter()
    {
    }

    public string GetCurrentExecutablePath()
    {
        return Environment.ProcessPath!;
    }
}


