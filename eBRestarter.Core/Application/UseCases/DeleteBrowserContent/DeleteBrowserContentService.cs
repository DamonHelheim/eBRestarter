using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using FluentResults;
using FluentValidation;

namespace eBRestarter.Core.Application.UseCases.DeleteBrowserContent;

public class DeleteBrowserContentService(
    IBrowserFactory browserFactory,
    IFileDeletionService fileDeletionService,
    IWindowsProcessControlService processService,
    ILocalizationService localizationService,
    IValidator<DeleteBrowserContentRequest> validator) : IDeleteBrowserContentUseCase
{
    private readonly IBrowserFactory _browserFactory = browserFactory;
    private readonly IFileDeletionService _fileDeletionService = fileDeletionService;
    private readonly IWindowsProcessControlService _processService = processService;
    private readonly ILocalizationService _localizationService = localizationService;
    private readonly IValidator<DeleteBrowserContentRequest> _validator = validator;

    public async Task<Result> ExecuteAsync(
        DeleteBrowserContentRequest request,
        IProgress<DeleteBrowserContentProgress> progress,
        CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return Result.Fail(validationResult.Errors[0].ErrorMessage);
        }

        try
        {
            var browser = _browserFactory.Create(request.BrowserType);
            string processName = GetProcessNameByType(request.BrowserType);

            if (request.ForceCloseProcess)
            {
                progress.Report(new DeleteBrowserContentProgress(_localizationService.GetString("Cleanup_ClosingBrowser"), 0, 0));
                _processService.CloseApplication(processName);
                await Task.Delay(1000, cancellationToken);
            }

            if (_processService.IsProcessAlive(processName))
            {
                return Result.Fail(new ProcessConflictError(_localizationService.GetString("Cleanup_BrowserRunning") ?? "Browser is running"));
            }

            var browserPaths = browser.GetPaths();
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
                return Result.Fail(_localizationService.GetString("Cleanup_NoPaths") ?? "No paths to clean");
            }

            progress.Report(new DeleteBrowserContentProgress(_localizationService.GetString("Cleanup_Analyzing"), 0, 0));

            int totalFiles = await _fileDeletionService.CountFilesAsync(directoriesToDelete);

            var statusProgress = new Progress<string>(status =>
                progress.Report(new DeleteBrowserContentProgress(status, 0, totalFiles)));

            var fileProgress = new Progress<int>(count =>
                progress.Report(new DeleteBrowserContentProgress(_localizationService.GetString("Cleanup_Running") ?? "Delete...", count, totalFiles)));

            await _fileDeletionService.DeleteFilesAsync(directoriesToDelete, statusProgress, fileProgress, cancellationToken);

            progress.Report(new DeleteBrowserContentProgress(_localizationService.GetString("Cleanup_Finished"), totalFiles, totalFiles));

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    private static string GetProcessNameByType(BrowserType type)
    {
        return type switch
        {
            BrowserType.Chrome => "chrome",
            BrowserType.Firefox => "firefox",
            BrowserType.Edge => "msedge",
            BrowserType.Brave => "brave",
            BrowserType.Vivaldi => "vivaldi",
            _ => string.Empty
        };
    }
}
