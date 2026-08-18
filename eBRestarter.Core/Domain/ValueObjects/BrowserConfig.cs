namespace eBRestarter.Core.Domain.ValueObjects;

/// <summary>
/// Represents browser-specific execution and cleanup configuration settings.
/// </summary>
public sealed record BrowserConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether to start the browser automatically when the application starts.
    /// </summary>
    public bool StartBrowserWithProgrammStart { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the routine checking browser process liveness is enabled.
    /// </summary>
    public bool CheckBrowserAliveRoutine { get; set; }

    /// <summary>
    /// Gets or sets the key or type string of the selected browser application.
    /// </summary>
    public string Selected { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target runtime duration in hours for browser execution runs.
    /// </summary>
    public int RuntimeHours { get; set; } = 1;

    /// <summary>
    /// Gets or sets the pause duration in seconds between browser execution runs.
    /// </summary>
    public int RuntimePauseSeconds { get; set; } = 20;

    /// <summary>
    /// Gets or sets the interval in days for scheduled browser cache deletion.
    /// </summary>
    public int DeleteBrowserCacheIntervalDays { get; set; }

    /// <summary>
    /// Gets or sets the scheduled next date for browser cache cleanup execution.
    /// </summary>
    public DateTime NextBrowserDeleteCacheDate { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserConfig"/> record with default values.
    /// </summary>
    public BrowserConfig() { }

    /// <summary>
    /// Updates the browser cache cleanup interval and recalculates the next cleanup date.
    /// </summary>
    /// <param name="intervalDays">The new cleanup interval in days.</param>
    /// <param name="timeProvider">Time provider for calculating local time.</param>
    public void UpdateCleanupSettings(int intervalDays, TimeProvider timeProvider)
    {
        DeleteBrowserCacheIntervalDays = intervalDays;
        CalculateNextCleanupDate(timeProvider);
    }

    /// <summary>
    /// Calculates the next scheduled browser cache cleanup date based on the current interval setting.
    /// </summary>
    /// <param name="timeProvider">Time provider for evaluating local date and time.</param>
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

    /// <summary>
    /// Explicitly sets the next scheduled browser cache cleanup date.
    /// </summary>
    /// <param name="date">The new next cleanup date.</param>
    public void SetNextCleanupDate(DateTime date)
    {
        NextBrowserDeleteCacheDate = date;
    }
}
