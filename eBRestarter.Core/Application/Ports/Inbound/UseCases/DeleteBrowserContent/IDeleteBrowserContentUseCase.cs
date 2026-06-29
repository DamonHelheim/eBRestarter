using FluentResults;
using eBRestarter.Core.Application.Models.Records;
namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.DeleteBrowserContent;

public interface IDeleteBrowserContentUseCase
{
    Task<Result> ExecuteAsync(
        DeleteBrowserContentRequest request,
        IProgress<DeleteBrowserContentProgress> progress,
        CancellationToken cancel);
}

