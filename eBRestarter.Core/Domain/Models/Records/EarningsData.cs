namespace eBRestarter.Core.Domain.Models.Records;

/// <summary>
/// Repräsentiert die abgerufenen Verdienstdaten der API.
/// </summary>
/// <param name="HourlyEarnings">Array mit 24 Werten für die Stunden des aktuellen Tages.</param>
/// <param name="DailyEarnings">Array mit den Werten für jeden Tag des aktuellen Monats.</param>
/// <param name="MonthlyEarnings">Array mit 12 Werten für die Monate des aktuellen Jahres.</param>
/// <param name="YearlySum">Die Gesamtsumme des aktuellen Jahres.</param>
/// <param name="MonthlySum">Die Gesamtsumme des aktuellen Monats.</param>
/// <param name="TodaySum">Die Gesamtsumme des heutigen Tages.</param>
public record EarningsData(
    double[] HourlyEarnings, // Die 24 Balken
    double[] DailyEarnings,
    double[] MonthlyEarnings,
    double YearlySum,
    double MonthlySum,
    double TodaySum
);
