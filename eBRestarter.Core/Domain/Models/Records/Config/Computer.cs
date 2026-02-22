namespace eBRestarter.Core.Domain.Models.Records.Config;

public record Computer
{
    // Das Fragezeichen macht es nullable -> Es kann "null" sein
    public DateTime? NextRestartDate { get; set; }
    public int ComputerRestartIntervalDays { get; set; } = 0;
    public int RestartClockTime { get; set; } = 0;
}
