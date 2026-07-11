namespace eBRestarter.Core.Domain.ValueObjects;

public sealed record Computer
{
    public DateTime? NextRestartDate { get; set; }
    public int ComputerRestartIntervalDays { get; set; }
    public int RestartClockTime { get; set; }

    public Computer() { }

    public Computer(DateTime? nextRestartDate, int intervalDays, int restartClockTime)
    {
        NextRestartDate = nextRestartDate;
        ComputerRestartIntervalDays = intervalDays;
        RestartClockTime = restartClockTime;
    }

    public void UpdateRestartSettings(int intervalDays, int restartClockTime, TimeProvider timeProvider)
    {
        ComputerRestartIntervalDays = intervalDays;
        RestartClockTime = restartClockTime;
        CalculateNextRestartDate(timeProvider);
    }

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

    public void SetNextRestartDate(DateTime? date)
    {
        NextRestartDate = date;
    }
}
