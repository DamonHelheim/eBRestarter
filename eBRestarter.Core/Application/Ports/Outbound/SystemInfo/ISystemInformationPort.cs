using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.SystemInfo;

public interface ISystemInformationPort
{
    Task<SystemInformationResponse> ExecuteAsync();
}