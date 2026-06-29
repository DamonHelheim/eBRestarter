namespace eBRestarter.Core.Application.Models.Records;

public sealed record IpInfoData(
    string IpAddress,
    string Hostname,
    string CountryCode,
    string CountryName
);
