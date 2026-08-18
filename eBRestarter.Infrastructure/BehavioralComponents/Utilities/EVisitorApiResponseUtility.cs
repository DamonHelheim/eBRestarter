using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Text.Json;

using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Infrastructure.BehavioralComponents.Utilities;

/// <summary>
/// Parses the eBesucher API JSON payloads into strongly typed results.
/// </summary>
/// <remarks>
/// 📝 Logging Guidelines Section 2: This utility is static and cannot receive an <see cref="ILogger"/>
/// via constructor injection. Instead of falling back to <c>Debug.WriteLine</c> (which is stripped
/// in release builds), each method accepts the caller's <see cref="ILogger"/>. This ensures log entries
/// retain the calling adapter's category, attributing errors to the actual API operation rather than an auxiliary utility.
/// </remarks>
public static class EVisitorApiResponseUtility
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Types & Strings (alphabetical) ──
    private const string CountryCodePropertyName = "countryCode";
    private const string CountryNamePropertyName = "countryName";
    private const string DefaultFallbackString = "-";
    private const string FromW3CPropertyName = "from_w3c";
    private const string HostPropertyName = "host";
    private const int HoursInDayCount = 24;
    private const string IpPropertyName = "ip";
    private const int MonthsInYearCount = 12;
    private const string ValuePropertyName = "value";


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Parses daily earnings from eBesucher JSON response for a given month length.
    /// </summary>
    /// <param name="jsonContent">The raw JSON response string.</param>
    /// <param name="daysInMonth">The number of days in the target month.</param>
    /// <param name="logger">The caller's logger instance for telemetry and error tracking.</param>
    /// <returns>An array containing daily earnings values rounded to 2 decimal places.</returns>
    public static double[] ParseDailyEarnings(string jsonContent, int daysInMonth, ILogger logger)
    {
        // ⚠️ Exception Guidelines Section 9: Explicit argument validation prevents a null logger
        // from throwing a NullReferenceException in the catch block where it would mask the root cause.
        ArgumentNullException.ThrowIfNull(logger);
        var dailyEarnings = new double[daysInMonth];

        if (string.IsNullOrWhiteSpace(jsonContent) || daysInMonth <= 0)
        {
            return dailyEarnings;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(jsonContent);
            var root = jsonDocument.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var jsonElement in root.EnumerateArray())
                {
                    if (jsonElement.TryGetProperty(FromW3CPropertyName, out var dateProperty) &&
                        dateProperty.GetString() is { Length: > 0 } dateString &&
                        DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date) &&
                        date.Day - 1 is int dayIndex && dayIndex >= 0 && dayIndex < daysInMonth)
                    {
                        dailyEarnings[dayIndex] += RetrieveValueSafe(jsonElement);
                    }
                }
            }

            for (var index = 0; index < dailyEarnings.Length; index++)
            {
                dailyEarnings[index] = Math.Round(dailyEarnings[index], 2);
            }
        }
        catch (Exception exception)
        {
            // 🔒 Section 8.3: A parsing failure previously resulted in an indistinguishable 0.0 value.
            // Logging ensures payload anomalies are visible while returning safe fallback values.
            logger.LogWarning(
                LogEventIds.Api.ApiResponseParsingFailed,
                exception,
                "Parsing the eBesucher API response in {ParseOperation} failed; falling back to default values.",
                nameof(ParseDailyEarnings));
        }

        return dailyEarnings;
    }

    /// <summary>
    /// Parses 24-hour earnings from eBesucher JSON response supporting object and array payload formats.
    /// </summary>
    /// <param name="jsonContent">The raw JSON response string.</param>
    /// <param name="logger">The caller's logger instance for telemetry and error tracking.</param>
    /// <returns>A 24-element array containing hourly earnings values rounded to 2 decimal places.</returns>
    public static double[] ParseHourlyEarnings(string jsonContent, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        var hourly = new double[HoursInDayCount];

        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return hourly;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(jsonContent);
            var root = jsonDocument.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                ParseHourlyEarningsFromJsonObject(root, hourly);
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                ParseHourlyEarningsFromJsonArray(root, hourly);
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                LogEventIds.Api.ApiResponseParsingFailed,
                exception,
                "Parsing the eBesucher API response in {ParseOperation} failed; falling back to default values.",
                nameof(ParseHourlyEarnings));
        }

        return hourly;
    }

    /// <summary>
    /// Parses IP information payload into a strongly typed <see cref="IpInfoData"/> record.
    /// </summary>
    /// <param name="jsonContent">The raw JSON response string.</param>
    /// <param name="logger">The caller's logger instance for telemetry and error tracking.</param>
    /// <returns>The deserialized <see cref="IpInfoData"/> if valid; otherwise, <see langword="null"/>.</returns>
    public static IpInfoData? ParseIpInfo(string jsonContent, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return null;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(jsonContent);
            var root = jsonDocument.RootElement;

            return new IpInfoData(
                IpAddress: RetrieveStringSafe(root, IpPropertyName),
                Hostname: RetrieveStringSafe(root, HostPropertyName),
                CountryCode: RetrieveStringSafe(root, CountryCodePropertyName),
                CountryName: RetrieveStringSafe(root, CountryNamePropertyName));
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                LogEventIds.Api.ApiResponseParsingFailed,
                exception,
                "Parsing the eBesucher API response in {ParseOperation} failed; falling back to default values.",
                nameof(ParseIpInfo));

            return null;
        }
    }

    /// <summary>
    /// Parses 12-month earnings from eBesucher JSON response.
    /// </summary>
    /// <param name="jsonContent">The raw JSON response string.</param>
    /// <param name="logger">The caller's logger instance for telemetry and error tracking.</param>
    /// <returns>A 12-element array containing monthly earnings values rounded to 2 decimal places.</returns>
    public static double[] ParseMonthlyEarnings(string jsonContent, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        var monthlyEarnings = new double[MonthsInYearCount];

        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return monthlyEarnings;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(jsonContent);
            var root = jsonDocument.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var jsonElement in root.EnumerateArray())
                {
                    if (jsonElement.TryGetProperty(FromW3CPropertyName, out var dateProperty) &&
                        dateProperty.GetString() is { Length: > 0 } dateString &&
                        DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date) &&
                        date.Month - 1 is int monthIndex && monthIndex >= 0 && monthIndex < MonthsInYearCount)
                    {
                        monthlyEarnings[monthIndex] += RetrieveValueSafe(jsonElement);
                    }
                }
            }

            for (var index = 0; index < monthlyEarnings.Length; index++)
            {
                monthlyEarnings[index] = Math.Round(monthlyEarnings[index], 2);
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                LogEventIds.Api.ApiResponseParsingFailed,
                exception,
                "Parsing the eBesucher API response in {ParseOperation} failed; falling back to default values.",
                nameof(ParseMonthlyEarnings));
        }

        return monthlyEarnings;
    }

    /// <summary>
    /// Populates the hourly array from a JSON array structure.
    /// </summary>
    private static void ParseHourlyEarningsFromJsonArray(JsonElement root, double[] hourly)
    {
        var index = 0;

        foreach (var element in root.EnumerateArray())
        {
            if (index >= HoursInDayCount)
            {
                break;
            }

            hourly[index] = element.TryGetDouble(out var value) ? Math.Round(value, 2) : 0.0;
            index++;
        }
    }

    /// <summary>
    /// Populates the hourly array from a JSON object structure where property names represent 1-based hours.
    /// </summary>
    private static void ParseHourlyEarningsFromJsonObject(JsonElement root, double[] hourly)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (int.TryParse(property.Name, out var hour) && hour is >= 1 and <= HoursInDayCount)
            {
                hourly[hour - 1] = property.Value.TryGetDouble(out var value) ? Math.Round(value, 2) : 0.0;
            }
        }
    }

    /// <summary>
    /// Safely extracts a string property value, returning a fallback string if absent or empty.
    /// </summary>
    private static string RetrieveStringSafe(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
        {
            return property.GetString() ?? DefaultFallbackString;
        }

        return DefaultFallbackString;
    }

    /// <summary>
    /// Safely extracts a numeric double value from a JSON element handling both number and formatted string values.
    /// </summary>
    private static double RetrieveValueSafe(JsonElement item)
    {
        if (item.TryGetProperty(ValuePropertyName, out var valueProperty))
        {
            if (valueProperty.ValueKind == JsonValueKind.Number)
            {
                return valueProperty.GetDouble();
            }

            if (valueProperty.ValueKind == JsonValueKind.String && valueProperty.GetString() is { Length: > 0 } valueString)
            {
                if (double.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue))
                {
                    return parsedValue;
                }

                if (double.TryParse(valueString.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValueReplaced))
                {
                    return parsedValueReplaced;
                }
            }
        }

        return 0.0;
    }
}
