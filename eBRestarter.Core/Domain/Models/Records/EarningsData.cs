using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models.Records
{
    public record EarningsData(
        double[] HourlyEarnings, // Die 24 Balken
        double MonthlySum,
        double TodaySum
    );
}
