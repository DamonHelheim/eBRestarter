using FluentResults;

namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.DeleteBrowserContent;

using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Errors;

public interface IDeleteBrowserContentUseCase
{
    Task<Result> ExecuteAsync(
        DeleteBrowserContentRequest request,
        IProgress<DeleteBrowserContentProgress> progress,
        CancellationToken cancel);
}

