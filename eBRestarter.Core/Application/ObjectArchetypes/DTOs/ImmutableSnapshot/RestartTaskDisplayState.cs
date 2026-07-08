namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.ImmutableSnapshot;

public sealed class RestartTaskDisplayState
{
    public string Username { get; init; } = "-";
    public string ChosenBrowser { get; init; } = string.Empty;
    public int RuntimeSeconds { get; init; }
    public int PauseSeconds { get; init; }
    public bool DeleteBrowserContentIsActive { get; init; }
    public string DeleteIsActivatedMessage { get; init; } = string.Empty;
    public string NextDeletionProcessMessage { get; init; } = string.Empty;
    public string NextDeletionProcessDateMessage { get; init; } = string.Empty;
    public bool CheckBrowserAliveRoutine { get; init; }

}
