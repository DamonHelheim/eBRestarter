using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.RestClient;
using eBRestarter.Core.Domain.Models;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Infrastructure.Constants;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace eBRestarter.Infrastructure.Services
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
                // KORREKTUR: System.Text.Json Parsing
                using var doc = JsonDocument.Parse(response.Content);
                var root = doc.RootElement;

                // Safe access helper (siehe unten) oder direkt zugreifen
                return new IpInfoData(
                    IpAddress: GetStringSafe(root, "ip"),
                    Hostname: GetStringSafe(root, "host"),
                    CountryCode: GetStringSafe(root, "country_code"),
                    CountryName: GetStringSafe(root, "country_name")
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

            var request = new ApiRequest
            {
                Url = ApiWebLinks.HourlyEarnings,
                Method = Core.Domain.Enums.HttpMethod.GET,
                Username = config.Settings.ApiUsername,
                Password = config.Settings.ApiKey
            };

            var response = await _restClient.ExecuteGetAsync(request);

            if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
                return null;

            try
            {
                // KORREKTUR: System.Text.Json Array Parsing
                using var doc = JsonDocument.Parse(response.Content);
                var root = doc.RootElement;

                double[] hourly;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    // Konvertiert jedes Element im JSON-Array zu double
                    hourly = root.EnumerateArray()
                                 .Select(element =>
                                 {
                                     // Robustes Parsing: Versucht Double, sonst 0
                                     return element.TryGetDouble(out double val) ? val : 0.0;
                                 })
                                 .ToArray();
                }
                else
                {
                    // Fallback, falls kein Array zurückkommt
                    hourly = new double[24];
                }

                // Falls das Array kleiner als 24 ist, auffüllen
                if (hourly.Length < 24)
                {
                    var temp = new double[24];
                    Array.Copy(hourly, temp, hourly.Length);
                    hourly = temp;
                }

                double todaySum = hourly.Sum();

                return new EarningsData(hourly, 0, todaySum);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Parsen der Earnings");
                return null;
            }
        }

        // Kleiner Helper für robusteres JSON-Lesen (vermeidet Exceptions bei fehlenden Properties)
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
