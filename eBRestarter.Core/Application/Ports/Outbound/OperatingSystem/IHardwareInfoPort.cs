using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

public interface IHardwareInfoPort
{
    Task<HardwareInfo> RetrieveHardwareInfoAsync();
}


