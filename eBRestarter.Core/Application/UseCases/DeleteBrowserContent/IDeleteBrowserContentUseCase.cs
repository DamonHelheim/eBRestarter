using System;
using System.Threading;
using System.Threading.Tasks;

namespace eBRestarter.Core.Application.UseCases.DeleteBrowserContent;

public interface IDeleteBrowserContentUseCase
{
    Task<DeleteBrowserContentResponse> ExecuteAsync(
        DeleteBrowserContentRequest request,
        IProgress<DeleteBrowserContentProgress> progress,
        CancellationToken cancellationToken);
}
