namespace eBRestarter.Core.Domain.Models.Records;

// Record für IP Infos
public record IpInfoData(
    string IpAddress,
    string Hostname,
    string CountryCode,
    string CountryName
);
