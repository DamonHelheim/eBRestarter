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
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.API;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for retrieving and managing eBesucher API account data and earnings.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Handles external API communication with eBesucher services via HTTP REST in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortEVisitorApiProvider"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterEVisitorApiProvider : IOutboundPortEVisitorApiProvider
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
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
    // ── Block 1: Injected dependencies ──
    private readonly IOutboundPortEVisitorConfigRepository _configPort;
    private readonly ILogger<AdapterEVisitorApiProvider> _logger;
    private readonly IRestClient _restClientPort;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes a new instance of <see cref="AdapterEVisitorApiProvider"/>.
    /// </summary>
    /// <param name="configPort">Config repository port.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="restClientPort">REST client port.</param>
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
    /// <summary>
    /// Retrieves combined hourly, daily, and monthly earnings data for the configured account.
    /// </summary>
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
            _logger.LogError(LogEventIds.Api.EarningsRetrievalFailed, exception, ErrorRetrievingEarningsLogMessage);
            return null;
        }
    }

    /// <summary>
    /// Retrieves IP info and surfbar status from the eBesucher API.
    /// </summary>
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
            return EVisitorApiResponseUtility.ParseIpInfo(response.Content, _logger);
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.Api.ApiResponseParsingFailed, exception, ErrorParsingIpDataLogMessage);
            return null;
        }
    }

    /// <summary>
    /// Fetches raw daily earnings array for the specified date range.
    /// </summary>
    /// <param name="username">Account username.</param>
    /// <param name="apiKey">API key.</param>
    /// <param name="start">Start date.</param>
    /// <param name="end">End date.</param>
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
            return EVisitorApiResponseUtility.ParseDailyEarnings(response.Content, daysInMonth, _logger);
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.Api.ApiResponseParsingFailed, exception, ErrorParsingDailyEarningsLogMessage);
            return new double[daysInMonth];
        }
    }

    /// <summary>
    /// Fetches raw hourly earnings array for the current day.
    /// </summary>
    /// <param name="username">Account username.</param>
    /// <param name="apiKey">API key.</param>
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
            return EVisitorApiResponseUtility.ParseHourlyEarnings(response.Content, _logger);
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.Api.ApiResponseParsingFailed, exception, ErrorParsingHourlyEarningsLogMessage);
            return new double[HoursInDayCount];
        }
    }

    /// <summary>
    /// Fetches raw monthly earnings array for the specified date range.
    /// </summary>
    /// <param name="username">Account username.</param>
    /// <param name="apiKey">API key.</param>
    /// <param name="start">Start date.</param>
    /// <param name="end">End date.</param>
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
            return EVisitorApiResponseUtility.ParseMonthlyEarnings(response.Content, _logger);
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.Api.ApiResponseParsingFailed, exception, ErrorParsingMonthlyEarningsLogMessage);
            return new double[MonthsInYearCount];
        }
    }
}
