namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

public interface IProcessInfoProviderOutboundPort
{
    string GetCurrentExecutablePath();
}


