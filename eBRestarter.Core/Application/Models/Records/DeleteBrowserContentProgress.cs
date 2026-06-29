namespace eBRestarter.Core.Application.Models.Records;

public record struct DeleteBrowserContentProgress(string StatusMessage, int CurrentFile, int TotalFiles);

