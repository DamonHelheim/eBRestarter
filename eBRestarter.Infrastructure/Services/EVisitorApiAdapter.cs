using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.RestClient;
using eBRestarter.Core.Domain.Models;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Infrastructure.Constants;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace eBRestarter.Infrastructure.Services.EBesucher
{
    public class EVisitorApiAdapter : IEVisitorApiService
    {
        private readonly IRestClientService _restClient;
        private readonly IEVisitorConfigService _configService;
        private readonly ILogger<EVisitorApiAdapter> _logger;

        public EVisitorApiAdapter(
            IRestClientService restClient,
            IEVisitorConfigService configService,
            ILogger<EVisitorApiAdapter> logger)
        {
            _restClient = restClient;
            _configService = configService;
            _logger = logger;
        }

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

                double[] hourly;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    hourly = root.EnumerateArray()
                                 .Select(element => element.TryGetDouble(out double val) ? val : 0.0)
                                 .ToArray();
                }
                else
                {
                    hourly = new double[24];
                }

                if (hourly.Length < 24)
                {
                    var temp = new double[24];
                    Array.Copy(hourly, temp, hourly.Length);
                    hourly = temp;
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
                            if (DateTime.TryParseExact(dateStr, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                            {
                                int dayIndex = date.Day - 1;
                                if (dayIndex >= 0 && dayIndex < daysInMonth)
                                {
                                    dailyEarnings[dayIndex] = GetValueSafe(item);
                                }
                            }
                        }
                    }
                }
                return dailyEarnings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fehler beim Parsen der Daily Earnings.");
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
                            if (DateTime.TryParseExact(dateStr, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                            {
                                int monthIndex = date.Month - 1; // 0-11
                                if (monthIndex >= 0 && monthIndex < 12)
                                {
                                    // Wir müssen hier aufaddieren, da die API vermutlich viele Einträge pro Monat liefert (jeden Tag)
                                    // und wir sie zu einem Monatsbalken zusammenfassen wollen.
                                    monthlyEarnings[monthIndex] += GetValueSafe(item);
                                }
                            }
                        }
                    }
                }
                return monthlyEarnings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fehler beim Parsen der Monthly Earnings.");
                return monthlyEarnings;
            }
        }

        // Helper um Value als Number oder String zu lesen
        private double GetValueSafe(JsonElement item)
        {
            if (item.TryGetProperty("value", out JsonElement valProp))
            {
                if (valProp.ValueKind == JsonValueKind.Number)
                    return valProp.GetDouble();
                else if (valProp.ValueKind == JsonValueKind.String &&
                         double.TryParse(valProp.GetString()?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double dVal))
                    return dVal;
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
}