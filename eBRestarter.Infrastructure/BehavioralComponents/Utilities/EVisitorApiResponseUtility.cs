using System;
using System.Globalization;
using System.Text.Json;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Infrastructure.BehavioralComponents.Utilities;

public static class EVisitorApiResponseUtility
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
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

    public static double[] ParseDailyEarnings(string jsonContent, int daysInMonth)
    {
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
        catch (JsonException)
        {
            // Failures are tolerated; default array returned
        }
        catch (Exception)
        {
            // Failures are tolerated; default array returned
        }

        return dailyEarnings;
    }

    public static double[] ParseHourlyEarnings(string jsonContent)
    {
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
        catch (JsonException)
        {
            // Failures are tolerated; an empty array is returned
        }
        catch (Exception)
        {
            // Failures are tolerated
        }

        return hourly;
    }

    public static IpInfoData? ParseIpInfo(string jsonContent)
    {
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
        catch (JsonException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static double[] ParseMonthlyEarnings(string jsonContent)
    {
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
        catch (JsonException)
        {
            // Failures are tolerated
        }
        catch (Exception)
        {
            // Failures are tolerated
        }

        return monthlyEarnings;
    }

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

    private static string RetrieveStringSafe(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
        {
            return property.GetString() ?? DefaultFallbackString;
        }

        return DefaultFallbackString;
    }

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
