using System.Net.NetworkInformation;

namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS
{
    // Abstrahiert die statischen Systemaufrufe
    public interface INetworkProvider
    {
        bool GetIsNetworkAvailable();
        NetworkInterface[] GetAllNetworkInterfaces();
    }
}
