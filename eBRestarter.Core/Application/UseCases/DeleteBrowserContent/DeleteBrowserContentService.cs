using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Enums;

namespace eBRestarter.Core.Application.UseCases.DeleteBrowserContent;

public class DeleteBrowserContentService : IDeleteBrowserContentUseCase
{
    private readonly IBrowserFactory _browserFactory;
    private readonly IFileDeletionService _fileDeletionService;
    private readonly IWindowsProcessControlService _processService;
    private readonly ILocalizationService _localizationService;

    public DeleteBrowserContentService(
        IBrowserFactory browserFactory,
        IFileDeletionService fileDeletionService,
        IWindowsProcessControlService processService,
        ILocalizationService localizationService)
    {
        _browserFactory = browserFactory;
        _fileDeletionService = fileDeletionService;
        _processService = processService;
        _localizationService = localizationService;
    }

    public async Task<DeleteBrowserContentResponse> ExecuteAsync(
        DeleteBrowserContentRequest request,
        IProgress<DeleteBrowserContentProgress> progress,
        CancellationToken cancellationToken)
    {
        try
        {
            var browser = _browserFactory.Create(request.BrowserType);
            string processName = GetProcessNameByType(request.BrowserType);

            // If forced close is requested, do it. Otherwise just check if running.
            if (request.ForceCloseProcess)
            {
                progress.Report(new DeleteBrowserContentProgress(_localizationService.GetString("Cleanup_ClosingBrowser"), 0, 0));
                _processService.CloseApplication(processName);
                await Task.Delay(1000, cancellationToken);
            }

            if (_processService.IsProcessAlive(processName))
            {
                return new DeleteBrowserContentResponse(false, true, _localizationService.GetString("Cleanup_BrowserRunning"));
            }

            var browserPaths = browser.GetPaths();
            var directoriesToDelete = new List<string>();

            if (request.DeleteCache && browserPaths.CacheDirs != null && browserPaths.CacheDirs.Count > 0)
                directoriesToDelete.AddRange(browserPaths.CacheDirs);

            if (request.DeleteCookies && browserPaths.CookiesDirs != null && browserPaths.CookiesDirs.Count > 0)
                directoriesToDelete.AddRange(browserPaths.CookiesDirs);

            if (directoriesToDelete.Count == 0)
            {
                return new DeleteBrowserContentResponse(false, false, _localizationService.GetString("Cleanup_NoPaths"));
            }

            progress.Report(new DeleteBrowserContentProgress(_localizationService.GetString("Cleanup_Analyzing"), 0, 0));
            int totalFiles = await _fileDeletionService.CountFilesAsync(directoriesToDelete);

            var statusProgress = new Progress<string>(status =>
                progress.Report(new DeleteBrowserContentProgress(status, 0, totalFiles)));

            var fileProgress = new Progress<int>(count =>
                progress.Report(new DeleteBrowserContentProgress(_localizationService.GetString("Cleanup_Running") ?? "Lösche...", count, totalFiles)));

            await _fileDeletionService.DeleteFilesAsync(directoriesToDelete, statusProgress, fileProgress, cancellationToken);

            progress.Report(new DeleteBrowserContentProgress(_localizationService.GetString("Cleanup_Finished"), totalFiles, totalFiles));
            return new DeleteBrowserContentResponse(true, false, string.Empty);
        }
        catch (Exception ex)
        {
            return new DeleteBrowserContentResponse(false, false, ex.Message);
        }
    }

    private string GetProcessNameByType(BrowserType type)
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
