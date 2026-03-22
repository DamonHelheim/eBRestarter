using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.RestClient;
using eBRestarter.Core.Domain.Models;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Infrastructure.Constants;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace eBRestarter.Infrastructure.Services;

public class EVisitorApiAdapter(
    IRestClientService restClient,
    IEVisitorConfigService configService,
    ILogger<EVisitorApiAdapter> logger) : IEVisitorApiService
{
    private readonly IRestClientService _restClient = restClient;
    private readonly IEVisitorConfigService _configService = configService;
    private readonly ILogger<EVisitorApiAdapter> _logger = logger;

    public async Task<IpInfoData?> GetIpInfoAsync()
    {
        var request = new ApiRequest
        {
            Url = ApiWebLinks.IpLink,
            Method = Core.Domain.Enums.HttpMethod.GET
        };

        var response = await _restClient.ExecuteGetAsync(request);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(response.Content);
            var root = doc.RootElement;

            return new IpInfoData(
                IpAddress: GetStringSafe(root, "ip"),
                Hostname: GetStringSafe(root, "host"),
                CountryCode: GetStringSafe(root, "countryCode"),
                CountryName: GetStringSafe(root, "countryName")
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Parsen der IP Daten");
            return null;
        }
    }

    public async Task<EarningsData?> GetEarningsAsync()
    {
        var config = _configService.LoadConfig();

        if (string.IsNullOrWhiteSpace(config.Settings.ApiUsername) ||
            string.IsNullOrWhiteSpace(config.Settings.ApiKey))
        {
            return null;
        }

        var username = config.Settings.ApiUsername;
        var apiKey = config.Settings.ApiKey;
        var now = DateTime.Now;

        // --- ZEITRÄUME BERECHNEN ---
        // 1. Monat: Erster des aktuellen Monats 00:00:00 bis Jetzt
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0);

        // 2. Jahr: Erster Januar des aktuellen Jahres 00:00:00 bis Jetzt
        var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0);

        try
        {
            // --- PARALLELE API AUFRUFE (Performance!) ---
            var tHourly = GetHourlyEarningsRawAsync(username, apiKey);

            // Details (Array) für den aktuellen Monat (Tage 1-31)
            var tMonthlyDetails = GetDailyEarningsRawAsync(username, apiKey, startOfMonth, now);

            // Details (Array) für das aktuelle Jahr (Monate Jan-Dez)
            var tYearlyDetails = GetMonthlyEarningsRawAsync(username, apiKey, startOfYear, now);

            await Task.WhenAll(tHourly, tMonthlyDetails, tYearlyDetails);

            // --- ERGEBNISSE ZUSAMMENSETZEN ---
            double[] hourlyArray = tHourly.Result;      // 24h (Heute)
            double[] dailyArray = tMonthlyDetails.Result; // Tage des Monats
            double[] monthlyArray = tYearlyDetails.Result; // 12 Monate des Jahres

            double monthlySum = dailyArray.Sum();
            double yearlySum = monthlyArray.Sum();
            double todaySum = hourlyArray.Sum();

            // Erweitere dein EarningsData Record um die neuen Arrays, wenn du Charts willst!
            // return new EarningsData(hourlyArray, dailyArray, monthlyArray, yearlySum, monthlySum, todaySum);

            // Aktuell passend zu deinem Record (Summen):
            return new EarningsData(hourlyArray, dailyArray, monthlyArray , yearlySum, monthlySum, todaySum);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Earnings (Gesamtprozess).");
            return null;
        }
    }

    // --- INTERNE HELPER METHODEN ---

    private async Task<double[]> GetHourlyEarningsRawAsync(string username, string apiKey)
    {
        var request = new ApiRequest
        {
            Url = ApiWebLinks.HourlyEarnings,
            Method = Core.Domain.Enums.HttpMethod.GET,
            Username = username,
            Password = apiKey
        };

        var response = await _restClient.ExecuteGetAsync(request);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
            return new double[24];

        try
        {
            using var doc = JsonDocument.Parse(response.Content);
            var root = doc.RootElement;

            // Unser Ziel-Array immer direkt mit 24 Feldern initialisieren
            double[] hourly = new double[24];

            // NEU: Verarbeitung als JSON-Objekt (z.B. {"1": 685.3, "2": 507.7})
            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in root.EnumerateObject())
                {
                    // String-Key (z.B. "1") in eine Zahl parsen
                    if (int.TryParse(property.Name, out int hour) && hour >= 1 && hour <= 24)
                    {
                        // Wert abgreifen und auf den korrekten Array-Index (Stunde - 1) legen
                        hourly[hour - 1] = property.Value.TryGetDouble(out double val) ? Math.Round(val, 2) : 0.0;
                    }
                }
            }
            // FALLBACK: Falls die API doch mal ein echtes Array sendet [685.3, 507.7]
            else if (root.ValueKind == JsonValueKind.Array)
            {
                var parsedArray = root.EnumerateArray()
                                      .Select(element => element.TryGetDouble(out double val) ? Math.Round(val, 2) : 0.0)
                                      .ToArray();

                // Nur maximal 24 Werte rüberkopieren, um Exceptions zu vermeiden
                Array.Copy(parsedArray, hourly, Math.Min(parsedArray.Length, 24));
            }

            return hourly;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Parsen der Hourly Earnings.");
            return new double[24];
        }
    }

    private async Task<double[]> GetDailyEarningsRawAsync(string username, string apiKey, DateTime start, DateTime end)
    {
        long unixStart = ((DateTimeOffset)start).ToUnixTimeSeconds();
        long unixEnd = ((DateTimeOffset)end).ToUnixTimeSeconds();

        string url = $"{ApiWebLinks.EarningsThisMonth}{unixStart}-{unixEnd}";

        var request = new ApiRequest
        {
            Url = url,
            Method = Core.Domain.Enums.HttpMethod.GET,
            Username = username,
            Password = apiKey
        };

        int daysInMonth = DateTime.DaysInMonth(start.Year, start.Month);
        var dailyEarnings = new double[daysInMonth];

        var response = await _restClient.ExecuteGetAsync(request);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
            return dailyEarnings;

        try
        {
            using var doc = JsonDocument.Parse(response.Content);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    if (item.TryGetProperty("from_w3c", out JsonElement dateProp))
                    {
                        string dateStr = dateProp.GetString() ?? "";

                        // LÖSUNG 1: Normales TryParse kann das W3C-Format (ISO 8601) nativ verarbeiten!
                        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime date))
                        {
                            int dayIndex = date.Day - 1;
                            if (dayIndex >= 0 && dayIndex < daysInMonth)
                            {
                                // LÖSUNG 2: Unbedingt += nutzen, um alle Einträge des Tages aufzusummieren!
                                dailyEarnings[dayIndex] += GetValueSafe(item);
                            }
                        }
                    }
                }
            }

            // Auf 2 Nachkommastellen runden, um Floating-Point-Artefakte zu vermeiden
            for (int i = 0; i < dailyEarnings.Length; i++)
            {
                dailyEarnings[i] = Math.Round(dailyEarnings[i], 2);
            }

            return dailyEarnings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Parsen der Daily Earnings.");
            return dailyEarnings;
        }
    }

    // NEU: Jahresübersicht (12 Monate)
    private async Task<double[]> GetMonthlyEarningsRawAsync(string username, string apiKey, DateTime start, DateTime end)
    {
        long unixStart = ((DateTimeOffset)start).ToUnixTimeSeconds();
        long unixEnd = ((DateTimeOffset)end).ToUnixTimeSeconds();

        string url = $"{ApiWebLinks.EarningsThisMonth}{unixStart}-{unixEnd}";

        var request = new ApiRequest
        {
            Url = url,
            Method = Core.Domain.Enums.HttpMethod.GET,
            Username = username,
            Password = apiKey
        };

        var monthlyEarnings = new double[12]; // Jan=0, Dez=11

        var response = await _restClient.ExecuteGetAsync(request);

        if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
            return monthlyEarnings;

        try
        {
            using var doc = JsonDocument.Parse(response.Content);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    if (item.TryGetProperty("from_w3c", out JsonElement dateProp))
                    {
                        string dateStr = dateProp.GetString() ?? "";

                        // LÖSUNG: Flexibles TryParse nutzen, um das ISO/W3C Format ("2026-03-14T20:00:00+00:00") zu verstehen
                        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime date))
                        {
                            int monthIndex = date.Month - 1; // 0-11
                            if (monthIndex >= 0 && monthIndex < 12)
                            {
                                // PERFEKT: Das Aufaddieren hast du hier schon richtig implementiert!
                                monthlyEarnings[monthIndex] += GetValueSafe(item);
                            }
                        }
                    }
                }
            }

            // Auf 2 Nachkommastellen runden, um Floating-Point-Artefakte zu vermeiden
            for (int i = 0; i < monthlyEarnings.Length; i++)
            {
                monthlyEarnings[i] = Math.Round(monthlyEarnings[i], 2);
            }

            return monthlyEarnings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Parsen der Monthly Earnings.");
            return monthlyEarnings;
        }
    }

    // Helper um Value als Number oder String zu lesen
    private double GetValueSafe(JsonElement item)
    {
        if (item.TryGetProperty("value", out JsonElement valProp))
        {
            if (valProp.ValueKind == JsonValueKind.Number)
            {
                return valProp.GetDouble();
            }
            else if (valProp.ValueKind == JsonValueKind.String &&
                     double.TryParse(valProp.GetString()?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double dVal))
            {
                return dVal;
            }
        }
        return 0.0;
    }

    private string GetStringSafe(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement prop) &&
            prop.ValueKind == JsonValueKind.String)
        {
            return prop.GetString() ?? "-";
        }
        return "-";
    }
}