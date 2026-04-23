using System;
namespace eBRestarter.Desktop.WinUI3.Messages;

public record ComputerRestartDateChangedMessage(DateTime? NewDate);
