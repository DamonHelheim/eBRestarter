using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Parsers;
using eBRestarter.Core.Application.Ports.Outbound;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Infrastructure.Api;
using eBRestarter.Infrastructure.Network;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters;

public sealed class EVisitorApiProviderAdapter(
    IRestClientPort restClientUseCase,
    IEVisitorConfigPort configService,
    IEVisitorApiResponseParser parser,
    ILogger<EVisitorApiProviderAdapter> logger) : IEVisitorApiProviderPort
{
    private readonly IRestClientPort _restClientUseCase = restClientUseCase;
    private readonly IEVisitorConfigPort _configService = configService;
    private readonly IEVisitorApiResponseParser _parser = parser;
    private readonly ILogger<EVisitorApiProviderAdapter> _logger = logger;

    public async Task<IpInfoData?> RetrieveIpInfoAsync()
    {
        var request = new ApiRequest
        {
            Url = ApiWebLinks.IpLink,
            Method = eBRestarter.Infrastructure.Network.HttpMethod.GET
        };

        var response = await _restClientUseCase.ExecuteGetAsync(request);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
            return null;

        try
        {
            return _parser.ParseIpInfo(response.Content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing IP data.");
            return null;
        }
    }

    public async Task<EarningsData?> RetrieveEarningsAsync()
    {
        var config = _configService.LoadConfig();

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
            Method = eBRestarter.Infrastructure.Network.HttpMethod.GET,
            Username = username,
            Password = apiKey
        };

        var response = await _restClientUseCase.ExecuteGetAsync(request);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
            return new double[24];

        try
        {
            return _parser.ParseHourlyEarnings(response.Content);
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
            Method = eBRestarter.Infrastructure.Network.HttpMethod.GET,
            Username = username,
            Password = apiKey
        };

        var daysInMonth = DateTime.DaysInMonth(start.Year, start.Month);

        var response = await _restClientUseCase.ExecuteGetAsync(request);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
            return new double[daysInMonth];

        try
        {
            return _parser.ParseDailyEarnings(response.Content, daysInMonth);
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
            Method = eBRestarter.Infrastructure.Network.HttpMethod.GET,
            Username = username,
            Password = apiKey
        };

        var response = await _restClientUseCase.ExecuteGetAsync(request);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
            return new double[12];

        try
        {
            return _parser.ParseMonthlyEarnings(response.Content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing monthly earnings.");
            return new double[12];
        }
    }
}









