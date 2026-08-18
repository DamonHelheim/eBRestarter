using System;

namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.SignalDTO.Messages;

/// <summary>
/// Signal message published when the next scheduled computer restart date changes.
/// </summary>
/// <param name="NewDate">The updated target restart date and time, or <see langword="null"/> if disabled.</param>
public sealed record ComputerRestartDateChangedMessage(DateTime? NewDate);
