namespace eBRestarter.Classes.EBInterfaces
{
    public interface IAPIWebLinks
    {
        public static string Hourly_Earnings { get { return "https://www.ebesucher.de/api/visitor_exchange.json/account/earnings_hourly/" + DateTime.Now.ToString("dd/MM/y").Replace('.', '-') + "?timezone=Europe%2FBerlin"; } }

        public const string IPLINK = "https://www.ebesucher.de/api/ip.json/data";

        public const string EARNINGS_THIS_MONTH = "https://www.ebesucher.de/api/visitor_exchange.json/account/earnings/";
    }
}
