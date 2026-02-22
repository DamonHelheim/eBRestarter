using eBRestarter.Core.Domain.Models.Records;

namespace eBRestarter.Core.Application.Interfaces;

public interface IHardwareInfoService
{
    Task<HardwareInfo> GetHardwareInfoAsync();
}
