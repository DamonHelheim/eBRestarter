namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public record struct DeleteBrowserContentProgress(string StatusMessage, int CurrentFile, int TotalFiles);

