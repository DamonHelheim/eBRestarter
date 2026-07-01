namespace eBRestarter.Core.Application.Ports.Inbound.Providers;

public interface ILocalizationProvider
{
    string RetrieveString(string key);
}
