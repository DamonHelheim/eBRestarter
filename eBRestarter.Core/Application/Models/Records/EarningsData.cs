namespace eBRestarter.Core.Application.Models.Records;

/// <summary>
/// Represents the retrieved earnings data from the API.
/// </summary>
/// <param name="HourlyEarnings">Array with 24 values for the hours of the current day.</param>
/// <param name="DailyEarnings">Array with values for each day of the current month.</param>
/// <param name="MonthlyEarnings">Array with 12 values for the months of the current year.</param>
/// <param name="YearlySum">The total sum for the current year.</param>
/// <param name="MonthlySum">The total sum for the current month.</param>
/// <param name="TodaySum">The total sum for today.</param>
public sealed record EarningsData(
    double[] HourlyEarnings, // The 24 bars
    double[] DailyEarnings,
    double[] MonthlyEarnings,
    double YearlySum,
    double MonthlySum,
    double TodaySum
);