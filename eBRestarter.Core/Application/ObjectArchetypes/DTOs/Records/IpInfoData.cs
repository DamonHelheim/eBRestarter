namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Represents public IP address and geolocation information.
/// </summary>
/// <param name="IpAddress">The public IP address.</param>
/// <param name="Hostname">The resolved network hostname.</param>
/// <param name="CountryCode">The two-letter ISO country code.</param>
/// <param name="CountryName">The full country name.</param>
public sealed record IpInfoData(
    string IpAddress,
    string Hostname,
    string CountryCode,
    string CountryName
);
