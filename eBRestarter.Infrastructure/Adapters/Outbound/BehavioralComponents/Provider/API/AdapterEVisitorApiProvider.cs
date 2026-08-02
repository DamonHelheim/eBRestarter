using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Infrastructure.Api;
using eBRestarter.Infrastructure.Api.Interfaces;
using eBRestarter.Infrastructure.BehavioralComponents.Utilities;
using eBRestarter.Infrastructure.ObjectArchetypes.Model;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.API;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for retrieving and managing eBesucher API account data and earnings.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Dienstleister im äußeren Ring (Infrastructure Layer) die externe API-Kommunikation mit dem eBesucher-Dienst via HTTP-REST.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortEVisitorApiProvider"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong> (Taxonomie: Provider Adapter), da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um technologische Netzwerk-Seiteneffekte und externe API-Abfragen auszuführen.
/// </para>
/// </summary>
public sealed class AdapterEVisitorApiProvider : IOutboundPortEVisitorApiProvider
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    private const string ErrorParsingDailyEarningsLogMessage = "Error parsing daily earnings.";
    private const string ErrorParsingHourlyEarningsLogMessage = "Error parsing hourly earnings.";
    private const string ErrorParsingIpDataLogMessage = "Error parsing IP data.";
    private const string ErrorParsingMonthlyEarningsLogMessage = "Error parsing monthly earnings.";
    private const string ErrorRetrievingEarningsLogMessage = "Error retrieving earnings (overall process).";
    private const int HoursInDayCount = 24;
    private const int MonthsInYearCount = 12;

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (Dependencies) ──
    private readonly IOutboundPortEVisitorConfigRepository _configPort;
    private readonly ILogger<AdapterEVisitorApiProvider> _logger;
    private readonly IRestClient _restClientPort;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    public AdapterEVisitorApiProvider(
        IOutboundPortEVisitorConfigRepository configPort,
        ILogger<AdapterEVisitorApiProvider> logger,
        IRestClient restClientPort)
    {
        ArgumentNullException.ThrowIfNull(configPort);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(restClientPort);

        _configPort = configPort;
        _logger = logger;
        _restClientPort = restClientPort;
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
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

            await Task.WhenAll(tHourly, tMonthlyDetails, tYearlyDetails).ConfigureAwait(false);
            var hourlyArray = await tHourly.ConfigureAwait(false);
            var dailyArray = await tMonthlyDetails.ConfigureAwait(false);
            var monthlyArray = await tYearlyDetails.ConfigureAwait(false);

            var monthlySum = dailyArray.Sum();
            var yearlySum = monthlyArray.Sum();
            var todaySum = hourlyArray.Sum();

            return new EarningsData(hourlyArray, dailyArray, monthlyArray, yearlySum, monthlySum, todaySum);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorRetrievingEarningsLogMessage);
            return null;
        }
    }

    public async Task<IpInfoData?> RetrieveIpInfoAsync()
    {
        var request = new ApiRequest
        {
            Url = ApiWebLinks.IpLink,
            Method = ObjectArchetypes.Enums.HttpMethod.GET
        };

        var response = await _restClientPort.ExecuteGetAsync(request).ConfigureAwait(false);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
        {
            return null;
        }

        try
        {
            return EVisitorApiResponseUtility.ParseIpInfo(response.Content);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorParsingIpDataLogMessage);
            return null;
        }
    }

    private async Task<double[]> RetrieveDailyEarningsRawAsync(
        string username,
        string apiKey,
        DateTime start,
        DateTime end)
    {
        var unixStart = ((DateTimeOffset)start).ToUnixTimeSeconds();
        var unixEnd = ((DateTimeOffset)end).ToUnixTimeSeconds();

        string url = $"{ApiWebLinks.EarningsThisMonth}{unixStart}-{unixEnd}";

        var request = new ApiRequest
        {
            Url = url,
            Method = ObjectArchetypes.Enums.HttpMethod.GET,
            Username = username,
            Password = apiKey
        };

        var daysInMonth = DateTime.DaysInMonth(start.Year, start.Month);

        var response = await _restClientPort.ExecuteGetAsync(request).ConfigureAwait(false);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
        {
            return new double[daysInMonth];
        }

        try
        {
            return EVisitorApiResponseUtility.ParseDailyEarnings(response.Content, daysInMonth);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorParsingDailyEarningsLogMessage);
            return new double[daysInMonth];
        }
    }

    private async Task<double[]> RetrieveHourlyEarningsRawAsync(string username, string apiKey)
    {
        var request = new ApiRequest
        {
            Url = ApiWebLinks.HourlyEarnings,
            Method = ObjectArchetypes.Enums.HttpMethod.GET,
            Username = username,
            Password = apiKey
        };

        var response = await _restClientPort.ExecuteGetAsync(request).ConfigureAwait(false);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
        {
            return new double[HoursInDayCount];
        }

        try
        {
            return EVisitorApiResponseUtility.ParseHourlyEarnings(response.Content);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorParsingHourlyEarningsLogMessage);
            return new double[HoursInDayCount];
        }
    }

    private async Task<double[]> RetrieveMonthlyEarningsRawAsync(
        string username,
        string apiKey,
        DateTime start,
        DateTime end)
    {
        var unixStart = ((DateTimeOffset)start).ToUnixTimeSeconds();
        var unixEnd = ((DateTimeOffset)end).ToUnixTimeSeconds();

        string url = $"{ApiWebLinks.EarningsThisMonth}{unixStart}-{unixEnd}";

        var request = new ApiRequest
        {
            Url = url,
            Method = ObjectArchetypes.Enums.HttpMethod.GET,
            Username = username,
            Password = apiKey
        };

        var response = await _restClientPort.ExecuteGetAsync(request).ConfigureAwait(false);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
        {
            return new double[MonthsInYearCount];
        }

        try
        {
            return EVisitorApiResponseUtility.ParseMonthlyEarnings(response.Content);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorParsingMonthlyEarningsLogMessage);
            return new double[MonthsInYearCount];
        }
    }
}
