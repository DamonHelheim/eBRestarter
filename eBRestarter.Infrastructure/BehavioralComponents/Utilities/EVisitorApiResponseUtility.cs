using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using System.Globalization;
using System.Text.Json;

namespace eBRestarter.Infrastructure.BehavioralComponents.Utilities;

public static class EVisitorApiResponseUtility
{
    public static IpInfoData? ParseIpInfo(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            return new IpInfoData(
                IpAddress: RetrieveStringSafe(root, "ip"),
                Hostname: RetrieveStringSafe(root, "host"),
                CountryCode: RetrieveStringSafe(root, "countryCode"),
                CountryName: RetrieveStringSafe(root, "countryName")
            );
        }
        catch
        {
            return null;
        }
    }

    public static double[] ParseHourlyEarnings(string jsonContent)
    {
        double[] hourly = new double[24];

        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return hourly;
        }

        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                ParseHourlyEarningsFromJsonObject(root, hourly);
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                ParseHourlyEarningsFromJsonArray(root, hourly);
            }
        }
        catch
        {
            // Failures are tolerated; an empty array is returned
        }

        return hourly;
    }

    public static double[] ParseDailyEarnings(string jsonContent, int daysInMonth)
    {
        double[] dailyEarnings = new double[daysInMonth];

        if (string.IsNullOrWhiteSpace(jsonContent))
            return dailyEarnings;

        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    if (item.TryGetProperty("from_w3c", out JsonElement dateProp))
                    {
                        string dateStr = dateProp.GetString() ?? "";

                        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime date))
                        {
                            int dayIndex = date.Day - 1;
                            if (dayIndex >= 0 && dayIndex < daysInMonth)
                            {
                                dailyEarnings[dayIndex] += RetrieveValueSafe(item);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < dailyEarnings.Length; i++)
            {
                dailyEarnings[i] = Math.Round(dailyEarnings[i], 2);
            }
        }
        catch
        {
            // Failures are tolerated
        }

        return dailyEarnings;
    }

    public static double[] ParseMonthlyEarnings(string jsonContent)
    {
        double[] monthlyEarnings = new double[12]; // Jan=0, Dec=11

        if (string.IsNullOrWhiteSpace(jsonContent))
            return monthlyEarnings;

        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    if (item.TryGetProperty("from_w3c", out JsonElement dateProp))
                    {
                        string dateStr = dateProp.GetString() ?? "";

                        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime date))
                        {
                            int monthIndex = date.Month - 1;
                            if (monthIndex >= 0 && monthIndex < 12)
                            {
                                monthlyEarnings[monthIndex] += RetrieveValueSafe(item);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < monthlyEarnings.Length; i++)
            {
                monthlyEarnings[i] = Math.Round(monthlyEarnings[i], 2);
            }
        }
        catch
        {
            // Failures are tolerated
        }

        return monthlyEarnings;
    }

    private static void ParseHourlyEarningsFromJsonObject(JsonElement root, double[] hourly)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (int.TryParse(property.Name, out int hour) && hour >= 1 && hour <= 24)
            {
                hourly[hour - 1] = property.Value.TryGetDouble(out double val) ? Math.Round(val, 2) : 0.0;
            }
        }
    }

    private static void ParseHourlyEarningsFromJsonArray(JsonElement root, double[] hourly)
    {
        var parsedArray = root.EnumerateArray()
            .Select(x => x.TryGetDouble(out double val) ? Math.Round(val, 2) : 0.0)
            .ToArray();

        Array.Copy(parsedArray, hourly, Math.Min(parsedArray.Length, 24));
    }

    private static double RetrieveValueSafe(JsonElement item)
    {
        if (item.TryGetProperty("value", out JsonElement valProp))
        {
            if (valProp.ValueKind == JsonValueKind.Number)
            {
                return valProp.GetDouble();
            }
            else if (valProp.ValueKind == JsonValueKind.String
                      && double.TryParse(valProp.GetString()?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double dVal))
            {
                return dVal;
            }
        }
        return 0.0;
    }

    private static string RetrieveStringSafe(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement prop)
            && prop.ValueKind == JsonValueKind.String)
        {
            return prop.GetString() ?? "-";
        }
        return "-";
    }
}

