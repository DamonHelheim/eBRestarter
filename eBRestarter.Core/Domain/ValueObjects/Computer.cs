namespace eBRestarter.Core.Domain.ValueObjects;

/// <summary>
/// Represents host computer restart schedule settings and calculation state.
/// </summary>
public sealed record Computer
{
    /// <summary>
    /// Gets or sets the scheduled next date and time for computer restart.
    /// </summary>
    public DateTime? NextRestartDate { get; set; }

    /// <summary>
    /// Gets or sets the interval in days between computer restarts (0 = disabled).
    /// </summary>
    public int ComputerRestartIntervalDays { get; set; }

    /// <summary>
    /// Gets or sets the clock time hour (0–23) at which the computer restart occurs.
    /// </summary>
    public int RestartClockTime { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Computer"/> record with default values.
    /// </summary>
    public Computer() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Computer"/> record with specified restart parameters.
    /// </summary>
    /// <param name="nextRestartDate">The scheduled next restart date and time.</param>
    /// <param name="intervalDays">The interval in days between restarts.</param>
    /// <param name="restartClockTime">The clock time hour for restarts.</param>
    public Computer(DateTime? nextRestartDate, int intervalDays, int restartClockTime)
    {
        NextRestartDate = nextRestartDate;
        ComputerRestartIntervalDays = intervalDays;
        RestartClockTime = restartClockTime;
    }

    /// <summary>
    /// Updates restart schedule settings and recalculates the next restart date.
    /// </summary>
    /// <param name="intervalDays">The new restart interval in days.</param>
    /// <param name="restartClockTime">The new restart clock time hour.</param>
    /// <param name="timeProvider">Time provider for calculating local time.</param>
    public void UpdateRestartSettings(int intervalDays, int restartClockTime, TimeProvider timeProvider)
    {
        ComputerRestartIntervalDays = intervalDays;
        RestartClockTime = restartClockTime;
        CalculateNextRestartDate(timeProvider);
    }

    /// <summary>
    /// Calculates the next scheduled computer restart date based on current interval and clock time settings.
    /// </summary>
    /// <param name="timeProvider">Time provider for evaluating local date and time.</param>
    public void CalculateNextRestartDate(TimeProvider timeProvider)
    {
        if (ComputerRestartIntervalDays > 0)
        {
            NextRestartDate = timeProvider.GetLocalNow().Date.AddDays(ComputerRestartIntervalDays).AddHours(RestartClockTime);
        }
        else
        {
            NextRestartDate = null;
        }
    }

    /// <summary>
    /// Explicitly sets the next scheduled computer restart date.
    /// </summary>
    /// <param name="date">The new next restart date.</param>
    public void SetNextRestartDate(DateTime? date)
    {
        NextRestartDate = date;
    }
}
