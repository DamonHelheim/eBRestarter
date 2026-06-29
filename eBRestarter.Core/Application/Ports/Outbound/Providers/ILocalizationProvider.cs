using eBRestarter.Core.Application.Ports.Outbound.Providers;
using eBRestarter.Core.Application.Providers;
namespace eBRestarter.Core.Application.Ports.Outbound.Providers;

public interface ILocalizationProvider
{
    string RetrieveString(string key);
}
