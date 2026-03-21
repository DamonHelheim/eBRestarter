using System;

namespace eBRestarter.Core.Domain.Models.Records;

public record ComputerRestartDateChangedMessage(DateTime? NewDate);
