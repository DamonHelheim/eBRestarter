using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Parsers;

public interface IEVisitorApiResponseParser
{
    IpInfoData? ParseIpInfo(string jsonContent);
    double[] ParseHourlyEarnings(string jsonContent);
    double[] ParseDailyEarnings(string jsonContent, int daysInMonth);
    double[] ParseMonthlyEarnings(string jsonContent);
}


