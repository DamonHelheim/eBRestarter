using System.Globalization;

namespace eBRestarter.Infrastructure.Api;

/// <summary>
/// Provides static endpoint URLs and URL builders for the eBesucher REST API.
/// </summary>
public static class ApiWebLinks
{
    /// <summary>
    /// Endpoint URL for querying client IP and connectivity data.
    /// </summary>
    public const string IpLink = "https://www.ebesucher.de/api/ip.json/data";

    /// <summary>
    /// Endpoint URL for querying account earnings for the current month.
    /// </summary>
    public const string EarningsThisMonth = "https://www.ebesucher.de/api/visitor_exchange.json/account/earnings/";

    /// <summary>
    /// Gets the endpoint URL for querying hourly earnings for the current date in the Europe/Berlin timezone.
    /// </summary>
    public static string HourlyEarnings
    {
        get
        {
            // Formats date as "yyyy-MM-dd" (e.g. "2026-04-13") invariant of current culture for the API endpoint.
            var dateStr = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            return $"https://www.ebesucher.de/api/visitor_exchange.json/account/earnings_hourly/{dateStr}?timezone=Europe%2FBerlin";
        }
    }
}

