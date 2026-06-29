using System;

namespace eBRestarter.Desktop.WinUI3.Messages;

public sealed record ComputerRestartDateChangedMessage(DateTime? NewDate);
