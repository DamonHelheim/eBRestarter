using System;

namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.SignalDTO.Messages;

public sealed record ComputerRestartDateChangedMessage(DateTime? NewDate);
