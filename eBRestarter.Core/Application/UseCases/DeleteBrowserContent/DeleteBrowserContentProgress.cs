namespace eBRestarter.Core.Application.UseCases.DeleteBrowserContent;

public record struct DeleteBrowserContentProgress(string StatusMessage, int CurrentFile, int TotalFiles);
