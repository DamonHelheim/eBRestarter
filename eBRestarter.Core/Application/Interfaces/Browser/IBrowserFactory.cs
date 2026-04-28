using eBRestarter.Core.Application.Enums;

namespace eBRestarter.Core.Application.Interfaces.Browser;

public interface IBrowserFactory
{
    IBrowser Create(BrowserType type);
}
