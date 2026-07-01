using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

public interface IHardwareInfoProviderOutboundPort
{
    Task<HardwareInfo> RetrieveHardwareInfoAsync();
}


