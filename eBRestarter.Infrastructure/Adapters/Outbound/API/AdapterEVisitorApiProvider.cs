using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Outbound;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Infrastructure.Api;
using eBRestarter.Infrastructure.Api.Interfaces;
using eBRestarter.Infrastructure.Models.Network;
using eBRestarter.Infrastructure.Utilities;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters.Outbound.API;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for retrieving and managing eBesucher API account data and earnings.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Dienstleister im äußeren Ring (Infrastructure Layer) die externe API-Kommunikation mit dem eBesucher-Dienst via HTTP-REST.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortEVisitorApiProvider"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong> (Taxonomie: Provider Adapter), da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um technologische Netzwerk-Seiteneffekte und externe API-Abfragen auszuführen.
/// </para>
/// </summary>
public sealed class AdapterEVisitorApiProvider(
    IRestClient restClientPort,
    IOutboundPortEVisitorConfigRepository configPort,
    // removed parser
    ILogger<AdapterEVisitorApiProvider> logger) : IOutboundPortEVisitorApiProvider
{
    private readonly IRestClient _restClientPort = restClientPort;
    private readonly IOutboundPortEVisitorConfigRepository _configPort = configPort;

    private readonly ILogger<AdapterEVisitorApiProvider> _logger = logger;

    public async Task<IpInfoData?> RetrieveIpInfoAsync()
    {
        var request = new ApiRequest
        {
            Url = ApiWebLinks.IpLink,
            Method = Enums.HttpMethod.GET
        };

        var response = await _restClientPort.ExecuteGetAsync(request);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
            return null;

        try
        {
            return EVisitorApiResponseUtility.ParseIpInfo(response.Content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing IP data.");
            return null;
        }
    }

    public async Task<EarningsData?> RetrieveEarningsAsync()
    {
        var config = _configPort.LoadConfig();

        if (string.IsNullOrWhiteSpace(config.Settings.ApiUsername)
            || string.IsNullOrWhiteSpace(config.Settings.ApiKey))
        {
            return null;
        }

        var username = config.Settings.ApiUsername;
        var apiKey = config.Settings.ApiKey;
        var now = DateTime.Now;

        // 1. Month: First day of the current month 00:00:00 to now
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Local);

        // 2. Year: January 1st of the current year 00:00:00 to now
        var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Local);

        try
        {
            var tHourly = RetrieveHourlyEarningsRawAsync(username, apiKey);

            // Details (Array) for the current month (Days 1-31)
            var tMonthlyDetails = RetrieveDailyEarningsRawAsync(username, apiKey, startOfMonth, now);

            // Details (Array) for the current year (Months Jan-Dec)
            var tYearlyDetails = RetrieveMonthlyEarningsRawAsync(username, apiKey, startOfYear, now);

            await Task.WhenAll(tHourly, tMonthlyDetails, tYearlyDetails);
            var hourlyArray = tHourly.Result;      // 24h (Today)
            var dailyArray = tMonthlyDetails.Result; // Days of the month
            var monthlyArray = tYearlyDetails.Result; // 12 Months of the year

            var monthlySum = dailyArray.Sum();
            var yearlySum = monthlyArray.Sum();
            var todaySum = hourlyArray.Sum();

            // Extend your EarningsData record with the new arrays if you want to use charts!
            // Currently matches your record schema (sums):
            return new EarningsData(hourlyArray, dailyArray, monthlyArray, yearlySum, monthlySum, todaySum);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving earnings (overall process).");
            return null;
        }
    }

    private async Task<double[]> RetrieveHourlyEarningsRawAsync(string username, string apiKey)
    {
        var request = new ApiRequest
        {
            Url = ApiWebLinks.HourlyEarnings,
            Method = Enums.HttpMethod.GET,
            Username = username,
            Password = apiKey
        };

        var response = await _restClientPort.ExecuteGetAsync(request);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
            return new double[24];

        try
        {
            return EVisitorApiResponseUtility.ParseHourlyEarnings(response.Content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing hourly earnings.");
            return new double[24];
        }
    }

    private async Task<double[]> RetrieveDailyEarningsRawAsync(string username, string apiKey, DateTime start, DateTime end)
    {
        var unixStart = ((DateTimeOffset)start).ToUnixTimeSeconds();
        var unixEnd = ((DateTimeOffset)end).ToUnixTimeSeconds();

        string url = $"{ApiWebLinks.EarningsThisMonth}{unixStart}-{unixEnd}";

        var request = new ApiRequest
        {
            Url = url,
            Method = Enums.HttpMethod.GET,
            Username = username,
            Password = apiKey
        };

        var daysInMonth = DateTime.DaysInMonth(start.Year, start.Month);

        var response = await _restClientPort.ExecuteGetAsync(request);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
            return new double[daysInMonth];

        try
        {
            return EVisitorApiResponseUtility.ParseDailyEarnings(response.Content, daysInMonth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing daily earnings.");
            return new double[daysInMonth];
        }
    }

    private async Task<double[]> RetrieveMonthlyEarningsRawAsync(string username, string apiKey, DateTime start, DateTime end)
    {
        var unixStart = ((DateTimeOffset)start).ToUnixTimeSeconds();
        var unixEnd = ((DateTimeOffset)end).ToUnixTimeSeconds();

        string url = $"{ApiWebLinks.EarningsThisMonth}{unixStart}-{unixEnd}";

        var request = new ApiRequest
        {
            Url = url,
            Method = Enums.HttpMethod.GET,
            Username = username,
            Password = apiKey
        };

        var response = await _restClientPort.ExecuteGetAsync(request);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
            return new double[12];

        try
        {
            return EVisitorApiResponseUtility.ParseMonthlyEarnings(response.Content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing monthly earnings.");
            return new double[12];
        }
    }
}









