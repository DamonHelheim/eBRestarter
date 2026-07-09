namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public sealed record IpInfoData(
    string IpAddress,
    string Hostname,
    string CountryCode,
    string CountryName
);
