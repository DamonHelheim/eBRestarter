using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using eBRestarter.Classes.Converter;
using eBRestarter.Model.Models;

namespace eBRestarter.Views.UserControls
{

    public partial class UC_Networktraffic : UserControl
    {
        private readonly string _forgroundcolorTheme = string.Empty;
        private readonly string _imagePathNetworkCardThemes = string.Empty;
        private readonly string _imagePathReceivedDataThemes = string.Empty;
        private readonly string _imagePathSendDataThemes = string.Empty;

        private readonly NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
        private readonly ConvertSizeSuffixes css = new();

        private readonly CancellationTokenSource source = new ();

        public UC_Networktraffic()
        {
            InitializeComponent();

            UpdateNetWorkTrafficData();
        }

        public void UpdateNetWorkTrafficData()
        {
            CancellationToken token = source.Token;

            ThreadStart @start = GetSpecificNetworkData;

            var UpdateNetworkDataThread = new Thread(@start)
            {
                Name = "UpdateNetworkDataThread",
                IsBackground = true,
            };

            UpdateNetworkDataThread.Start();
        }

        private void GetSpecificNetworkData()
        {
            while (true)
            {
                try
                {
                    List<NetWorkCardAndTrafficInfo> networkcards = [];

                    this.Dispatcher.Invoke(new Action(() =>
                    {

                        if (!NetworkInterface.GetIsNetworkAvailable())
                        {

                            networkcards.Add(

                                new NetWorkCardAndTrafficInfo
                                {
                                    ImagePathNetworkCard = _imagePathNetworkCardThemes!,
                                    ImagePathReceivedData = _imagePathReceivedDataThemes!,
                                    ImagePathSendData = _imagePathSendDataThemes!,

                                    AdapterName = "Netzwerkkarte: nicht verfügbar",
                                    ReceivedData = "Empfangen: -",
                                    SendData = "Gesendet: -",

                                    Foregroundcolor = _forgroundcolorTheme!
                                }
                            );
                        }

                        foreach (NetworkInterface nic in interfaces)
                        {

                            if (css.SizeSuffix(nic.GetIPv4Statistics().BytesSent).Equals("0.0 bytes"))
                            {

                            }
                            else
                            {
                                networkcards.Add(

                                    new NetWorkCardAndTrafficInfo
                                    {
                                        ImagePathNetworkCard = _imagePathNetworkCardThemes!,
                                        ImagePathReceivedData = _imagePathReceivedDataThemes!,
                                        ImagePathSendData = _imagePathSendDataThemes!,

                                        AdapterName = "Netzwerkkarte: " + nic.Name,
                                        ReceivedData = "Empfangen: " + css.SizeSuffix(nic.GetIPv4Statistics().BytesReceived),
                                        SendData = "Gesendet: " + css.SizeSuffix(nic.GetIPv4Statistics().BytesSent),

                                        Foregroundcolor = _forgroundcolorTheme!
                                    }
                                );
                            }
                        }

                        icNetworkCardsList.ItemsSource = networkcards;

                    }));

                    Thread.Sleep(1000);
                }

                catch (TaskCanceledException)
                {

                }
                catch (Exception)
                {

                }
            }
        }
    }
}
