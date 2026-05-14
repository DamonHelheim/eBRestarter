using FluentResults;

namespace eBRestarter.Core.Application.UseCases.DeleteBrowserContent;

public interface IDeleteBrowserContentUseCase
{
    Task<Result> ExecuteAsync(
        DeleteBrowserContentRequest request,
        IProgress<DeleteBrowserContentProgress> progress,
        CancellationToken cancel);
}
