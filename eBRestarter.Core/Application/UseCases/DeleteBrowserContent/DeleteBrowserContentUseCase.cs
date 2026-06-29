using eBRestarter.Core.Application.Ports.Inbound.UseCases.DeleteBrowserContent;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Formatters;
using eBRestarter.Core.Application.Ports.Outbound;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using FluentResults;
using FluentValidation;
using eBRestarter.Core.Application.Ports.Outbound.Providers;
using eBRestarter.Core.Application.Providers;

namespace eBRestarter.Core.Application.UseCases.DeleteBrowserContent;

using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Errors;

public sealed class DeleteBrowserContentUseCase(
    IBrowserFactoryPort BrowserFactory,
    IFileDeletionPort fileDeletionService,
    IOsProcessControlPort processService,
    ILocalizationProvider LocalizationService,
    IValidator<DeleteBrowserContentRequest> validator) : IDeleteBrowserContentUseCase
{
    private readonly IBrowserFactoryPort _browserFactory = BrowserFactory;
    private readonly IFileDeletionPort _fileDeletionService = fileDeletionService;
    private readonly IOsProcessControlPort _processService = processService;
    private readonly ILocalizationProvider _localizationService = LocalizationService;
    private readonly IValidator<DeleteBrowserContentRequest> _validator = validator;

    public async Task<Result> ExecuteAsync(
        DeleteBrowserContentRequest request,
        IProgress<DeleteBrowserContentProgress> progress,
        CancellationToken cancel)
    {
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return Result.Fail(validationResult.Errors[0].ErrorMessage);
        }

        try
        {
            var browser = _browserFactory.Create(request.BrowserType);
            var processName = browser.ProcessName;

            if (request.ForceCloseProcess)
            {
                progress.Report(new DeleteBrowserContentProgress(_localizationService.RetrieveString("Cleanup_ClosingBrowser"), 0, 0));
                _processService.CloseApplication(processName);
                await Task.Delay(1000, cancel);
            }

            if (_processService.IsProcessAlive(processName))
            {
                return Result.Fail(new ProcessConflictError(_localizationService.RetrieveString("Cleanup_BrowserRunning") ?? "Browser is running"));
            }

            var browserPaths = browser.ResolvePaths();
            var directoriesToDelete = new List<string>();

            if (request.DeleteCache && browserPaths.CacheDirs?.Count > 0)
            {
                directoriesToDelete.AddRange(browserPaths.CacheDirs);
            }

            if (request.DeleteCookies && browserPaths.CookiesDirs?.Count > 0)
            {
                directoriesToDelete.AddRange(browserPaths.CookiesDirs);
            }

            if (directoriesToDelete.Count == 0)
            {
                return Result.Fail(_localizationService.RetrieveString("Cleanup_NoPaths") ?? "No paths to clean");
            }

            progress.Report(new DeleteBrowserContentProgress(_localizationService.RetrieveString("Cleanup_Analyzing"), 0, 0));

            int totalFiles = await _fileDeletionService.CountFilesAsync(directoriesToDelete);

            var statusProgress = new Progress<string>(status =>
                progress.Report(new DeleteBrowserContentProgress(status, 0, totalFiles)));

            var fileProgress = new Progress<int>(count =>
                progress.Report(new DeleteBrowserContentProgress(_localizationService.RetrieveString("Cleanup_Running") ?? "Delete...", count, totalFiles)));

            await _fileDeletionService.DeleteFilesAsync(directoriesToDelete, statusProgress, fileProgress, cancel);

            progress.Report(new DeleteBrowserContentProgress(_localizationService.RetrieveString("Cleanup_Finished"), totalFiles, totalFiles));

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }
}








