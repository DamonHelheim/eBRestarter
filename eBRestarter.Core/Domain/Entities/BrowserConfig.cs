using System;

namespace eBRestarter.Core.Domain.Entities;

public class BrowserConfig
{
    public bool StartBrowserWithProgrammStart { get; set; }
    public bool CheckBrowserAliveRoutine { get; set; }
    public string Selected { get; set; } = string.Empty;
    public int RuntimeHours { get; set; } = 1;
    public int RuntimePauseSeconds { get; set; } = 20;
    public int DeleteBrowserCacheIntervalDays { get; set; }
    public DateTime NextBrowserDeleteCacheDate { get; set; }

    public BrowserConfig() { }

    public void UpdateCleanupSettings(int intervalDays, TimeProvider timeProvider)
    {
        DeleteBrowserCacheIntervalDays = intervalDays;
        CalculateNextCleanupDate(timeProvider);
    }

    public void CalculateNextCleanupDate(TimeProvider timeProvider)
    {
        if (DeleteBrowserCacheIntervalDays > 0)
        {
            NextBrowserDeleteCacheDate = timeProvider.GetLocalNow().Date.AddDays(DeleteBrowserCacheIntervalDays);
        }
        else
        {
            NextBrowserDeleteCacheDate = DateTime.MaxValue;
        }
    }

    public void SetNextCleanupDate(DateTime date)
    {
        NextBrowserDeleteCacheDate = date;
    }
}
