namespace eBRestarter.Model.Models
{
    public struct NetWorkCardAndTrafficInfo
    {
        private string _imagePathNetworkCard;
        private string _imagePathReceivedData;
        private string _imagePathSendData;

        private string _adapterName;
        private string _receivedData;
        private string _sendData;

        private string _foregroundcolor;

        public string ImagePathNetworkCard { readonly get => _imagePathNetworkCard; set => _imagePathNetworkCard = value; }
        public string ImagePathReceivedData { readonly get => _imagePathReceivedData; set => _imagePathReceivedData = value; }
        public string ImagePathSendData { readonly get => _imagePathSendData; set => _imagePathSendData = value; }
        public string AdapterName { readonly get => _adapterName; set => _adapterName = value; }
        public string ReceivedData { readonly get => _receivedData; set => _receivedData = value; }
        public string SendData { readonly get => _sendData; set => _sendData = value; }
        public string Foregroundcolor { readonly get => _foregroundcolor; set => _foregroundcolor = value; }

    }
}
