using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace eBRestarter.Infrastructure.Services.WindowsOS
{
    public class NetworkProvider : INetworkProvider
    {
        public bool GetIsNetworkAvailable() => NetworkInterface.GetIsNetworkAvailable();
        public NetworkInterface[] GetAllNetworkInterfaces() => NetworkInterface.GetAllNetworkInterfaces();
    }
}
