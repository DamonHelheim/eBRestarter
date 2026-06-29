using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Browser;

namespace eBRestarter.Core.Application.Ports.Outbound.Browser;

public interface IBrowserFactoryPort
{
    IBrowserPort Create(BrowserType type);
}



