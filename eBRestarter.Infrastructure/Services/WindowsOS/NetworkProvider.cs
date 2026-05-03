using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using System.Net.NetworkInformation;

namespace eBRestarter.Infrastructure.Services.WindowsOS
{
    public class NetworkProvider : INetworkProvider
    {
        public bool GetIsNetworkAvailable() => NetworkInterface.GetIsNetworkAvailable();
        public NetworkInterface[] GetAllNetworkInterfaces() => NetworkInterface.GetAllNetworkInterfaces();
    }
}
