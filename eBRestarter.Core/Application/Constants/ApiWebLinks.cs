using System.Globalization;

namespace eBRestarter.Core.Application.Constants;

public static class ApiWebLinks
{
    public static string HourlyEarnings
    {
        get
        {
            // Baut exakt "yyyy-MM-dd" (z.B. "2026-04-13") absolut unbeeindruckt von der eingestellten Sprache.
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            return $"https://www.ebesucher.de/api/visitor_exchange.json/account/earnings_hourly/{dateStr}?timezone=Europe%2FBerlin";
        }
    }

    public const string IpLink = "https://www.ebesucher.de/api/ip.json/data";

    public const string EarningsThisMonth = "https://www.ebesucher.de/api/visitor_exchange.json/account/earnings/";
}
