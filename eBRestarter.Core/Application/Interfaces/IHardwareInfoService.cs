using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Interfaces;

public interface IHardwareInfoService
{
    Task<HardwareInfo> RetrieveHardwareInfoAsync();
}
