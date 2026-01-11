using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Infrastructure.Constants
{
    public static class ApiWebLinks
    {
        // Dynamische Property, da sich das Datum bei jedem Aufruf ändern kann
        public static string HourlyEarnings
        {
            get
            {
                // Erzeugt direkt das Format dd-MM-yyyy (statt Replace zu nutzen)
                string dateStr = DateTime.Now.ToString("dd-MM-yyyy");
                return $"https://www.ebesucher.de/api/visitor_exchange.json/account/earnings_hourly/{dateStr}?timezone=Europe%2FBerlin";
            }
        }

        public const string IpLink = "https://www.ebesucher.de/api/ip.json/data";

        public const string EarningsThisMonth = "https://www.ebesucher.de/api/visitor_exchange.json/account/earnings/";
    }
}
